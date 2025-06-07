using UnityEngine;

public class PlacementPreview : MonoBehaviour
{
    [Header("Preview Materials")]
    [SerializeField] private Material validPreviewMaterial;
    [SerializeField] private Material invalidPreviewMaterial;
    [SerializeField] private Material warningPreviewMaterial;
    
    private Renderer[] previewRenderers;
    private Material[] originalMaterials;
    
    public void SetupPreview(GameObject previewObject)
    {
        return;
        previewRenderers = previewObject.GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[previewRenderers.Length];
        
        for (int i = 0; i < previewRenderers.Length; i++)
        {
            originalMaterials[i] = previewRenderers[i].material;
        }
        
        // Disable colliders for preview
        var colliders = previewObject.GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }
        
        UpdateValidationVisuals(new PlacementValidationResult(true));
    }
    
    public void UpdateValidationVisuals(PlacementValidationResult validationResult)
    {
        return;
        if (previewRenderers == null) return;
        
        Material materialToUse;
        
        if (!validationResult.IsValid)
        {
            materialToUse = invalidPreviewMaterial;
        }
        else if (validationResult.Warnings.Count > 0)
        {
            materialToUse = warningPreviewMaterial;
        }
        else
        {
            materialToUse = validPreviewMaterial;
        }
        
        foreach (var renderer in previewRenderers)
        {
            renderer.material = materialToUse;
        }
    }
    
    public void RestoreOriginalMaterials(GameObject previewObject)
    {
        return;
        if (previewRenderers == null || originalMaterials == null) return;
        
        for (int i = 0; i < previewRenderers.Length && i < originalMaterials.Length; i++)
        {
            if (previewRenderers[i] != null)
            {
                previewRenderers[i].material = originalMaterials[i];
            }
        }
    }
}