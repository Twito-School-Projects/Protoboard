using UnityEngine;

public class ElectronicComponent : MonoBehaviour, IDrag
{
    protected Rigidbody rb;
    public bool isUnidirectional = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    protected void Update()
    {
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
}