using System;
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

    private Wire positiveWire;
    private Wire negativeWire;
    
    public float voltage = 1.5f;

    public static event Action<Wire, ConnectionPoint, ConnectionPoint> OnBatteryDestroyed;
    
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

        var foundNode = CircuitManager.Instance.FindNodeInTrees(positiveHole);

        if (foundNode != null)
        {
            Debug.Log(foundNode.Data.name);
        }
    }

    private ConnectionPoint ConnectElectrodesToBreadboard(Hole startHole, Hole endHole, BatteryElectrode electrode)
    {
        Hole hole;
        hole = Vector3.Distance(startHole.transform.position, electrode.transform.position) <
               Vector3.Distance(endHole.transform.position, electrode.transform.position) ? startHole : endHole;

        Wire w = WireMaker.Instance.CreateWireBetweenTwoPoints(electrode, hole);

        if (electrode.charge == Charge.Positive) positiveWire = w; 
        else negativeWire = w;
        
        return hole;
    }

    private void OnDestroy()
    {
        positiveWire.end.previousConnectedPoint = null;
        negativeWire.end.previousConnectedPoint = null;

        Destroy(positiveWire.gameObject);
        Destroy(negativeWire.gameObject);
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
    }

    public override void OnDragEnd()
    {
        rb.useGravity = true;
    }
}