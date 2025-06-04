using System.Linq;
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
        var breadboard = ComponentTracker.Instance.breadboard;

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

        var startNode =  breadboard.CircuitTree.DepthFirstSearch(breadboard.CircuitTree.Root, start);

        if (startNode != null)
        {
            //removing the child node (not deleting it)
            var endNode = startNode.Children.First(x => x.Data == end);

            if (endNode != null)
            {
                Debug.Log("State: " % Colorize.Gold + endNode.Data.powered);

                breadboard.PropogatePower(endNode);
                startNode.RemoveChildNode(endNode);
                CircuitTree tree = new CircuitTree(null);
                tree.Root = startNode;

                breadboard.DisconnectedCircuitTrees.Add(tree);
            }
        }
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