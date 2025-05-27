using UnityEngine;

public class Battery : ElectronicComponent
{
    [HideInInspector]
    public BatteryElectrode positiveElectrode;

    [HideInInspector]
    public BatteryElectrode negativeElectrode;

    public float voltage = 1.5f;

    private new void Start()
    {
        base.Start();

        isUnidirectional = false;
        var electrodes = transform.GetComponentsInChildren<BatteryElectrode>();

        positiveElectrode = electrodes[0];
        negativeElectrode = electrodes[1];
    }

    // Update is called once per frame
    private new void Update()
    {
        base.Update();
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    public override void OnDragStart()
    {
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;

        if (positiveElectrode.wire != null) positiveElectrode.wire.OnDragStart();
        if (negativeElectrode.wire != null) negativeElectrode.wire.OnDragStart();
    }

    public override void OnDragEnd()
    {
        rb.useGravity = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Surface"))
        {
            if (positiveElectrode.wire != null) positiveElectrode.wire.OnDragEnd();
            if (negativeElectrode.wire != null) negativeElectrode.wire.OnDragEnd();
        }
    }
}