using UnityEngine;

/*
 * new idea, if there is a battery, there cannot be an ardunio vice versa.
 * When dragging the battery onto the mat, it will autoconnect to save me the headache
 *
 *
 */
public class Battery : ElectronicComponent
{
    public BatteryElectrode positiveElectrode;
    public BatteryElectrode negativeElectrode;

    public float voltage = 1.5f;

    private new void Start()
    {
        base.Start();

        if (ComponentTracker.Instance.battery != null)
        {
            Destroy(gameObject);
        }

        ComponentTracker.Instance.battery = this;

        isUnidirectional = false;
        var electrodes = transform.GetComponentsInChildren<BatteryElectrode>();

        positiveElectrode = electrodes[0];
        negativeElectrode = electrodes[1];

        var breadboard = ComponentTracker.Instance.breadboard;
        var startHole = breadboard.rails[3].holes[0];
        var endHole = breadboard.rails[3].holes[breadboard.rails[3].holes.Count - 1];

        if (breadboard.rails.Count == 0)
        {
            Debug.Log("Something is wrong");
        }

        if (Vector3.Distance(startHole.transform.position, positiveElectrode.transform.position) < Vector3.Distance(endHole.transform.position, positiveElectrode.transform.position))
        {
            Debug.Log("Creating wire from the left");
            WireMaker.Instance.CreateWireBetweenTwoPoints(positiveElectrode, startHole);
        } else if (Vector3.Distance(startHole.transform.position, positiveElectrode.transform.position) > Vector3.Distance(endHole.transform.position, positiveElectrode.transform.position))
        {
            Debug.Log("Creating wire from the left");
            WireMaker.Instance.CreateWireBetweenTwoPoints(positiveElectrode, endHole);
        }
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