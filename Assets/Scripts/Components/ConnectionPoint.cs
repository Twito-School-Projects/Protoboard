using UnityEngine;

public enum ConnectionPointType
{
    Battery,
    Terminal,
    Rail,
}

public class ConnectionPoint : MonoBehaviour
{
    public MeshRenderer MeshRenderer;
    public Material material;
    protected Color startColor;
    protected bool isLockedHighlight = false;

    [HideInInspector]
    public bool hasConstantCharge = false;

    public bool isTaken = false;
    public bool powered;
    public bool isBeingHighlighted = false;


    public ConnectionPoint nextConnectedPoint;
    public ConnectionPoint previousConnectedPoint;
    public Charge charge;

    public ConnectionPointType type;
    public bool IsOccupied { get; set; } = false;
    public ElectronicComponent OccupiedBy { get; set; } = null;
    
    public void Start()
    {
        MeshRenderer = GetComponent<MeshRenderer>();
        material = MeshRenderer.material;

        startColor = material.color;
        material.color = Color.clear;
    }
    
    private void OnEnable()
    {
        Wire.OnWireConnected += OnWireConnectedEvent;
        Wire.OnWireDeleted += OnWireDeletedEvent;
        WireMaker.WireCreationCancelled += OnWireCreationCancelled;
    }
    private void OnDisable()
    {
        Wire.OnWireConnected -= OnWireConnectedEvent;
        Wire.OnWireDeleted -= OnWireDeletedEvent;
        WireMaker.WireCreationCancelled -= OnWireCreationCancelled;
    }

    public void Highlight(bool lockHighlight = false)
    {
        if (lockHighlight) isLockedHighlight = true;
        material.color = startColor;
        isBeingHighlighted = true;
    }

    public void RemoveHighlight(bool overrideHighlight = false)
    {
        if (overrideHighlight || !isLockedHighlight)
        {
            if (material != null && material.color != Color.clear)
            {
                material.color = Color.clear;
            }
            isBeingHighlighted = false;
        }
    }

    public virtual void ConnectToHole(ConnectionPoint hole)
    { }

    public virtual void SetPowered(bool powered)
    {
    }

    private void OnWireCreationCancelled()
    {
        RemoveHighlight(true);
    }

    private void OnWireDeletedEvent(Wire wireComponent, ConnectionPoint start, ConnectionPoint end)
    {
        if (start == this)
        {
            nextConnectedPoint = null;
            if (powered) end.SetPowered(false);
        }
        else
        {
            end.previousConnectedPoint = null;
        }
        isTaken = false;
    }


    private void OnWireConnectedEvent(Wire wireComponent, ConnectionPoint source, ConnectionPoint target)
    {
        //for another hole
        if (source != this && target != this)
            return;
        Debug.Log("connected to wire");
        
        RemoveHighlight(true);
        isTaken = true;
        IsOccupied = true;
        //occupiedby = ???
        
        if (source == this)
            ConnectToHole(target);
    }
}