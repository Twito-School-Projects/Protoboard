using UnityEngine;

public class SnapSystem : MonoBehaviour
{
    [Header("Snap Settings")]
    [SerializeField] private float gridSize = 1f;
    [SerializeField] private bool enableGridSnap = true;
    [SerializeField] private bool enableComponentSnap = true;
    [SerializeField] private float componentSnapDistance = 1f;
    
    [Header("Breadboard Snapping")]
    [SerializeField] private bool enableBreadboardSnap = true;
    [SerializeField] private LayerMask holeLayer = 1 << 7; // Layer for breadboard holes
    
    public Vector3 GetSnappedPosition(Vector3 worldPosition, ComponentData componentData)
    {
        Vector3 snappedPosition = worldPosition;
        
        // Special handling for breadboard-compatible components
        if (enableBreadboardSnap && RequiresBreadboardSnapping(componentData.componentType))
        {
            var breadboardSnap = TrySnapToBreadboardTwoPegs(worldPosition, componentData);
            if (breadboardSnap.HasValue)
            {
                return breadboardSnap.Value;
            }
        }
        
        // Grid snapping
        // if (enableGridSnap)
        // {
        //     snappedPosition = SnapToGrid(snappedPosition);
        // }
        
        // Component snapping (snap to nearby components)
        // if (enableComponentSnap)
        // {
        //     snappedPosition = SnapToNearbyComponents(snappedPosition, componentData);
        // }
        
        return snappedPosition;
    }
    
    private bool RequiresBreadboardSnapping(ComponentType componentType)
    {
        return componentType == ComponentType.LED || componentType == ComponentType.Resistor;
    }
    
    private Vector3? TrySnapToBreadboardTwoPegs(Vector3 position, ComponentData componentData)
    {
        // Find the breadboard
        var breadboard = ComponentTracker.Instance.breadboard;
        if (!breadboard) return null;
        
        // Get all holes on the breadboard
        Collider[] allHoles = Physics.OverlapSphere(position, breadboard.HoleDistance * 3f, holeLayer);
        if (allHoles.Length < 2) return null;
        
        // Convert to Hole components and filter available ones
        var availableHoles = new System.Collections.Generic.List<Hole>();
        foreach (var collider in allHoles)
        {
            var hole = collider.GetComponent<Hole>();
            if (hole && !hole.IsOccupied)
            {
                availableHoles.Add(hole);
            }
        }
        
        if (availableHoles.Count < 2) return null;
        
        // Get the component peg spacing from the prefab
        var componentPegs = GetComponentPegs(componentData);
        if (componentPegs.Length < 2) return null;
        
        float pegSpacing = Vector3.Distance(componentPegs[0], componentPegs[1]);
        
        // Find the best pair of holes for two pegs
        float bestDistance = float.MaxValue;
        Vector3? bestPosition = null;
        
        for (int i = 0; i < availableHoles.Count; i++)
        {
            for (int j = i + 1; j < availableHoles.Count; j++)
            {
                var hole1 = availableHoles[i];
                var hole2 = availableHoles[j];
                
                // Check if holes are in valid positions
                if (!AreHolesValidForComponent(hole1, hole2, componentData.componentType))
                    continue;
                
                float holeDistance = Vector3.Distance(hole1.transform.position, hole2.transform.position);
                
                // Check if the distance between holes matches component peg spacing (with some tolerance)
                if (Mathf.Abs(holeDistance - pegSpacing) > 0.2f)
                    continue;
                
                // Calculate center position between the two holes
                Vector3 centerPosition = (hole1.transform.position + hole2.transform.position) / 2f;
                float distanceToMouse = Vector3.Distance(position, centerPosition);
                
                if (distanceToMouse < bestDistance)
                {
                    bestDistance = distanceToMouse;
                    bestPosition = centerPosition;
                }
            }
        }
        
        return bestPosition;
    }
    
