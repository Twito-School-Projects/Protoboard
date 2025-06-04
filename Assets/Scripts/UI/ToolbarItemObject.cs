using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ToolbarItemObject", menuName = "Scriptable Objects/ToolbarItemObject")]
public class ToolbarItemObject : ScriptableObject
{
    public GameObject Prefab;
    public Sprite SpriteIcon;
    public string Name;
    public Dimension Dimensions;
}

[Serializable]
public class Dimension
{
    public int Width;
    public int Height;
}