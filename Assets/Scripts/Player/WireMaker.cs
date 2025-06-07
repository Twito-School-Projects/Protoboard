using System;
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

    private void OnEnable()
    {
        inputActions.FindActionMap("Player").Enable();
        mouseClick = InputSystem.actions.FindAction("StartWireCreation");

        mouseClick.Enable();
        mouseClick.performed += MousePressed;
    }

    private void OnDisable()
    {
        mouseClick.Disable();
        mouseClick.performed -= MousePressed;

        inputActions.FindActionMap("Player").Disable();
    }

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (highlightedConnectPoint != null)
        {
            highlightedConnectPoint.Highlight();
        }

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        if (ComponentPlacementSystem.Instance.CurrentState != PlacementState.Idle)
            return;
        
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider != null && hit.collider.gameObject.CompareTag("ConnectionPoint"))
            {
                if (highlightedConnectPoint != null && highlightedConnectPoint.gameObject.name != hit.collider.gameObject.name)
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
                if (highlightedConnectPoint != null)
                    highlightedConnectPoint.RemoveHighlight();
            }
        }
    }

    private void MousePressed(InputAction.CallbackContext context)
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        
        //do nott allow wire creation if the placement system is in use
        if (ComponentPlacementSystem.Instance.CurrentState != PlacementState.Idle)
        {
            return;
        }
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider != null && hit.collider.gameObject.CompareTag("ConnectionPoint"))
            {
                if (!isMakingWire)
                    StartWireCreator(hit.collider.gameObject, startConnectionPoint);
                else
                    EndWireCreator(hit.collider.gameObject, endConnectionPoint);
            }
            else if (hit.collider != null && hit.collider.gameObject.CompareTag("Wire"))
            {
                Wire wire = hit.collider.gameObject.GetComponent<Wire>();
                wire.Disconnect();
                Destroy(hit.collider.gameObject);
            }
        }
    }

    private void StartWireCreator(GameObject clickedGameObject, ConnectionPoint sPoint)
    {
        if (isMakingWire)
            return;
       
        //If we're clicking the same hole
        if (sPoint != null && clickedGameObject.name == sPoint.gameObject.name)
        {
            sPoint.RemoveHighlight(true);
            //sPoint = null;
            Debug.Log("Clicked the same hole.");
            //isMakingWire = false;
            return;
        }

        clickedGameObject.TryGetComponent(out startConnectionPoint);

        //if the point already has a wire
        if (startConnectionPoint.isTaken)
        {
            startConnectionPoint.RemoveHighlight(true);
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


        /*
           Make a restriction that one of points has to be powered to be able to put a wire on it.
           */

        if (!startConnectionPoint.powered && !endConnectionPoint.powered)
        {
            Debug.Log("The starting point must be powered, cancelling wire creation");
            startConnectionPoint.RemoveHighlight(true);
            endConnectionPoint.RemoveHighlight(true);

            startConnectionPoint = null;
            endConnectionPoint = null;
            isMakingWire = false;

           
            return;
        }

        //battery to battery should not work
        if (startConnectionPoint.type == ConnectionPointType.Battery && endConnectionPoint.type == ConnectionPointType.Battery)
        {
            return;
        }

        //battery to terminal should not work
        if (startConnectionPoint.type == ConnectionPointType.Battery && endConnectionPoint.type == ConnectionPointType.Terminal || endConnectionPoint.type == ConnectionPointType.Battery && startConnectionPoint.type == ConnectionPointType.Terminal)
        {
            return;
        }
        
        Debug.Log("Ending wire from hole: " + clickedGameObject.name);

        var breadboard = ComponentTracker.Instance.breadboard;
        ConnectionPoint parentPoint = startConnectionPoint;
        ConnectionPoint childPoint = endConnectionPoint;
        if (!breadboard.CircuitTree.IsEmpty)
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
                return;
            }

            var parentNode = breadboard.CircuitTree.DepthFirstSearch(breadboard.CircuitTree.Root, parentPoint);
            var possibleDisconnectedChildNode = breadboard.DisconnectedCircuitTrees.Find(x => x.DepthFirstSearch(x.Root, childPoint) != null);
            
            breadboard.PropogatePower(parentNode);
            //if the child node is by itself
            if (possibleDisconnectedChildNode == null)
            {
                var childNode = new CircuitNode(endConnectionPoint);
                parentNode.AddChildNode(childNode);

                //add all the rest of the terminal nodes to the tree
                if (childPoint.type == ConnectionPointType.Terminal)
                {
                    Hole childHole = childPoint as Hole;
                    foreach (var child in childHole.parentTerminal.holes)
                    {
                        var cNode = new CircuitNode(child);
                        parentNode.AddChildNode(cNode);
                    }
                }

                if (childPoint.type == ConnectionPointType.Rail && childPoint.charge == Charge.Negative)
                {
                    Debug.Log("Complete circuit" % Colorize.Green);
                }
            } else //if the chilld node is its own circuit tree that was previously disconnected
            {
                parentNode.Children.Add(possibleDisconnectedChildNode.Root);
                breadboard.DisconnectedCircuitTrees.Remove(possibleDisconnectedChildNode);

                Debug.Log("Reconnected to power" % Colorize.Blue);
            }

            var paths = breadboard.CircuitTree.getPaths(breadboard.CircuitTree.Root);
            paths.Reverse();

            foreach (var path in paths)
            {
                string a = "";
                foreach (var item in path)
                {
                    a += item.Data.name + " -> ";
                }
                //Debug.Log(a);
            }
            
        }

        Wire wireComponent = CreateWireBetweenTwoPoints(parentPoint, childPoint);

        //positive electrode
        if (startConnectionPoint.type == ConnectionPointType.Battery && startConnectionPoint.charge == Charge.Positive)
        {
            if (endConnectionPoint.type == ConnectionPointType.Rail)
            {
                var connectionPoint = (Hole)endConnectionPoint;
                if (connectionPoint.parentBreadboard.CircuitTree.Root == null)
                {
                    //this positive rail is now the root rail
                    connectionPoint.parentBreadboard.CircuitTree.Root = new CircuitNode(startConnectionPoint);
                }
            }
        }
        
        startConnectionPoint = null;
        endConnectionPoint = null;
        isMakingWire = false;
        numberOfWires++;
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
        var wireComponent = wire.GetComponent<Wire>();

        wire.transform.localScale = new Vector3(
            wire.transform.localScale.x,
            wire.transform.localScale.y,
            distance
        );

        wireComponent.start = source;
        wireComponent.end = target;
        wireComponent.ChangeToRandomColour();

        source.wire = wireComponent;
        target.wire = wireComponent;

        source.ConnectToHole(target);

        

        //cleanup
        source.RemoveHighlight(true);
        target.RemoveHighlight(true);

        source.isTaken = true;
        target.isTaken = true;

        return wireComponent;
    }
}