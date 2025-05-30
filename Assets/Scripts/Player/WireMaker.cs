using UnityEngine;
using UnityEngine.InputSystem;

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

    private void StartWireCreator(GameObject startGameObject, ConnectionPoint startConnectionPoint)
    {
        if (isMakingWire)
            return;

        //If we're clicking the same hole
        if (startConnectionPoint != null && startGameObject.name == startConnectionPoint.gameObject.name)
        {
            startConnectionPoint.RemoveHighlight(true);
            startConnectionPoint = null;
            Debug.Log("Cancelled wire creation, clicked the same hole.");
            isMakingWire = false;
            return;
        }

        startGameObject.TryGetComponent(out startConnectionPoint);

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

        this.startConnectionPoint = startConnectionPoint;

        Debug.Log("Creating wire from hole: " + startGameObject.name);
    }

    private void EndWireCreator(GameObject endGameObject, ConnectionPoint endConnectionPoint)
    {
        if (!isMakingWire)
            return;

        //If the end hole is the same as the start hole
        if (endGameObject.name == startConnectionPoint.gameObject.name)
        {
            Debug.Log("Clicked the same hole, cancelling wire creation.");
            return;
        }

        endGameObject.TryGetComponent(out this.endConnectionPoint);

        if (this.endConnectionPoint.isTaken)
        {
            Debug.Log("This hole is already taken, choose another.");
            this.endConnectionPoint = null;
            return;
        }

        // bool isInvalidCharge =
        //     (startConnectPoint.hasConstantCharge && endConnectPoint.hasConstantCharge) &&
        //     startConnectPoint.charge != Charge.None &&
        //     endConnectPoint.charge != Charge.None &&
        //     startConnectPoint.charge != endConnectPoint.charge;

        // if (isInvalidCharge)
        // {
        //     Debug.Log("Cannot connect positive to negative");
        //     endConnectPoint = null;
        //     return;
        // }

        //battery to battery should not work
        if (startConnectionPoint.type == ConnectionPointType.Battery && this.endConnectionPoint.type == ConnectionPointType.Battery)
        {
            return;
        }

        //battery to terminal should not work
        if (startConnectionPoint.type == ConnectionPointType.Battery && this.endConnectionPoint.type == ConnectionPointType.Terminal || this.endConnectionPoint.type == ConnectionPointType.Battery && startConnectionPoint.type == ConnectionPointType.Terminal)
        {
            return;
        }
        
        Debug.Log("Ending wire from hole: " + endGameObject.name);

        this.endConnectionPoint = endConnectionPoint;
        Wire wireComponent = CreateWireBetweenTwoPoints(startConnectionPoint, this.endConnectionPoint);

        //positive electrode
        if (startConnectionPoint.type == ConnectionPointType.Battery && startConnectionPoint.charge == Charge.Positive)
        {
            if (this.endConnectionPoint.type == ConnectionPointType.Rail)
            {
                var connectionPoint = (Hole)this.endConnectionPoint;
                if (connectionPoint.parentBreadboard.CircuitTree.Root == null)
                {
                    //this positive rail is now the root rail
                    connectionPoint.parentBreadboard.CircuitTree.Root = new CircuitNode(startConnectionPoint);
                }
            }
        }
        
        startConnectionPoint = null;
        this.endConnectionPoint = null;
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

        var breadboard = ComponentTracker.Instance.breadboard;
        if (!breadboard.CircuitTree.IsEmpty)
        {
            breadboard.CircuitTree.DepthFirstSearch(breadboard.CircuitTree.Root, source);
        }

        //cleanup
        source.RemoveHighlight(true);
        target.RemoveHighlight(true);

        source.isTaken = true;
        target.isTaken = true;

        return wireComponent;
    }
}