using UnityEngine;
using UnityEngine.InputSystem;

public class WireMaker : MonoBehaviour
{
    [SerializeField]
    private InputActionAsset inputActions;

    private InputAction mouseClick;

    [SerializeField]
    private GameObject wirePrefab;

    private Camera mainCamera;

    [SerializeField]
    private bool isMakingWire = false;

    [SerializeField]
    private ConnectionPoint startConnectPoint = null;

    [SerializeField]
    private ConnectionPoint endConnectPoint = null;

    [SerializeField]
    private ConnectionPoint highlightedConnectPoint;

    [SerializeField]
    private Transform wiresContainer;

    private int numberOfWires = 0;

    private void Awake()
    {
    }

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
                    StartWireCreator(hit.collider.gameObject);
                else
                    EndWireCreator(hit.collider.gameObject);
            }
            else if (hit.collider != null && hit.collider.gameObject.CompareTag("Wire"))
            {
                Wire wire = hit.collider.gameObject.GetComponent<Wire>();
                wire.Disconnect();
                Destroy(hit.collider.gameObject);
            }
        }
    }

    private void StartWireCreator(GameObject startGameObject)
    {
        if (isMakingWire)
            return;

        //If we're clicking the same hole
        if (startConnectPoint != null && startGameObject.name == startConnectPoint.gameObject.name)
        {
            startConnectPoint.RemoveHighlight(true);
            startConnectPoint = null;
            Debug.Log("Cancelled wire creation, clicked the same hole.");
            isMakingWire = false;
            return;
        }

        startGameObject.TryGetComponent(out startConnectPoint);

        if (startConnectPoint.isTaken)
        {
            startConnectPoint.RemoveHighlight(true);
            startConnectPoint = null;
            isMakingWire = false;
            Debug.Log("This hole is already taken, cancelling wire creation.");
            return;
        }

        isMakingWire = true;
        startConnectPoint.Highlight(true);

        Debug.Log("Creating wire from hole: " + startGameObject.name);
    }

    private void EndWireCreator(GameObject endGameObject)
    {
        if (!isMakingWire)
            return;

        //If the end hole is the same as the start hole
        if (endGameObject.name == startConnectPoint.gameObject.name)
        {
            Debug.Log("Clicked the same hole, cancelling wire creation.");
            return;
        }

        endGameObject.TryGetComponent(out endConnectPoint);

        if (endConnectPoint.isTaken)
        {
            Debug.Log("This hole is already taken, choose another.");
            endConnectPoint = null;
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
        if (startConnectPoint.type == ConnectionPointType.Battery && endConnectPoint.type == ConnectionPointType.Battery)
        {
            return;
        }

        //battery to terminal should not work
        if (startConnectPoint.type == ConnectionPointType.Battery && endConnectPoint.type == ConnectionPointType.Terminal || endConnectPoint.type == ConnectionPointType.Battery && startConnectPoint.type == ConnectionPointType.Terminal)
        {
            return;
        }
        
        Debug.Log("Ending wire from hole: " + endGameObject.name);

        CreateWireBetweenTwoPoints(startConnectPoint.transform.position, endConnectPoint.transform.position);

        startConnectPoint.RemoveHighlight(true);
        endConnectPoint.RemoveHighlight(true);

        startConnectPoint.isTaken = true;
        endConnectPoint.isTaken = true;

        startConnectPoint = null;
        endConnectPoint = null;
        isMakingWire = false;
        numberOfWires++;
    }

    private void CreateWireBetweenTwoPoints(Vector3 start, Vector3 end)
    {
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

        wireComponent.start = startConnectPoint;
        wireComponent.end = endConnectPoint;
        wireComponent.ChangeToRandomColour();

        startConnectPoint.wire = wireComponent;
        endConnectPoint.wire = wireComponent;

        startConnectPoint.ConnectToHole(endConnectPoint);
    }
}