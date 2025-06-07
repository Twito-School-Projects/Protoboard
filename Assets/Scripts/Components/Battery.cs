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
            if (!PlaceUIObject.Instance.isPlacingObject && PlaceUIObject.Instance.currentlySelectedComponent.componentType == componentType)
                Destroy(gameObject);
        }

        ComponentTracker.Instance.battery = this;

        isUnidirectional = false;
        var electrodes = transform.GetComponentsInChildren<BatteryElectrode>();

        positiveElectrode = electrodes[0];
        negativeElectrode = electrodes[1];

        var breadboard = ComponentTracker.Instance.breadboard;
        var startPositiveHole = breadboard.rails[3].holes[0];
        var endPositiveHole = breadboard.rails[3].holes[24];

        var startNegativeHole = breadboard.rails[2].holes[0];
        var endNegativeHole = breadboard.rails[2].holes[24];

        if (breadboard.rails.Count == 0)
        {
            Debug.Log("Something is wrong");
        }

        var positiveHole = ConnectElectrodesToBreadboard(startPositiveHole, endPositiveHole, positiveElectrode);
        var negativeHole = ConnectElectrodesToBreadboard(startNegativeHole, endNegativeHole, negativeElectrode);

        breadboard.SetRootCircuitNode(positiveElectrode, ((Hole)positiveHole).parentRail);
        var foundNode = breadboard.CircuitTree.DepthFirstSearch(breadboard.CircuitTree.Root, positiveHole);

        if (foundNode != null)
        {
            Debug.Log(foundNode.Data.name);
        }
    }

    private ConnectionPoint ConnectElectrodesToBreadboard(Hole startHole, Hole endHole, BatteryElectrode electrode)
    {
        if (Vector3.Distance(startHole.transform.position, electrode.transform.position) < Vector3.Distance(endHole.transform.position, electrode.transform.position))
        {
            Debug.Log("Creating wire from the left");
            WireMaker.Instance.CreateWireBetweenTwoPoints(electrode, startHole);
            return startHole;
        }
        else
        {
            Debug.Log("Creating wire from the right");
            WireMaker.Instance.CreateWireBetweenTwoPoints(electrode, endHole);
            return endHole;
        }
    }

    private void OnDestroy()
    {
        positiveElectrode.wire.end.previousConnectedPoint = null;
        negativeElectrode.wire.end.previousConnectedPoint = null;

        positiveElectrode.wire.end.wire = null;
        negativeElectrode.wire.end.wire = null;

        Destroy(positiveElectrode.wire.gameObject);
        Destroy(negativeElectrode.wire.gameObject);

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

        positiveElectrode.wire?.OnDragStart();
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