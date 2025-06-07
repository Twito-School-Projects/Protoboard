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
        ComponentPlacementSystem.OnPlacementStarted += OnPlacementStarted;
        ComponentPlacementSystem.OnComponentPlaced += OnComponentPlaced;
        ComponentPlacementSystem.OnPlacementCancelled += OnPlacementCancelled;
    }
    
    private void OnDisable()
    {
        ComponentPlacementSystem.OnPlacementStarted -= OnPlacementStarted;
        ComponentPlacementSystem.OnComponentPlaced -= OnComponentPlaced;
        ComponentPlacementSystem.OnPlacementCancelled -= OnPlacementCancelled;
    }
    
    private void SetupUI()
    {
        m_Root = GetComponentInChildren<UIDocument>().rootVisualElement;
        toolbarGrid = m_Root.Q<VisualElement>("Grid");
        
        // foreach (var component in availableComponents)
        // {
        //     CreateToolbarButton(component);
        // }

        foreach (var child in toolbarGrid.Children())
        {
            ComponentData data = child.dataSource as ComponentData;
            child.RegisterCallback((ClickEvent evt) => OnComponentSelected(data));
        }
    }
    
    // private void CreateToolbarButton(ComponentData componentData)
    // {
    //     var button = new VisualElement();
    //     
    //     button.RegisterCallback((ClickEvent evt) => OnComponentSelected(componentData));
    //     button.style.backgroundImage = new StyleBackground(componentData.icon);
    //     button.tooltip = componentData.tooltip;
    //     button.AddToClassList("toolbar-icon");
    //     
    //     toolbarGrid.Add(button);
    // }
    
    private void OnComponentSelected(ComponentData componentData)
    {
        if (!ComponentPlacementSystem.Instance.StartPlacement(componentData))
        {
            Debug.LogWarning($"Could not start placement for {componentData.componentName}");
        }
    }
    
    private void OnPlacementStarted(ComponentData componentData)
    {
        // Update UI to show placement mode
        Debug.Log($"Started placing {componentData.componentName}");
    }
    
    private void OnComponentPlaced(GameObject placedComponent)
    {
        // Update UI after successful placement
        Debug.Log($"Successfully placed {placedComponent.name}");
    }
    
    private void OnPlacementCancelled()
    {
        // Update UI after cancelled placement
        Debug.Log("Placement cancelled");
    }

}