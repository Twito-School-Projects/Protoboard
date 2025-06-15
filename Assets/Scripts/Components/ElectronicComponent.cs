using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public enum ComponentType
{
    Battery,
    Breadboard,
    LED,
    Resistor,
    Arduino,
    None
}

public class ElectronicComponent : MonoBehaviour, IDrag
{
    private static readonly int Color1 = Shader.PropertyToID("_Color");
    [SerializeField] protected Rigidbody rb;
    public bool isUnidirectional = true;
    public Renderer[] renderers;
    public ComponentType componentType;

    [Header("Placement")]
    public bool isInPlacementMode = false;

    public bool isSelectedFromUI;
    [Header("Snapping")]
    public float snapDistance = 0.5f;
    public LayerMask snapTargetLayer = 1; // Layer for breadboard holes

    public Transform mouseTargetPeg;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    [SerializeField] private InputActionAsset inputActions;
    private InputAction mouseClick;
    
    private Camera mainCamera;
    public new Collider collider;

    public bool hasCircuitCompleted = false ;

    [SerializeField]
    private Dictionary<string, Color> originalColors = new();
    
    public Hole anodeHole;
    public Hole cathodeHole;

    public Transform anodeLocation;
    public Transform cathodeLocation;
    
    private void Awake()
    {
        foreach (var r in renderers)
        {
            foreach (var rMaterial in r.materials)
            {
                originalColors.Add(rMaterial.name, rMaterial.color);
            }
        }
    }

    protected void Start()
    {
        //rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
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

    }

    // Update is called once per frame
    protected void Update()
    {
        if (anodeHole && cathodeHole)
            hasCircuitCompleted = anodeHole.wasPropagated && cathodeHole.wasPropagated;
    }

    private void OnDestroy()
    {
        if (isInPlacementMode) Debug.Log("Destroyed for some reason");

        if (!isInPlacementMode)
        {
            Deleted();
        }
    }

    private void MousePressed(InputAction.CallbackContext context)
    {
        if (isInPlacementMode) return;
        
        if (!context.performed) return;
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        if (!Physics.Raycast(ray, out hit)) return;
        if (!hit.collider || hit.collider!= collider) return;
        Deleted();
    }
    

    public virtual void SetPlacementMode(bool placementMode)
    {
        isInPlacementMode = placementMode;

        if (placementMode)
        {
            // Enable placement mode - make semi-transparent, disable physics, etc.
            rb.isKinematic = true;

            // Make semi-transparent for preview
            foreach (var renderer in renderers)
            {
                var materials = renderer.materials;
                foreach (var material in materials)
                {
                    if (material.HasProperty(Color1))
                    {
                        var color = material.color;
                        color.a = 0.5f;
                        material.color = color;
                    }
                }
            }
        }
        else
        {
            // Disable placement mode - restore normal state
            // Restore full opacity
            rb.isKinematic = true;

            foreach (var renderer in renderers)
            {
                var materials = renderer.materials;
                foreach (var material in materials)
                {
                    foreach (var rMaterial in renderer.materials)
                    {
                        if (!material.HasProperty(Color1))
                        {
                            throw new Exception("Material does not have _Color property: " + material.name);
                        }

                        // Restore original color
                        if (originalColors.TryGetValue(rMaterial.name, out var color))
                            rMaterial.color = color;
                    }
                }
            }
        }
    }


    public virtual void OnDragStart()
    {
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        
        // Clear hole occupancy when starting to drag
        if (anodeHole != null) anodeHole.ClearOccupancy();
        if (cathodeHole != null) cathodeHole.ClearOccupancy();
    }


    public virtual void OnDragEnd()
    {
        rb.useGravity = true;
    }

    public virtual void DisableFunctionality()
    {
        collider.enabled = false;

        foreach (var item in renderers)
        {
            Color newColor = item.material.color;
            newColor.a = 0.1f;

            item.material.color = newColor;
        }
    }

    public virtual void FollowMouse()
    {

    }

