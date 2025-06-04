using UnityEngine;
using UnityEngine.UIElements;

public class ToolbarItemVisual : VisualElement
{
    private readonly ToolbarItemObject item;

    public ToolbarItemVisual(ToolbarItemObject item)
    {
        this.item = item;

        name = $"{item.Name}";
        style.height = item.Dimensions.Height *
            Toolbar.SlotDimension.Height;
        style.width = item.Dimensions.Width *
            Toolbar.SlotDimension.Width;
        style.visibility = Visibility.Hidden;

        VisualElement icon = new VisualElement
        {
            style = { backgroundImage = item.SpriteIcon.texture }
        };
        Add(icon);

        icon.AddToClassList("visual-icon");
        AddToClassList("visual-icon-container");
    }

    public void SetPosition(Vector2 pos)
    {
        style.left = pos.x;
        style.top = pos.y;
    }
}