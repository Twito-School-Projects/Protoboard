using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;


public enum PlacementState
{
    Idle,
    Previewing,
    Placing,
    Validating
}

public class  ComponentPlacementSystem : Singleton<ComponentPlacementSystem>
{
    [Header("Placement Settings")]
    [SerializeField] private LayerMask placementSurface = 1;
    [SerializeField] private float snapDistance = 0.5f;
    [SerializeField] private float previewHeight = 2f;
    
    [Header("Input")]
    [SerializeField] private InputAction placeAction;
    [SerializeField] private InputAction cancelAction;
    
    [Header("Extra Data")]
    [SerializeField] private GameObject matObject;
    // Events
    public static event Action<ComponentData> OnPlacementStarted;
    public static event Action<GameObject> OnComponentPlaced;
    public static event Action OnPlacementCancelled;
    public static event Action<PlacementValidationResult> OnValidationChanged;
    
    // State
    public PlacementState CurrentState { get; private set; } = PlacementState.Idle;
    public ComponentData CurrentComponentData { get; private set; }
    public GameObject PreviewObject { get; private set; }

    public ElectronicComponent PreviewComponent { get; private set; }
    // Components
    private Camera mainCamera;
    private PlacementValidator validator;
    private PlacementPreview previewRenderer;
    private Hole highlightedConnectPoint;

    private void Awake()
    {
        base.Awake();
        
        mainCamera = Camera.main;
        validator = GetComponent<PlacementValidator>();
        previewRenderer = GetComponent<PlacementPreview>();
    }
    
    private void OnEnable()
    {
        placeAction.Enable();
        cancelAction.Enable();
        
        placeAction.performed += OnPlacePressed;
        cancelAction.performed += OnCancelPressed;
    }
    
    private void OnDisable()
    {
        placeAction.performed -= OnPlacePressed;
        cancelAction.performed -= OnCancelPressed;
        
        placeAction.Disable();
        cancelAction.Disable();
    }
    
    private void Update()
    {
        if (CurrentState == PlacementState.Previewing)
        {
            snapDistance = ComponentTracker.Instance.breadboard.HoleDistance;
            UpdatePreview();
        }
    }
    
    public bool StartPlacement(ComponentData componentData)
    {
        if (CurrentState != PlacementState.Idle)
        {
            Debug.LogWarning("Cannot start placement - system is not idle");
            return false;
        }
        
        CurrentComponentData = componentData;
        CurrentState = PlacementState.Previewing;
        
        CreatePreviewObject();
        OnPlacementStarted?.Invoke(componentData);
        
        return true;
    }
    
    public void CancelPlacement()
    {
        if (CurrentState == PlacementState.Idle) return;
        
        DestroyPreviewObject();
        ResetState();
        OnPlacementCancelled?.Invoke();
    }
    
    private void CreatePreviewObject()
    {
        if (!CurrentComponentData?.prefab) return;
        
        PreviewObject = Instantiate(CurrentComponentData.prefab);
        var component = PreviewObject.GetComponent<ElectronicComponent>();
        
        if (component)
        {
            component.SetPlacementMode(true);
            PreviewComponent = component;
        }
        
        previewRenderer.SetupPreview(PreviewObject);
    }
    
    private void UpdatePreview()
    {
        if (!PreviewObject) return;
        Vector3 worldPositionFromMouse = GetWorldPositionFromMouse();
        Hole possibleSnapPoint = GetPossibleHoleLocation();
        Transform targetPeg = PreviewComponent.mouseTargetPeg;
        
        Vector3 pegOffset = targetPeg.position - PreviewObject.transform.position;
    
        // Position the parent so the peg ends up at the mouse position
        Vector3 targetParentPosition = worldPositionFromMouse - pegOffset;
        PreviewObject.transform.position = new Vector3(targetParentPosition.x, previewHeight, targetParentPosition.z);

        if (possibleSnapPoint)
        {
            
            // Validate placement
            var validationResult = validator.ValidatePlacement(possibleSnapPoint, worldPositionFromMouse, CurrentComponentData);
            previewRenderer.UpdateValidationVisuals(validationResult);
            OnValidationChanged?.Invoke(validationResult);
        }
    }

    private Hole GetPossibleHoleLocation()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, placementSurface))
        {
            if (hit.collider && hit.collider.gameObject.CompareTag("ConnectionPoint"))
            {
                if (highlightedConnectPoint && highlightedConnectPoint.gameObject.name != hit.collider.gameObject.name)
                {
                    highlightedConnectPoint.RemoveHighlight();
                    highlightedConnectPoint = null;

                    return highlightedConnectPoint;
                }

                highlightedConnectPoint = hit.collider.gameObject.GetComponent<Hole>();
                highlightedConnectPoint.Highlight();
                return highlightedConnectPoint;
            }
            else
            {
                if (highlightedConnectPoint)
                    highlightedConnectPoint.RemoveHighlight();
            }
        }
        else
        {
            highlightedConnectPoint = null;
        }

        return null;
    }
    
    private Vector3 GetWorldPositionFromMouse()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        Debug.DrawRay(ray.origin, ray.direction * 100, Color.red);
        
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            return hit.point;
        }
        
        // Fallback to a plane at y=0
        float distance = -ray.origin.y / ray.direction.y;
        return ray.origin + ray.direction * distance;
    }
    
    private void OnPlacePressed(InputAction.CallbackContext context)
    {
        if (CurrentState != PlacementState.Previewing) return;
        if (!highlightedConnectPoint) return;
        
        Vector3 position = PreviewObject.transform.position;
        position.y = highlightedConnectPoint.transform.position.y;
        Vector3 targetPositionFromMouse = GetWorldPositionFromMouse();

        var validationResult = validator.ValidatePlacement(highlightedConnectPoint, targetPositionFromMouse, CurrentComponentData);
        
        if (validationResult.IsValid)
        {
            PlaceComponent(position);
        }
        else
        {
            Debug.Log($"Cannot place component: {validationResult.ErrorMessage}");
        }
    }
    
    private void OnCancelPressed(InputAction.CallbackContext context)
    {
        CancelPlacement();
    }
    
    private void PlaceComponent(Vector3 position)
    {
        CurrentState = PlacementState.Placing;
    
        // Create the actual component
        GameObject placedObject = Instantiate(CurrentComponentData.prefab, position, CurrentComponentData.prefab.transform.rotation );
        var component = placedObject.GetComponent<ElectronicComponent>();
    
        if (component)
        {
            component.SetPlacementMode(true); // Keep in placement mode initially
            
            bool snapped = component.TrySnapToBreadboard(highlightedConnectPoint);
            if (snapped)
            {
                //we're done here
                component.SetPlacementMode(false);
                component.OnPlaced();
                OnComponentPlaced?.Invoke(placedObject);
            }
            else
            {
                // If snapping failed, destroy the component and stay in preview mode
                Destroy(placedObject);
                Debug.Log("Cannot place component - no valid breadboard holes found");
                CurrentState = PlacementState.Previewing;
                return;
            }
            
        }
    
        // Clean up
        DestroyPreviewObject();
        ResetState();
    }
    
    private void DestroyPreviewObject()
    {
        if (!PreviewObject) return;
        Destroy(PreviewObject);
        PreviewObject = null;
    }
    
    private void ResetState()
    {
        CurrentState = PlacementState.Idle;
        CurrentComponentData = null;
    }
}