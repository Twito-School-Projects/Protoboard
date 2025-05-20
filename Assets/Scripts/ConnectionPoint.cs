using UnityEngine;

public class ConnectionPoint : MonoBehaviour
{
    protected MeshRenderer MeshRenderer;
    protected Material material;
    protected Color startColor;
    protected bool isLockedHighlight = false;

    [HideInInspector]
    public bool hasConstantCharge = false;

    public bool isTaken = false;
    public bool powered;
    public ConnectionPoint nextConnectedPoint;
    public ConnectionPoint previousConnectedPoint;
    public Charge charge;

    public Wire wire;

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
    }

    public void RemoveHighlight(bool overrideHighlight = false)
    {
        if (overrideHighlight || !isLockedHighlight)
            material.color = Color.clear;
    }

    public virtual void ConnectToHole(ConnectionPoint hole)
    { }

    public virtual void SetPowered(bool powered)
    { }
}