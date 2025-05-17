using UnityEngine;

public class LED : ElectronicComponent
{
    public Hole anode;
    public Hole cathode;

    public Transform anodeLocation;
    public Transform cathodeLocation;

    private new void Start()
    {
        base.Start();
    }

    private new void Update()
    {
        base.Update();
    }
}