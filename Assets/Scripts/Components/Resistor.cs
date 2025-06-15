using UnityEngine;

public class Resistor : ElectronicComponent
{
    public float resistance = 0.1f;
    
    private void Update()
    {
        base.Update();

        if (hasCircuitCompleted)
        {
            
        }
    }
    protected override Vector3 CalculateMidpoint()
    {
        return  new Vector3(
            (cathodeHole.transform.position.x + anodeHole.transform.position.x) / 2,
            transform.position.y,
            cathodeHole.transform.position.z
        );
    }

    public override void OnPlaced()
    {
        base.OnPlaced();
        var anodeNode = CircuitManager.Instance.FindNodeInTrees(anodeHole);
        //CircuitManager.Instance.PropagateResistanceUp(anodeNode, this);
    }

    public float GetVoltageDrop()
    {
        return resistance;
    }
}