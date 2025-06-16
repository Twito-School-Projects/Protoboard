using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class WireMaker : Singleton<WireMaker>
{
    public Battery battery;
    public Breadboard breadboard;

    [SerializeField] private InputActionAsset inputActions;

    private InputAction mouseClick;
    private InputAction cancelAction;

    [SerializeField] private GameObject wirePrefab;

    private Camera mainCamera;

    [SerializeField] private bool isMakingWire = false;

    [SerializeField] private ConnectionPoint startConnectionPoint = null;
    [SerializeField] private ConnectionPoint endConnectionPoint = null;
    [SerializeField] private ConnectionPoint highlightedConnectPoint;

    [SerializeField] private Transform wiresContainer;

    private int numberOfWires = 0;

    public static event Action<Hole> WireCreationCancelled;
    public static event Action<Hole> WireCreationStarted;
    public static event Action<Hole, Hole> WireCreationEnded;
    
    public LayerMask breadboardLayerMask;
    
    private void OnEnable()
    {
        var map = inputActions.FindActionMap("Player");
        mouseClick = map.FindAction("Click");
        cancelAction = inputActions.FindAction("UI/Cancel");
        
        cancelAction.Enable();
        mouseClick.Enable();
        
        mouseClick.performed += MousePressed;
        cancelAction.performed += CancelActionOnPerformed;
        
        WireCreationStarted += OnWireCreationStarted;
        WireCreationEnded += OnWireCreationEnded;
        WireCreationCancelled += OnWireCreationCancelled;
    }

    private void CancelActionOnPerformed(InputAction.CallbackContext obj)
    {
        if (!isMakingWire)
            return;
        
        WireCreationCancelled?.Invoke(null);
    }


    private void OnDisable()
    {
        mouseClick.performed -= MousePressed;
        cancelAction.performed -= CancelActionOnPerformed;
        
        mouseClick.Disable();
        cancelAction.Disable();

        inputActions.FindActionMap("Player").Disable();
        
        WireCreationStarted -= OnWireCreationStarted;
        WireCreationEnded -= OnWireCreationEnded;
        WireCreationCancelled -= OnWireCreationCancelled;

    }
    
    private void OnWireCreationCancelled(Hole _)
    {
        startConnectionPoint = null;
        endConnectionPoint = null;
        isMakingWire = false;
    }

    
    private void OnWireCreationEnded(Hole _, Hole _2)
    {
        startConnectionPoint = null;
        endConnectionPoint = null;
        isMakingWire = false;
        numberOfWires++;
    }

    private void OnWireCreationStarted(Hole _)
    {
    }

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (highlightedConnectPoint)
        {
            highlightedConnectPoint.Highlight();
        }

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        if (ComponentPlacementSystem.Instance.CurrentState != PlacementState.Idle)
            return;
        
        if (Physics.Raycast(ray, out hit, breadboardLayerMask))
        {
            if (hit.collider && hit.collider.gameObject.CompareTag("ConnectionPoint"))
            {
                if (highlightedConnectPoint && highlightedConnectPoint.gameObject.name != hit.collider.gameObject.name)
                {
                    highlightedConnectPoint.RemoveHighlight();
                    highlightedConnectPoint = null;

                    return;
                }

                highlightedConnectPoint = hit.collider.gameObject.GetComponent<ConnectionPoint>();
                highlightedConnectPoint.Highlight();
            }
            else
            {
                highlightedConnectPoint?.RemoveHighlight();
            }
        }
    }

    private void MousePressed(InputAction.CallbackContext context)
    {
        Debug.Log("Wants to create");
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        
        //do nott allow wire creation if the placement system is in use
        if (ComponentPlacementSystem.Instance.CurrentState != PlacementState.Idle)
        {
            return;
        }
        if (Physics.Raycast(ray, out hit, breadboardLayerMask))
        {
            if (hit.collider && hit.collider.gameObject.CompareTag("ConnectionPoint"))
            {
                if (!isMakingWire)
                {
                    if (StartWireCreator(hit.collider.gameObject, startConnectionPoint))
                        WireCreationStarted?.Invoke(startConnectionPoint as Hole);
                }
                else
                {
                    if (EndWireCreator(hit.collider.gameObject, endConnectionPoint))
                        WireCreationEnded?.Invoke(startConnectionPoint as Hole, endConnectionPoint as Hole);
                }
            }
        }
    }

    private bool StartWireCreator(GameObject clickedGameObject, ConnectionPoint sPoint)
    {
        if (isMakingWire)
            return false;
       
        //If we're clicking the same hole
        if (sPoint && clickedGameObject.name == sPoint.gameObject.name)
        {
            WireCreationCancelled?.Invoke(startConnectionPoint as Hole);
            //sPoint = null;
            Debug.Log("Clicked the same hole.");
            //isMakingWire = false;
            return false;
        }

        clickedGameObject.TryGetComponent(out startConnectionPoint);

        //if the point already has a wire
        if (startConnectionPoint.isTaken)
        {
            WireCreationCancelled?.Invoke(startConnectionPoint as Hole);
            Debug.Log("This hole is already taken, cancelling wire creation.");
            return false;
        }
        
        isMakingWire = true;
        startConnectionPoint.Highlight(true);

        Debug.Log("Creating wire from hole: " + clickedGameObject.name);
        return true;
    }

    private bool EndWireCreator(GameObject clickedGameObject, ConnectionPoint ePoint)
    {
        if (!isMakingWire)
            return false;

        //If the end hole is the same as the start hole
        if (clickedGameObject.name == startConnectionPoint.gameObject.name)
        {
            Debug.Log("Tried to use the start point as the end point");
            WireCreationCancelled?.Invoke(null);
            return false;
        }

        clickedGameObject.TryGetComponent(out endConnectionPoint);

        if (endConnectionPoint.isTaken)
        {
            Debug.Log("This hole is already taken, choose another.");
            WireCreationCancelled?.Invoke(endConnectionPoint as Hole);
            return false;
        }

        if (!startConnectionPoint.powered && !endConnectionPoint.powered)
        {
            Debug.Log("The starting point must be powered, cancelling wire creation");
            WireCreationCancelled?.Invoke(endConnectionPoint as Hole);
            return false;
        }

        if (!ValidateConnectionPoints())
        {
            Debug.Log("Invalid connection points, cancelling wire creation");
            WireCreationCancelled?.Invoke(null);
            return false;
        }

        if (!HandleCircuitConnection(out var parentPoint, out var childPoint))
        {
            Debug.Log("Failed to handle circuit connection, cancelling wire creation");
            WireCreationCancelled?.Invoke(endConnectionPoint as Hole);
            return false;
        }
        
        Wire wireComponent = CreateWireBetweenTwoPoints(parentPoint, childPoint);
        Debug.Log("Ending wire from hole: " + clickedGameObject.name);
        

        return true;
    }

    private bool HandleCircuitConnection(out ConnectionPoint parentPoint, out ConnectionPoint childPoint)
    {
        parentPoint = startConnectionPoint;
        childPoint = endConnectionPoint;
        
        if (!DetermineParentPoint(ref parentPoint, ref childPoint)) return false;
        //there are already some connections
        var possibleTree = CircuitManager.Instance.GetTree(parentPoint as Hole);
        if (possibleTree == null)
        {
            
        }
        else if (possibleTree.IsEmpty)
        {
            
        }

        var parentNode = CircuitManager.Instance.FindNodeInTree(TreeType.Battery, parentPoint);
        var possibleDisconnectedChildNode = CircuitManager.Instance.FindNodeInTree(TreeType.DisconnectedBattery, childPoint);

        if (parentNode == null)
        {
            WireCreationCancelled?.Invoke(null);
            Debug.Log("Parent node is null SOMETHING IS VERY WRONG");
            return false;
        }

        if (!(parentPoint as Hole).IsPositiveRail && parentNode.Parent == null)
        {
            WireCreationCancelled?.Invoke(null);
            Debug.Log("Parent is not positive rail and has no parent, something is very wrong");
            return false;
        }
        
        CircuitManager.Instance.PropagatePower(parentNode);
        
        //if the child node is by itself
        if (possibleDisconnectedChildNode == null)
        {
            var childNode = CircuitManager.Instance.AddChild(ref parentNode, ref childPoint);
            
            if (childPoint.type == ConnectionPointType.Rail && childPoint.charge == Charge.Negative)
            {
                CircuitManager.Instance.PropagateUp(childNode);
                Debug.Log("Complete circuit" % Colorize.Green);
            }
        } else //if the child node is its own circuit tree that was previously disconnected
        {
            CircuitManager.Instance.ReconnectTree(parentPoint, childPoint, TreeType.Battery);
            Debug.Log("Reconnected to power" % Colorize.Blue);
        }
        
        return true;
    }

    private bool DetermineParentPoint(ref ConnectionPoint parentPoint, ref ConnectionPoint childPoint)
    {
        if (startConnectionPoint.powered)
        {
            parentPoint = startConnectionPoint;
            childPoint = endConnectionPoint;
            return true;
        }
        if (endConnectionPoint.powered)
        {
            parentPoint = endConnectionPoint;
            childPoint = startConnectionPoint;
            return true;
        }
        
        return false;
    }

    private bool ValidateConnectionPoints()
    {
        //battery to battery should not work
        if (startConnectionPoint.type == ConnectionPointType.Battery && endConnectionPoint.type == ConnectionPointType.Battery)
        {
            return false;
        }

        //battery to terminal should not work
        if (startConnectionPoint.type == ConnectionPointType.Battery && endConnectionPoint.type == ConnectionPointType.Terminal || endConnectionPoint.type == ConnectionPointType.Battery && startConnectionPoint.type == ConnectionPointType.Terminal)
        {
            return false;
        }

        var s = startConnectionPoint as Hole;
        var e = endConnectionPoint as Hole;

        if (s.IsNegativeRail && e.IsNegativeRail)
        {
            return false;
        }

        return true;
    }

    public Wire CreateWireBetweenTwoPoints(ConnectionPoint source, ConnectionPoint target)
    {
        Vector3 start = source.transform.position;
        Vector3 end = target.transform.position;

        float distance = Vector3.Distance(start, end);
        Vector3 midPoint = (start + end) / 2;
        Quaternion rotation = Quaternion.LookRotation(end - start, Vector3.up);

        midPoint.y += numberOfWires * wirePrefab.transform.localScale.y;

        var wire = Instantiate(wirePrefab, midPoint, rotation, wiresContainer);
        wire.name = "Wire " + numberOfWires;
        var wireComponent = wire.GetComponent<Wire>();

        wire.transform.localScale = new Vector3(
            wire.transform.localScale.x,
            wire.transform.localScale.y,
            distance
        );

        wireComponent.OnCreated(source, target);
        wireComponent.ChangeToRandomColour();

        return wireComponent;
    }
}