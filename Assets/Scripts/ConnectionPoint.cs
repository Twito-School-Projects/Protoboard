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

    public Wire wire;
    public ConnectionPointType type;

    public void Start()
    {
        MeshRenderer = GetComponent<MeshRenderer>();
        material = MeshRenderer.material;

        startColor = material.color;
        material.color = Color.clear;
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
    { }
}