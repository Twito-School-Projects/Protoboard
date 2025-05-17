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
    private Hole startHole = null;

    [SerializeField]
    private Hole endHole = null;

    [SerializeField]
    private Hole highlightedHole;

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
        if (highlightedHole != null)
        {
            highlightedHole.Highlight();
        }

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider != null && hit.collider.gameObject.CompareTag("Hole"))
            {
                if (highlightedHole != null && highlightedHole.gameObject.name != hit.collider.gameObject.name)
                {
                    highlightedHole.RemoveHighlight();
                    highlightedHole = null;

                    return;
                }

                highlightedHole = hit.collider.gameObject.GetComponent<Hole>();
                highlightedHole.Highlight();
            }
            else
            {
                if (highlightedHole != null)
                    highlightedHole.RemoveHighlight();
            }
        }
    }

    private void MousePressed(InputAction.CallbackContext context)
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider != null && hit.collider.gameObject.CompareTag("Hole"))
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
        if (startHole != null && startGameObject.name == startHole.gameObject.name)
        {
            return;
        }

        startGameObject.TryGetComponent(out startHole);

        if (startHole.isTaken)
        {
            startHole.RemoveHighlight();
            startHole = null;
            Debug.Log("This hole is already taken, cancelling wire creation.");
            return;
        }

        isMakingWire = true;
        startHole.Highlight(true);

        Debug.Log("Creating wire from hole: " + startGameObject.name);
    }

    private void EndWireCreator(GameObject endGameObject)
    {
        if (!isMakingWire)
            return;

        //If the end hole is the same as the start hole
        if (endGameObject.name == startHole.gameObject.name)
        {
            Debug.Log("Clicked the same hole, cancelling wire creation.");
            return;
        }

        endGameObject.TryGetComponent(out endHole);

        if (endHole.isTaken)
        {
            Debug.Log("This hole is already taken, choose another.");
            endHole = null;
            return;
        }
        else if (startHole.charge != Charge.None && endHole.charge != Charge.None && startHole.charge != endHole.charge)
        {
            Debug.Log("Cannot connect positive to negative");
            endHole = null;
            return;
        }

        Debug.Log("Ending wire from hole: " + endGameObject.name);

        CreateWireBetweenTwoPoints(startHole.transform.position, endHole.transform.position);

        startHole.RemoveHighlight(true);
        endHole.RemoveHighlight(true);

        startHole.isTaken = true;
        endHole.isTaken = true;

        startHole = null;
        endHole = null;
        isMakingWire = false;
        numberOfWires++;
    }

    private void CreateWireBetweenTwoPoints(Vector3 start, Vector3 end)
    {
        float distance = Vector3.Distance(start, end);
        Vector3 midPoint = (start + end) / 2;
        Quaternion rotation = Quaternion.LookRotation(end - start, Vector3.up);

        midPoint.y += (numberOfWires * wirePrefab.transform.localScale.y);

        var parent = startHole.parentBreadboard.transform;
        var wire = Instantiate(wirePrefab, midPoint, rotation, parent);
        var wireComponent = wire.GetComponent<Wire>();

        wire.transform.localScale = new Vector3(
            wire.transform.localScale.x / parent.localScale.x,
            wire.transform.localScale.y / parent.localScale.y,
            distance / parent.localScale.z
        );

        wireComponent.start = startHole;
        wireComponent.end = endHole;
        wireComponent.ChangeToRandomColour();

        startHole.ConnectToHole(endHole);
    }
}