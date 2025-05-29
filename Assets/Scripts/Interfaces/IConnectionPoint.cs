using UnityEngine;

public interface IConnectionPoint
{
    public MeshRenderer MeshRenderer { get; set; }
    public Material material {get;set;}
    public Color startColor {get;set;}
    public bool IsLockedHighlight { get; set; } 

    public bool HasConstantCharge  {get;set;}

    public bool IsTaken  {get;set;}
    public bool Powered {get;set;}
    public bool IsBeingHighlighted  {get;set;}


    public ConnectionPoint NextConnectedPoint {get;set;}
    public ConnectionPoint PreviousConnectedPoint {get;set;}
    public Charge Charge {get;set;}

    public Wire Wire {get;set;}
    public ConnectionPointType type {get;set;}
}