using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using static Unity.VisualScripting.Member;
using static UnityEngine.GraphicsBuffer;

public class WireMaker : Singleton<WireMaker>
{
    public Battery battery;
    public Breadboard breadboard;

    [SerializeField] private InputActionAsset inputActions;

    private InputAction mouseClick;

    [SerializeField] private GameObject wirePrefab;

    private Camera mainCamera;

    [SerializeField] private bool isMakingWire = false;

    [SerializeField] private ConnectionPoint startConnectionPoint = null;
    [SerializeField] private ConnectionPoint endConnectionPoint = null;
    [SerializeField] private ConnectionPoint highlightedConnectPoint;

    [SerializeField] private Transform wiresContainer;

    private int numberOfWires = 0;

    public static event Action WireCreationCancelled;
    public static event Action WireCreationStarted;
    public static event Action WireCreationEnded;
    
    public LayerMask breadboardLayerMask;
    
    private void OnEnable()
    {
        var map = inputActions.FindActionMap("Player");
        mouseClick = map.FindAction("Click");

        mouseClick.Enable();
        mouseClick.performed += MousePressed;
        
        WireCreationStarted += OnWireCreationStarted;
        WireCreationEnded += OnWireCreationEnded;
    }



    private void OnDisable()
    {
        mouseClick.Disable();
        mouseClick.performed -= MousePressed;

        inputActions.FindActionMap("Player").Disable();
        
        WireCreationStarted -= OnWireCreationStarted;
        WireCreationEnded -= OnWireCreationEnded;
    }
    
    private void OnWireCreationEnded()
    {
    }

    private void OnWireCreationStarted()
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
                    StartWireCreator(hit.collider.gameObject, startConnectionPoint);
                    WireCreationStarted?.Invoke();
                }
                else
                {
                    EndWireCreator(hit.collider.gameObject, endConnectionPoint);
                    WireCreationEnded?.Invoke();
                }
            }
        }
    }

    private void StartWireCreator(GameObject clickedGameObject, ConnectionPoint sPoint)
    {
        if (isMakingWire)
            return;
       
        //If we're clicking the same hole
        if (sPoint && clickedGameObject.name == sPoint.gameObject.name)
        {
            WireCreationCancelled?.Invoke();
            //sPoint = null;
            Debug.Log("Clicked the same hole.");
            //isMakingWire = false;
            return;
        }

        clickedGameObject.TryGetComponent(out startConnectionPoint);

        //if the point already has a wire
        if (startConnectionPoint.isTaken)
        {
            WireCreationCancelled?.Invoke();
            startConnectionPoint = null;
            isMakingWire = false;
            Debug.Log("This hole is already taken, cancelling wire creation.");
            return;
        }
        
        isMakingWire = true;
        startConnectionPoint.Highlight(true);

        Debug.Log("Creating wire from hole: " + clickedGameObject.name);
    }

    private void EndWireCreator(GameObject clickedGameObject, ConnectionPoint ePoint)
    {
        if (!isMakingWire)
            return;

        //If the end hole is the same as the start hole
        if (clickedGameObject.name == startConnectionPoint.gameObject.name)
        {
            Debug.Log("Tried to use the start point as the end point");
            return;
        }

        clickedGameObject.TryGetComponent(out endConnectionPoint);

        if (endConnectionPoint.isTaken)
        {
            Debug.Log("This hole is already taken, choose another.");
            endConnectionPoint = null;
            return;
        }

        if (!startConnectionPoint.powered && !endConnectionPoint.powered)
        {
            Debug.Log("The starting point must be powered, cancelling wire creation");
            WireCreationCancelled?.Invoke();

            startConnectionPoint = null;
            endConnectionPoint = null;
            isMakingWire = false;
           
            return;
        }

        if (!ValidateConnectionPoints())
        {
            Debug.Log("Invalid connection points, cancelling wire creation");
            return;
        }

        if (!HandleCircuitConnection(out var parentPoint, out var childPoint))
        {
            Debug.Log("Failed to handle circuit connection, cancelling wire creation");
            isMakingWire = false;
            startConnectionPoint = null;
            endConnectionPoint = null;
            
            return;
        }
        Wire wireComponent = CreateWireBetweenTwoPoints(parentPoint, childPoint);
        Debug.Log("Ending wire from hole: " + clickedGameObject.name);
        
        startConnectionPoint = null;
        endConnectionPoint = null;
        isMakingWire = false;
        numberOfWires++;
    }

    private bool HandleCircuitConnection(out ConnectionPoint parentPoint, out ConnectionPoint childPoint)
    {
        parentPoint = startConnectionPoint;
        childPoint = endConnectionPoint;
        
        //there are already some connections
        var possibleTree = CircuitManager.Instance.GetTree(parentPoint as Hole);
        if (possibleTree == null)
        {
            
        }
        else if (possibleTree.IsEmpty)
        {
            
        }
        if (DetermineParentPoint(ref parentPoint, ref childPoint)) return false;

        var parentNode = CircuitManager.Instance.FindNodeInTree(TreeType.Battery, parentPoint);
        var possibleDisconnectedChildNode = CircuitManager.Instance.FindNodeInTree(TreeType.DisconnectedBattery, childPoint);

        if (parentNode == null)
        {
            throw new Exception("Parent node is null SOMETHING IS VERY WRONG");
        }

        if (!(parentPoint as Hole).IsPositiveRail && parentNode.Parent == null)
        {
            throw new Exception("Parent is not positive rail and has no parent, something is very wrong");
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
        }
        else if (endConnectionPoint.powered)
        {
            parentPoint = endConnectionPoint;
            childPoint = startConnectionPoint;
        }
        else
        {
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