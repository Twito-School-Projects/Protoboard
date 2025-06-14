using System;
using System.Linq;
using UnityEngine;

public class LED : ElectronicComponent
{
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
    
    public Hole anodeHole;
    public Hole cathodeHole;

    public Transform anodeLocation;
    public Transform cathodeLocation;
    
    public Material lampMaterial;
    public float emissionIntensity = 0.2f;
    
    private Color startEmissiveColour;
    
    private new void Start()
    {
        base.Start();
        isUnidirectional = false;
        startEmissiveColour = lampMaterial.GetColor(EmissionColor);
    }

    private new void Update()
    {
        base.Update();
        if (anodeHole && cathodeHole)
            //hasCircuitCompleted = CircuitManager.Instance.HasValidPath(anodeHole, cathodeHole) && cathodeHole.powered;
        

        if (hasCircuitCompleted)
        {
            SetLedIntensity();
        }
        else
        {
            lampMaterial.SetColor(EmissionColor, startEmissiveColour);
        }
            //CircuitManager.Instance.PrintPaths(CircuitManager.Instance.Trees.First().Value);
    }

    private void SetLedIntensity()
    {
        emissionIntensity = cathodeHole.voltage;
        lampMaterial.SetColor(EmissionColor, startEmissiveColour * emissionIntensity);
    }
    
    public override bool TrySnapToBreadboard(Hole initialHole)
    {
        Hole cathodeHole = initialHole;
        Hole anodeHole = FindNearestHole(anodeLocation.position, cathodeHole);

        if (anodeHole)
        {
            SnapToHole(cathodeLocation, initialHole);
            SnapToHole(anodeLocation, anodeHole);

            this.cathodeHole = initialHole;
            this.anodeHole = anodeHole;
            
            Debug.Log(cathodeHole.name);
            Debug.Log(anodeHole.name);
    
            Vector3 midpoint = new Vector3(
                (cathodeHole.transform.position.x + anodeHole.transform.position.x) / 2,
                transform.position.y,
                cathodeHole.transform.position.z
            );
            transform.position = midpoint;
        }

        // Check if both pins can snap to valid holes
        return anodeHole && cathodeHole && anodeHole != cathodeHole;
    }
    
    public override void OnDragStart()
    {
        base.OnDragStart();
        
        // Clear hole occupancy when starting to drag
        if (anodeHole != null) anodeHole.ClearOccupancy();
        if (cathodeHole != null) cathodeHole.ClearOccupancy();
    }

    protected override void Deleted()
    {
        if (!cathodeHole.powered) return;
        if (isInPlacementMode) return;
        
        //having power flow from the cathode to the anode
        var cathodeNode = CircuitManager.Instance.FindNodeInTrees(cathodeHole);
        var anodeNode = cathodeNode.Children.FirstOrDefault(x => x.Data == anodeHole);

        if (cathodeNode == null)
        {
            throw new Exception("Something is very wrong");
        }
        if (anodeNode == null)
        {
            throw new Exception("Something is very wrong");
        }
    
        CircuitManager.Instance.DisconnectNodes(cathodeNode.Data, anodeNode.Data);
    }
    
    public override void OnPlaced()
    {
        base.OnPlaced();
        if (!cathodeHole.powered) return;
        
        //having power flow from the cathode to the anode
        var cathodeNode = CircuitManager.Instance.FindNodeInTree(TreeType.Battery, cathodeHole);
        var anodeNode = CircuitManager.Instance.FindNodeInTree(TreeType.Battery, anodeHole);

        if (cathodeNode == null)
        {
            Debug.Log("Cathode is not in the tree, something is wrong" % Colorize.Orange);
            return;
        }
        
        anodeNode ??= CircuitManager.Instance.FindNodeInTree(TreeType.DisconnectedBattery, anodeHole);
        if (anodeNode == null)
        {
            anodeNode = CircuitManager.Instance.AddChild(ref cathodeNode, ref anodeHole);
            CircuitManager.Instance.PropagatePower(cathodeNode);
            Debug.Log("anode not in tree, adding it");
        }
        else
        {
            CircuitManager.Instance.ReconnectTree(cathodeHole,anodeHole, TreeType.Battery);
            Debug.Log("anode in tree, reconnecting");
        }
    }
}