    protected virtual Vector3 CalculateMidpoint()
    {
        return Vector3.zero;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
    }
    
    
    protected Hole FindNearestHole(Vector3 position, Hole other)
    {
        Collider[] nearbyColliders = Physics.OverlapSphere(position, snapDistance, snapTargetLayer);
        
        Hole nearestHole = null;
        float nearestDistance = float.MaxValue;
        
        foreach (var collider in nearbyColliders)
        {
            Hole hole = collider.GetComponent<Hole>();
            
            //prevents the other peg using the hole alrady
            if (hole && hole.name == other.name) continue;
            
            if (hole && !hole.IsOccupied)
            {
                //Debug.Log("Find: " + hole.name);
                float distance = Vector3.Distance(position, hole.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestHole = hole;
                }
            }
        }
        
        return nearestHole;
    }

    protected void SnapToHole(Transform componentPin, Hole targetHole)
    {
        if (targetHole != null)
        {
            componentPin.position = targetHole.transform.position;
            targetHole.IsOccupied = true;
            targetHole.OccupiedBy = this;
        }
    }

    /// <summary>
    /// For after the component is placed, try to snap it to the breadboard.
    /// </summary>
    /// <returns></returns>
    public virtual bool TrySnapToBreadboard(Hole initialHole)
    {
        if (componentType == ComponentType.Breadboard || componentType == ComponentType.Battery) return false;
        
        Hole cathodeHole = initialHole;
        Hole anodeHole = FindNearestHole(anodeLocation.position, cathodeHole);

        if (anodeHole)
        {
            SnapToHole(cathodeLocation, initialHole);
            SnapToHole(anodeLocation, anodeHole);

            this.cathodeHole = initialHole;
            this.anodeHole = anodeHole;
            
            Debug.Log(cathodeHole.name);
            Debug.Log(anodeHole.name);
    
            Vector3 midpoint = new Vector3(
                (cathodeHole.transform.position.x + anodeHole.transform.position.x) / 2,
                transform.position.y,
                cathodeHole.transform.position.z
            );
            transform.position = midpoint;
        }

        // Check if both pins can snap to valid holes
        return anodeHole && cathodeHole && anodeHole != cathodeHole;
    }
    
    protected virtual void Deleted()
    {
        if (componentType == ComponentType.Breadboard || componentType == ComponentType.Battery) return;

        if (!cathodeHole.powered) return;
        if (isInPlacementMode) return;
        
        //having power flow from the cathode to the anode
        var cathodeNode = CircuitManager.Instance.FindNodeInTrees(cathodeHole);
        var anodeNode = cathodeNode.Children.FirstOrDefault(x => x.Data == anodeHole);

        if (cathodeNode == null)
        {
            throw new Exception("Something is very wrong");
        }
        if (anodeNode == null)
        {
            throw new Exception("Something is very wrong");
        }
    
        CircuitManager.Instance.DisconnectNodes(cathodeNode.Data, anodeNode.Data);
    }
    
    public virtual void OnPlaced()
    {
        if (componentType == ComponentType.Breadboard || componentType == ComponentType.Battery) return;
        if (!cathodeHole.powered) return;
        
        //having power flow from the cathode to the anode
        var cathodeNode = CircuitManager.Instance.FindNodeInTree(TreeType.Battery, cathodeHole);
        var anodeNode = CircuitManager.Instance.FindNodeInTree(TreeType.Battery, anodeHole);

        if (cathodeNode == null)
        {
            Debug.Log("Cathode is not in the tree, something is wrong" % Colorize.Orange);
            return;
        }
        
        anodeNode ??= CircuitManager.Instance.FindNodeInTree(TreeType.DisconnectedBattery, anodeHole);
        if (anodeNode == null)
        {
            if (this is Resistor)
            {
                if (!anodeHole.Resistors.Contains(this as Resistor))
                {
                    anodeHole.Resistors.Add(this as Resistor);
                }
            }
            anodeNode = CircuitManager.Instance.AddChild(ref cathodeNode, ref anodeHole);
            CircuitManager.Instance.PropagatePower(cathodeNode);
            Debug.Log("anode not in tree, adding it");
        }
        else
        {
            CircuitManager.Instance.ReconnectTree(cathodeHole,anodeHole, TreeType.Battery);
            Debug.Log("anode in tree, reconnecting");
        }
    }
}