using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Toolbar : Singleton<Toolbar>
{
   [SerializeField] private List<ComponentData> availableComponents;
    
    private VisualElement m_Root;
    private VisualElement toolbarGrid;
    
    private void OnEnable()
    {
        SetupUI();

    }
    
    private void OnDisable()
    {

    }
    
    private void SetupUI()
    {
        m_Root = GetComponentInChildren<UIDocument>().rootVisualElement;
        toolbarGrid = m_Root.Q<VisualElement>("Grid");
        
        foreach (var child in toolbarGrid.Children())
        {
            ComponentData data = child.dataSource as ComponentData;
            child.RegisterCallback((ClickEvent evt) => OnComponentSelected(data));
        }
    }
    
    
    private void OnComponentSelected(ComponentData componentData)
    {
        if (!ComponentPlacementSystem.Instance.StartPlacement(componentData))
        {
            Debug.LogWarning($"Could not start placement for {componentData.componentName}");
        }
    }
}