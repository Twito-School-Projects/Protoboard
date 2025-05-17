using UnityEngine;

public class Wire : MonoBehaviour
{
    public Hole start;
    public Hole end;

    [SerializeField]
    private MeshRenderer meshRenderer;

    [SerializeField]
    private Material material;

    private void Start()
    {
        material = meshRenderer.materials[0];
    }

    public void ChangeToRandomColour()
    {
        Color randomColor = new Color(Random.value, Random.value, Random.value);
        material.color = randomColor;
    }

    public void Disconnect()
    {
        if (start.powered)
        {
            end.SetPowered(false);
        }

        start.isTaken = false;
        end.isTaken = false;
    }
}