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

    private void OnEnable()
    {
        mouseClick.Enable();
        mouseClick.performed += PlaceComponent;
    }

    private void OnDisable()
    {
        mouseClick.performed -= PlaceComponent;
        mouseClick.Disable();
    }

    //when the user wants to place the componenet
    private void PlaceComponent(InputAction.CallbackContext context)
    {
        if (!isPlacingObject) return;
        isPlacingObject = true;
        currentlySelectedComponent.OnDragEnd();
        Debug.Log("Stopping");
    }

    private void FollowUpdate()
    {
        Debug.Log("Placing");
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mousePosition.y = 0;

        currentlySelectedObject.transform.position = mousePosition;
    }

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (isPlacingObject)
        {
            currentlySelectedComponent.isSelectedFromUI = true;
            currentlySelectedComponent.FollowMouse();
            FollowUpdate();
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
        if (battery && currentlySelectedComponent.componentType == ComponentType.Battery)
        {
            DestroyCurrent();
            return;
        }
        else if (breadboard && currentlySelectedComponent.componentType == ComponentType.Breadboard)
        {
            DestroyCurrent();
            return;
        }

        if (!currentlySelectedComponent) return;
        
        currentlySelectedComponent.OnDragStart();
        currentlySelectedComponent.DisableFunctionality();
        
        isPlacingObject = true;
    }
}
