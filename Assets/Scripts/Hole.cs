using UnityEngine;

public class Hole : MonoBehaviour
{
    public bool powered;

    private Material material;
    private MeshRenderer MeshRenderer;

    private Color startColor;

    public bool isConnectedToGround;
    public bool isCconnectedToPower;
    public bool isFloating => !isConnectedToGround;

    private void Start()
    {
        MeshRenderer = GetComponent<MeshRenderer>();
        material = MeshRenderer.material;

        startColor = material.color;
        material.color = Color.clear;
    }

    public void Highlight()
    {
        material.color = startColor;
    }

    public void RemoveHighlight()
    {
        material.color = Color.clear;
    }
}