    private Vector3[] GetComponentPegs(ComponentData componentData)
    {
        // Get the component from the prefab to determine peg positions
        var component = componentData.prefab.GetComponent<ElectronicComponent>();
        if (!component) return new Vector3[0];
        
        if (component is LED led && led.anodeLocation && led.cathodeLocation)
        {
            return new[] { led.anodeLocation.position, led.cathodeLocation.position };
        }
        else if (component is Resistor resistor && resistor.pin1Location && resistor.pin2Location)
        {
            return new[] { resistor.pin1Location.position, resistor.pin2Location.position };
        }
        
        return new Vector3[0];
    }
    
    private bool AreHolesValidForComponent(Hole hole1, Hole hole2, ComponentType componentType)
    {
        // For terminal holes (A-J rows), components should span across the center gap
        if (hole1.IsTerminal && hole2.IsTerminal)
        {
            // Check if holes are in the same column but different terminal groups
            if (hole1.column == hole2.column)
            {
                // One should be in top group (A-E) and other in bottom group (F-J)
                bool hole1IsTop = hole1.row <= 5;
                bool hole2IsTop = hole2.row <= 5;
                return hole1IsTop != hole2IsTop;
            }
            
            // Or adjacent columns in same terminal group
            if (Mathf.Abs(hole1.column - hole2.column) == 1)
            {
                bool hole1IsTop = hole1.row <= 5;
                bool hole2IsTop = hole2.row <= 5;
                return hole1IsTop == hole2IsTop; // Same side of breadboard
            }
        }
        
        // For rail holes, components typically don't span rails
        if (hole1.IsPositiveRail || hole1.IsNegativeRail || hole2.IsPositiveRail || hole2.IsNegativeRail)
        {
            return false;
        }
        
        return false;
    }
    
    private Vector3? TrySnapToBreadboard(Vector3 position, ComponentData componentData)
    {
        // Find the breadboard
        var breadboard = ComponentTracker.Instance.breadboard;
        if (!breadboard) return null;
        
        // Find nearby holes
        Collider[] nearbyHoles = Physics.OverlapSphere(position, breadboard.HoleDistance, holeLayer);
        if (nearbyHoles.Length == 0) return null;
        
        // Find the nearest available hole
        Hole nearestHole = null;
        float nearestDistance = float.MaxValue;
        
        foreach (var collider in nearbyHoles)
        {
            var hole = collider.GetComponent<Hole>();
            if (hole && !hole.IsOccupied)
            {
                float distance = Vector3.Distance(position, hole.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestHole = hole;
                }
            }
        }
        
        return nearestHole?.transform.position;
    }
    
    private Vector3 SnapToGrid(Vector3 position)
    {
        float snappedX = Mathf.Round(position.x / gridSize) * gridSize;
        float snappedZ = Mathf.Round(position.z / gridSize) * gridSize;
        
        return new Vector3(snappedX, position.y, snappedZ);
    }
    
    private Vector3 SnapToNearbyComponents(Vector3 position, ComponentData componentData)
    {
        // Find nearby components
        Collider[] nearbyComponents = Physics.OverlapSphere(position, componentSnapDistance, 1 << 6);
        
        Vector3 closestSnapPoint = position;
        float closestDistance = float.MaxValue;
        
        foreach (var col in nearbyComponents)
        {
            var component = col.GetComponent<ElectronicComponent>();
            if (!component) continue;
            
            // Get snap points from the component
            var snapPoints = GetComponentSnapPoints(component);
            
            foreach (var snapPoint in snapPoints)
            {
                float distance = Vector3.Distance(position, snapPoint);
                if (distance < closestDistance && distance < componentData.snapRadius)
                {
                    closestDistance = distance;
                    closestSnapPoint = snapPoint;
                }
            }
        }
        
        return closestSnapPoint;
    }
    
    private Vector3[] GetComponentSnapPoints(ElectronicComponent component)
    {
        if (component is LED led)
        {
            return new[] { led.cathodeLocation.position, led.anodeLocation.position };
        } else if (component is Resistor resistor)
        {
            return new []{resistor.pin1Location.position, resistor.pin2Location.position};
        }
        
        return new Vector3[0];
    }
}