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
        
        // If in placement mode, show snap preview
        // if (isInPlacementMode)
        // {
        //     CheckForSnapTargets();
        // }
    }
    
    // private void CheckForSnapTargets()
    // {
    //     // Find nearest holes for both pins for visual feedback
    //     anode = FindNearestHole(anodeLocation.position);
    //     cathode = FindNearestHole(cathodeLocation.position);
    //     
    //     // Visual feedback could be added here (highlighting holes, etc.)
    // }
    //
    public override void OnDragEnd()
    {
        base.OnDragEnd();
        
        // Don't handle snapping here anymore - let ComponentPlacementSystem handle it
    }
    
    public override bool TrySnapToBreadboard(Hole initialHole)
    {
        Hole cathodeHole = initialHole;
        Hole anodeHole = FindNearestHole(anodeLocation.position, cathodeHole);

        if (anodeHole)
        {
            SnapToHole(cathodeLocation, initialHole);
            SnapToHole(anodeLocation, anodeHole);

            cathode = initialHole;
            anode = anodeHole;
            
            Debug.Log(cathodeHole.name);
            Debug.Log(anodeHole.name);
    
            Vector3 midpoint = new Vector3(
                (cathodeHole.transform.position.x + anodeHole.transform.position.x) / 2,
                transform.position.y,
                cathodeHole.transform.position.z
            );
            transform.position = midpoint;
        }

        // Check if both pins can snap to valid holes
        return anodeHole && cathodeHole && anodeHole != cathodeHole;
    }
    
    public override void OnDragStart()
    {
        base.OnDragStart();
        
        // Clear hole occupancy when starting to drag
        if (anode != null) anode.ClearOccupancy();
        if (cathode != null) cathode.ClearOccupancy();
    }
}