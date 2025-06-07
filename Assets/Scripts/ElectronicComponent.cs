using System;
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
    public Sprite imageSprite;
    public new Collider collider;
    public Renderer[] renderers;
    public ComponentType componentType;
    public bool isSelectedFromUI = false;

    [Header("Placement")]
    public bool isInPlacementMode = false;

    [Header("Snapping")]
    public float snapDistance = 0.5f;
    public LayerMask snapTargetLayer = 1; // Layer for breadboard holes

    public Transform mouseTargetPeg;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected void Start()
    {
        //rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
    }

    // Update is called once per frame
    protected void Update()
    {
    }

    private void OnDestroy()
    {
        if (isInPlacementMode) Debug.Log("Destroyed for some reason");
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
                    if (material.HasProperty(Color1))
                    {
                        var color = material.color;
                        color.a = 1.0f;
                        material.color = color;
                    }
                }
            }
        }
    }
    
    public virtual void OnPlaced()
    {
        // Override in derived classes for component-specific placement logic
    }


    public virtual void OnDragStart()
    {
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
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
                Debug.Log("Find: " + hole.name);
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
        return false;
    }
}