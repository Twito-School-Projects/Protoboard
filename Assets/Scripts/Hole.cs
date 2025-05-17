using UnityEngine;

public class Hole : MonoBehaviour
{
    public bool powered = false;

    private MeshRenderer MeshRenderer;
    private Material material;
    private Color startColor;
    private bool isLockedHighlight = false;

    public char rowChar;
    public int row => rowChar - 'A' + 1; // Convert char to int (A=1, B=2, C=3, etc.)
    public int column;

    public Breadboard parentBreadboard;
    public Terminal parentTerminal;
    public Rail parentRail;

    public Charge charge = Charge.None;
    public bool isTaken = false;

    public Hole nextConnectedHole;
    public Hole previousConnectedHole;

    private async void Start()
    {
        MeshRenderer = GetComponent<MeshRenderer>();
        material = MeshRenderer.material;

        startColor = material.color;
        material.color = Color.clear;

        //await Task.Delay(200);
        //PowerSection();
    }

    private void Update()
    {
        if (powered)
        {
            material.color = charge == Charge.Positive ? Color.red : Color.black;
        }
    }

    public void Highlight(bool lockHighlight = false)
    {
        if (lockHighlight) isLockedHighlight = true;
        material.color = startColor;
    }

    public void RemoveHighlight(bool overrideHighlight = false)
    {
        if ((overrideHighlight && isLockedHighlight) || !isLockedHighlight)
            material.color = Color.clear;
    }

    public void ConnectToHole(Hole hole)
    {
        if (nextConnectedHole == null)
        {
            nextConnectedHole = hole;
            if (powered && !hole.powered)
            {
                hole.SetPowered(true);
                hole.previousConnectedHole = this;
            }
        }
    }

    public void SetPowered(bool state)
    {
        powered = state;
        if (state)
        {
            if (charge != Charge.None)
            {
                parentRail.AddPowerSource(this);
            }
            else
            {
                parentTerminal.AddPowerSource(this);
            }
        }
        else
        {
            if (charge != Charge.None)
            {
                parentRail.RemovePowerSource(this);
            }
            else
            {
                parentTerminal.RemovePowerSource(this);
            }
        }
    }

    public void Debug_SetPoweredOnColour()
    {
        if (charge != Charge.None)
        {
            material.color = charge == Charge.Positive ? Color.red : Color.black;
        }
        else
        {
            material.color = column % 2 == 0 ? Color.green : Color.darkOrange;
        }
    }

    public void Debug_SetPoweredOffColour() => material.color = Color.clear;
}