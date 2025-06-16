using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

/// <summary>
/// Responsible for sending power throughout the board
/// </summary>
public class Wire : MonoBehaviour
{
    public ConnectionPoint start;
    public ConnectionPoint end;

    [SerializeField]
    private MeshRenderer meshRenderer;

    [SerializeField]
    private Material material;

    [HideInInspector]
    public BoxCollider boxCollider;

    [SerializeField] private InputActionAsset inputActions;

    private Camera mainCamera;
    private InputAction mouseClick;
    
    public static event Action<Wire, ConnectionPoint, ConnectionPoint> OnWireDeleted;
    public static event Action<Wire, ConnectionPoint, ConnectionPoint> OnWireConnected;

    private void Start()
    {
        material = meshRenderer.materials[0];
        boxCollider = GetComponent<BoxCollider>();
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        var map = inputActions.FindActionMap("Player");
        mouseClick = map.FindAction("Click");

        mouseClick.Enable();
        mouseClick.performed += MousePressed;
    }

    private void OnDisable()
    {
        mouseClick.performed -= MousePressed;
        mouseClick.Disable();
    }
    private void Update()
    {
        UpdateScale();
    }

    public void OnCreated(ConnectionPoint start, ConnectionPoint end)
    {
        this.start = start;
        this.end = end;

        OnWireConnected?.Invoke(this, start, end);
    }
    
    private void MousePressed(InputAction.CallbackContext callbackContext)
    {
        if (!callbackContext.performed) return;
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        if (!Physics.Raycast(ray, out hit)) return;
        if (!hit.collider || hit.collider!= boxCollider) return;
        
        Disconnect();
        Destroy(gameObject);
        OnWireDeleted?.Invoke(this, start, end);
    }

    private void UpdateScale()
    {
        if (!start || !end) return;
        float distance = Vector3.Distance(start.transform.position, end.transform.position);    

        Vector3 midPoint = (start.transform.position + end.transform.position) / 2;
        Quaternion rotation = Quaternion.LookRotation(end.transform.position - start.transform.position, Vector3.up);

        transform.position = midPoint;
        transform.rotation = rotation;
        transform.localScale = new Vector3(
            transform.localScale.x,
            transform.localScale.y,
            distance
        );
    }

    public void ChangeToRandomColour()
    {
        Color randomColor = new Color(Random.value, Random.value, Random.value);
        material.color = randomColor;
    }

    public void Disconnect()
    {
        
    }
}