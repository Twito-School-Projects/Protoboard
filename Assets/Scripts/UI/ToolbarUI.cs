using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class Toolbar : Singleton<Toolbar>
{
    public List<StoredItem> StoredItems;
    public ToolbarItem currentlySelected;

    public static Dimension SlotDimension { get; private set; }
    public Dimension ToolbarDimensions;

    private VisualElement m_Root;
    private VisualElement toolbarGrid;

    private bool m_IsInventoryReady;

    private async void Configure()
    {
        m_Root = GetComponentInChildren<UIDocument>().rootVisualElement;
        toolbarGrid = m_Root.Q<VisualElement>("Grid");
        List<VisualElement> toolbarIcons = toolbarGrid.Children().ToList();

        for (int i = 0; i < StoredItems.Count; i++)
        {
            var child = toolbarIcons[i];
            child.tooltip = StoredItems[i].Details.Name;
            Debug.Log(child.tooltip);

            child.Add(StoredItems[i].RootVisual);
            child.RegisterCallback<ClickEvent>(OnClickIcon);
        }

        VisualElement itemDetails = m_Root.Q<VisualElement>("ToolbarIcon");

        await UniTask.WaitForEndOfFrame();
        ConfigureSlotDimensions();
        m_IsInventoryReady = true;
    }

    private void OnDisable()
    {
        IEnumerable<VisualElement> toolbarIcons = toolbarGrid.Children();

        foreach (var child in toolbarIcons)
        {
            child.UnregisterCallback<ClickEvent>(OnClickIcon);
        }
    }

    private void OnClickIcon(ClickEvent evt)
    {
        Debug.Log("Clicked on items");
    }

    private void ConfigureSlotDimensions()
    {
        VisualElement firstSlot = toolbarGrid.Children().First();

        SlotDimension = new Dimension
        {
            Width = Mathf.RoundToInt(firstSlot.worldBound.width),
            Height = Mathf.RoundToInt(firstSlot.worldBound.height)
        };
    }

    private async Task<bool> GetPositionForItem(VisualElement newItem)
    {
        for (int y = 0; y < ToolbarDimensions.Height; y++)
        {
            for (int x = 0; x < ToolbarDimensions.Width; x++)
            {
                //try position
                SetItemPosition(newItem, new Vector2(SlotDimension.Width * x,
                    SlotDimension.Height * y));

                await UniTask.WaitForEndOfFrame();

                StoredItem overlappingItem = StoredItems.FirstOrDefault(s =>
                    s.RootVisual != null &&
                    s.RootVisual.layout.Overlaps(newItem.layout));

                //Nothing is here! Place the item.
                if (overlappingItem == null)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private static void SetItemPosition(VisualElement element, Vector2 vector)
    {
        element.style.left = vector.x;
        element.style.top = vector.y;
    }

    private void Start() => LoadInventory();

    protected override void ExtraAwake() => Configure();

    private async void LoadInventory()
    {
        await UniTask.WaitUntil(() => m_IsInventoryReady);

        foreach (StoredItem loadedItem in StoredItems)
        {
            ToolbarItemVisual toolbarItemVisual = new ToolbarItemVisual(loadedItem.Details);

            AddItemToInventoryGrid(toolbarItemVisual);

            bool inventoryHasSpace = await GetPositionForItem(toolbarItemVisual);

            if (!inventoryHasSpace)
            {
                Debug.Log("No space - Cannot pick up the item");
                RemoveItemFromInventoryGrid(toolbarItemVisual);
                continue;
            }

            ConfigureInventoryItem(loadedItem, toolbarItemVisual);
        }
    }

    private void AddItemToInventoryGrid(VisualElement item) => toolbarGrid.Add(item);

    private void RemoveItemFromInventoryGrid(VisualElement item) => toolbarGrid.Remove(item);

    private static void ConfigureInventoryItem(StoredItem item, ToolbarItemVisual visual)
    {
        item.RootVisual = visual;
        visual.style.visibility = Visibility.Visible;
    }
}