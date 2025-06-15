using System;
using System.Linq;
using UnityEngine;

public class LED : ElectronicComponent
{
    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");
    private static readonly int ColorID = Shader.PropertyToID("_Color");
    

    public Renderer lampRenderer;
    public Material lampMaterial;
    public float emissionIntensity = 0.2f;
    public float emissionMultiplier = 5f;
    
    private Color startEmissiveColour;
    private Color startColour;
    private float alpha = 0.6f;
    
    private new void Start()
    {
        base.Start();
        isUnidirectional = false;
        lampMaterial = lampRenderer.materials.First(x => x.name.Contains("lightMat"));
        
        startEmissiveColour = lampMaterial.GetColor(EmissionColorID);
        startColour = lampMaterial.GetColor(ColorID);
    }

    private new void Update()
    {
        base.Update();
        if (anodeHole && cathodeHole)
            hasCircuitCompleted = anodeHole.wasPropagated && cathodeHole.wasPropagated;
        

        if (hasCircuitCompleted)
        {
            lampMaterial.EnableKeyword("_EMISSION");
            SetLedIntensity();
        }
        else
        {
            lampMaterial.DisableKeyword("_EMISSION");
        }
            //CircuitManager.Instance.PrintPaths(CircuitManager.Instance.Trees.First().Value);
    }

    private void SetLedIntensity()
    {
        emissionIntensity = Mathf.Clamp(CalculateEmissionIntensity(), 1, 10 );
        lampMaterial.SetColor(EmissionColorID, startEmissiveColour * emissionIntensity);
    }

    protected override Vector3 CalculateMidpoint()
    {
        return new Vector3(
            (cathodeHole.transform.position.x + anodeHole.transform.position.x) / 2,
            transform.position.y,
            cathodeHole.transform.position.z
        );
    }

    private float CalculateEmissionIntensity()
    {
        return emissionIntensity = Mathf.Clamp(
            cathodeHole.currentVoltage * emissionMultiplier, 
            0.1f, 
            10f
        );
    }
}