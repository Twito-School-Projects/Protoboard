using UnityEngine;

public class Wire : MonoBehaviour
{
    public ConnectionPoint start;
    public ConnectionPoint end;

    [SerializeField]
    private MeshRenderer meshRenderer;

    [SerializeField]
    private Material material;

    [HideInInspector]
    public BoxCollider boxCollider;

    private void Start()
    {
        material = meshRenderer.materials[0];
        boxCollider = GetComponent<BoxCollider>();
    }

    private void Update()
    {
        float distance = Vector3.Distance(start.transform.position, end.transform.position);

        Vector3 midPoint = (start.transform.position + end.transform.position) / 2;
        Quaternion rotation = Quaternion.LookRotation(end.transform.position - start.transform.position, Vector3.up);

        transform.position = midPoint;
        transform.rotation = rotation;
        transform.localScale = new Vector3(
          transform.localScale.x,
          transform.localScale.y,
          distance
        );
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

        start.nextConnectedPoint = null;
        end.previousConnectedPoint = null;

        start.isTaken = false;
        end.isTaken = false;
        
        start.wire = null;
        end.wire = null;

    }

    public void OnDragStart()
    {
        boxCollider.enabled = false;
    }

    public void OnDragEnd()
    {
        boxCollider.enabled = true;
    }
}