using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlaceUIObject : Singleton<PlaceUIObject>
{
    [SerializeField]
    private InputAction mouseClick;

    public ElectronicComponent currentlySelectedComponent;
    public GameObject currentlySelectedObject;

    public bool isPlacingObject = false;
    private Vector3 velocity = Vector3.zero;
    private Camera mainCamera;

    private IEnumerator Coroutine;
    private void OnEnable()
    {
        mouseClick.Enable();
        mouseClick.performed += PlaceComponent;
    }

    private void OnDisable()
    {
        mouseClick.Disable();
        mouseClick.performed -= PlaceComponent;
    }

    //when the user wants to place the componenet
    private void PlaceComponent(InputAction.CallbackContext context)
    {
        if (isPlacingObject)
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;

            if (isPlacingObject)
            {
                if (Coroutine != null) StopCoroutine(Coroutine);
                Debug.Log("Stopping");
            }
        }
    }

    private IEnumerator FollowUpdate(GameObject clickedObject)
    {
        float initialDistance = Vector3.Distance(clickedObject.transform.position, mainCamera.transform.position);
        while (mouseClick.ReadValue<float>() != 0)
        {
            Debug.Log("Following");
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            Vector3 direction = ray.GetPoint(initialDistance) - clickedObject.transform.position;
            Vector3 xzDirection = new Vector3(direction.x, 0f, direction.z);

            clickedObject.transform.position = Vector3.SmoothDamp(clickedObject.transform.position, ray.GetPoint(initialDistance), ref velocity, 1);
            clickedObject.transform.position = new Vector3(clickedObject.transform.position.x, 4, clickedObject.transform.position.z);
            yield return null;
        }
    }

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (currentlySelectedComponent != null)
        {
            currentlySelectedComponent.isSelectedFromUI = true;
            currentlySelectedComponent.FollowMouse();
        }
    }

    public void DestroyCurrent()
    {
        Destroy(currentlySelectedObject);
        currentlySelectedObject = null;
        currentlySelectedComponent = null;
    }

    public void Create(GameObject prefab)
    {
        currentlySelectedObject = Instantiate(prefab);
        currentlySelectedComponent = currentlySelectedObject.GetComponent<ElectronicComponent>();

        var breadboard = ComponentTracker.Instance.breadboard;
        var battery = ComponentTracker.Instance.battery;

        //cannot have multiple of each
        if (battery != null && currentlySelectedComponent.componentType == ComponentType.Battery)
        {
            DestroyCurrent();
            return;
        }
        else if (breadboard != null && currentlySelectedComponent.componentType == ComponentType.Breadboard)
        {
            DestroyCurrent();
            return;
        }

        if (currentlySelectedComponent != null)
        {
            Coroutine = FollowUpdate(currentlySelectedObject);

            StartCoroutine(Coroutine);
        currentlySelectedComponent.DisableFunctionality();
            isPlacingObject = true;

        }
    }
}
