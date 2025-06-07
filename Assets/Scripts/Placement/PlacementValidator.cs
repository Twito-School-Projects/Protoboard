
using System.Collections.Generic;
using UnityEngine;

public struct PlacementValidationResult
{
    public bool IsValid;
    public string ErrorMessage;
    public List<string> Warnings;
    
    public PlacementValidationResult(bool isValid, string errorMessage = "", List<string> warnings = null)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
        Warnings = warnings ?? new List<string>();
    }
}

public class PlacementValidator : MonoBehaviour
{
    [SerializeField] private float overlapCheckRadius = 0.5f;
    [SerializeField] private LayerMask componentLayer = 1 << 6;
    
    public PlacementValidationResult ValidatePlacement(Hole hole, Vector3 mousePosition, ComponentData componentData)
    {
        var warnings = new List<string>();
        
        // Check for overlapping components
        if (HasOverlappingComponents(mousePosition))
        {
            return new PlacementValidationResult(false, "Cannot place here - overlapping with existing component");
        }

        if (hole.IsOccupied || hole.isTaken)
        {
            return new PlacementValidationResult(false, "Cannot place here - occupied by another component");
        }
        // Check surface validity
        // if (!IsValidSurface(mousePosition, componentData))
        // {
        //     return new PlacementValidationResult(false, "Invalid placement surface");
        // }
        
        // Check component-specific rules
        var componentValidation = ValidateComponentSpecificRules(componentData);
        if (!componentValidation.IsValid)
        {
            return componentValidation;
        }
        
        // Check for warnings
        CheckPlacementWarnings(componentData, warnings);
        
        return new PlacementValidationResult(true, "", warnings);
    }
    
    private bool HasOverlappingComponents(Vector3 position)
    {
        Collider[] overlapping = Physics.OverlapSphere(position, overlapCheckRadius, componentLayer);
        return overlapping.Length > 0;
    }
    
    private bool IsValidSurface(Vector3 position, ComponentData componentData)
    {
        bool result = Physics.Raycast(position, Vector3.down, 2f, componentData.validSurfaces);
        return result;
    }
    
    private PlacementValidationResult ValidateComponentSpecificRules(ComponentData componentData)
    {
        switch (componentData.componentType)
        {
            case ComponentType.Battery:
                if (!componentData.allowMultiple && ComponentTracker.Instance.battery)
                {
                    return new PlacementValidationResult(false, "Only one battery allowed at a time");
                }
                break;
                
            case ComponentType.Breadboard:
                if (!componentData.allowMultiple && ComponentTracker.Instance.breadboard)
                {
                    return new PlacementValidationResult(false, "Only one breadboard allowed at a time");
                }
                break;
                
            case ComponentType.LED:
            case ComponentType.Resistor:
                if (componentData.requiresBreadboard && !ComponentTracker.Instance.breadboard)
                {
                    return new PlacementValidationResult(false, "Breadboard required for this component");
                }
                if (componentData.requiresPowerSource && !ComponentTracker.Instance.battery)
                {
                    return new PlacementValidationResult(false, "Power source required for this component");
                }
                break;
        }
        
        return new PlacementValidationResult(true);
    }
    
    private void CheckPlacementWarnings(ComponentData componentData, List<string> warnings)
    {
        // Check distance from breadboard
        if (ComponentTracker.Instance.breadboard)
        {
            // float distanceToBreadboard = Vector3.Distance(position, ComponentTracker.Instance.breadboard.transform.position);
            // if (distanceToBreadboard > 5f)
            // {
            //     warnings.Add("Component is far from breadboard - may be harder to connect");
            // }
        }
        
        // Check if power source exists
        if (componentData.requiresPowerSource && !ComponentTracker.Instance.battery)
        {
            warnings.Add("No power source available - component won't function");
        }
    }
}