using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Toolbar : Singleton<Toolbar>
{
    public List<ToolbarItemObject> ToolbarItems;
    public List<VisualElement> ToolbarVisuals;
    public ElectronicComponent currentlySelected;

    private VisualElement m_Root;
    private VisualElement toolbarGrid;

    private async void Configure()
    {
        m_Root = GetComponentInChildren<UIDocument>().rootVisualElement;
        toolbarGrid = m_Root.Q<VisualElement>("Grid");
        ToolbarVisuals = toolbarGrid.Children().ToList();

        for (int i = 0; i < ToolbarItems.Count; i++)
        {  
            var child = ToolbarVisuals[i];
            var data = child.dataSource as ToolbarItemObject;

            child.style.backgroundImage = new StyleBackground(data.SpriteIcon);
            child.tooltip = ToolbarItems[i].Name;
            child.RegisterCallback<ClickEvent>(OnClickIcon);
        }

        await UniTask.WaitForEndOfFrame();
    }

    private void OnEnable()
    {
        Configure();
        foreach (var child in ToolbarVisuals)
        {
            child.RegisterCallback<ClickEvent>(OnClickIcon);
        }
    }

    private void OnDisable()
    {
        foreach (var child in ToolbarVisuals)
        {
            child.UnregisterCallback<ClickEvent>(OnClickIcon);
        }
    }

    private void OnClickIcon(ClickEvent evt)
    {
        if (PlaceUIObject.Instance.currentlySelectedObject != null)
        {
            PlaceUIObject.Instance.DestroyCurrent();
        }

        var targetVisual = evt.target as VisualElement;
        var data =  targetVisual.dataSource as ToolbarItemObject;

        if (data == null)
        {
            Debug.Log("No icon information found");
            return;
        }

        PlaceUIObject.Instance.Create(data.Prefab);
    }
}