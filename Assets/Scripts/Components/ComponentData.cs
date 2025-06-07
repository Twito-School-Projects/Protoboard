using UnityEngine;

[CreateAssetMenu(fileName = "New Component Data", menuName = "Electronics/Component Data")]
public class ComponentData : ScriptableObject
{
    [Header("Basic Info")]
    public string componentName;
    public ComponentType componentType;
    public GameObject prefab;
    public Sprite icon;
    
    [Header("Placement Rules")]
    public bool allowMultiple = true;
    public bool requiresBreadboard = false;
    public bool requiresPowerSource = false;
    public float snapRadius = 0.5f;
    
    [Header("Validation")]
    public LayerMask validSurfaces = 1;
    public Vector3 boundsSize = Vector3.one;
    
    [Header("UI")]
    public string description;
    public string tooltip;
}