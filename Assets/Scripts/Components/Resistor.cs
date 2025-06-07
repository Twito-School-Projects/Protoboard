using UnityEngine;

public class Resistor : ElectronicComponent
{
    public Hole pin1;
    public Hole pin2;
    
    public Transform pin1Location;
    public Transform pin2Location;
    
    private void Update()
    {
        base.Update();
        
        // if (isInPlacementMode)
        // {
        //     CheckForSnapTargets();
        // }
    }
    
    // private void CheckForSnapTargets()
    // {
    //     pin1 = FindNearestHole(pin1Location.position);
    //     pin2 = FindNearestHole(pin2Location.position);
    //     
    //     // Add visual feedback here
    // }
    
    public override void OnDragEnd()
    {
        base.OnDragEnd();
        
        // Don't handle snapping here anymore - let ComponentPlacementSystem handle it
    }
    
    public override bool TrySnapToBreadboard(Hole initialHole)
    {
        Hole pin1Hole = initialHole;
        Hole pin2Hole = FindNearestHole(pin2Location.position, pin1Hole);

        if (pin2Hole)
        {
            SnapToHole(pin1Location, initialHole);
            SnapToHole(pin2Location, pin2Hole);

            pin1 = initialHole;
            pin2 = pin2Hole;
            
            Debug.Log(pin1Hole.name);
            Debug.Log(pin2Hole.name);
    
            Vector3 midpoint = new Vector3(
                (pin1Hole.transform.position.x + pin2Hole.transform.position.x) / 2,
                transform.position.y,
                pin1Hole.transform.position.z
            );

            transform.position = midpoint;
        }

        // Check if both pins can snap to valid holes
        return pin2Hole && pin1Hole && pin2Hole != pin1Hole;
    }
    
    public override void OnDragStart()
    {
        base.OnDragStart();
        
        if (pin1 != null) pin1.ClearOccupancy();
        if (pin2 != null) pin2.ClearOccupancy();
    }
}