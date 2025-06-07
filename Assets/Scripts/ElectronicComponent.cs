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
    protected Rigidbody rb;
    public bool isUnidirectional = true;
    public Sprite imageSprite;
    public new Collider collider;
    public Renderer[] renderers;
    public ComponentType componentType;
    public bool isSelectedFromUI = false;

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
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
}