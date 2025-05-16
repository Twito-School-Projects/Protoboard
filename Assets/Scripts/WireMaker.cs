using UnityEngine;
using UnityEngine.InputSystem;

public class WireMaker : MonoBehaviour
{
    [SerializeField]
    private InputActionAsset inputActions;

    private InputAction mouseClick;

    [SerializeField]
    private GameObject cylinderPrefab;

    private Camera mainCamera;
    private bool isMakingWire = false;

    private Hole? startHole = null;
    private Hole? endHole = null;

    private Hole currentHoveredHole;

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
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider != null && hit.collider.gameObject.CompareTag("Hole"))
            {
                hit.collider.gameObject.TryGetComponent<Hole>(out currentHoveredHole);
                currentHoveredHole?.Highlight();
            }
            else
            {
                currentHoveredHole?.RemoveHighlight();
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
        }
    }

    private void StartWireCreator(GameObject startGameObject)
    {
        //If we're clicking the same hole
        if (startHole != null && startGameObject.name == startHole?.gameObject.name)
        {
            return;
        }

        isMakingWire = true;
        startGameObject.TryGetComponent(out startHole);
        Debug.Log("Creating wire from hole: " + startGameObject.name);
    }

    private void EndWireCreator(GameObject endGameObject)
    {
        //If the end hole is the same as the start hole
        if (endGameObject.name == startHole.gameObject.name)
        {
            return;
        }

        endGameObject.TryGetComponent(out endHole);
        isMakingWire = false;
        Debug.Log("Ending wire from hole: " + endGameObject.name);

        CreateCylinderBetweenTwoPoints(startHole.gameObject.transform.position, endHole.gameObject.transform.position);
    }

    private void CreateCylinderBetweenTwoPoints(Vector3 start, Vector3 end)
    {
        float distance = Vector3.Distance(start, end);
        Vector3 midPoint = (start + end) / 2;
        Quaternion rotation = Quaternion.LookRotation(end - start, Vector3.up);

        var cylinder = Instantiate(cylinderPrefab, midPoint, Quaternion.identity);
        cylinder.transform.localScale = new Vector3(cylinder.transform.localScale.x, distance / 2, cylinder.transform.localScale.z);
    }
}