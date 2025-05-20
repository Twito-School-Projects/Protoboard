public class BatteryElectrode : ConnectionPoint
{
    private new void Start()
    {
        base.Start();
        powered = true;
        hasConstantCharge = true;
    }

    public override void ConnectToHole(ConnectionPoint hole)
    {
        if (nextConnectedPoint == null)
        {
            nextConnectedPoint = hole;
            if (powered && !hole.powered)
            {
                hole.SetPowered(true);
            }
        }
    }
}