using UnityEngine;

public class Hole : ConnectionPoint
{
    public char rowChar;
    public int row => rowChar - 'A' + 1; // Convert char to int (A=1, B=2, C=3, etc.)
    public int column;

    public Breadboard parentBreadboard;
    public Terminal parentTerminal;
    public Rail parentRail;

    private new void Start()
    {
        base.Start();
        powered = false;
    }

    private void Update()
    {
        powered = parentTerminal != null ? parentTerminal.Powered :
            parentRail != null ? parentRail.Powered : false;

        if (isBeingHighlighted)
            return;
        if (powered)
            {
                Debug_SetPoweredOnColour();
            }
            else
            {
                Debug_SetPoweredOffColour();
            }
    }

    public override void ConnectToHole(ConnectionPoint hole)
    {
        if (nextConnectedPoint == null)
        {
            nextConnectedPoint = hole;
            if (powered && !hole.powered)
            {
                hole.SetPowered(true);
            }
            hole.previousConnectedPoint = this;
        }
    }

    public override void SetPowered(bool state)
    {
        powered = state;
        if (state)
        {
            if (charge != Charge.None)
            {
                if (parentRail != null) parentRail.AddPowerSource(this);
            }
            else
            {
                if (parentTerminal != null) parentTerminal.AddPowerSource(this);
            }
        }
        else
        {
            if (charge != Charge.None)
            {
                if (parentRail != null) parentRail.RemovePowerSource(this);
            }
            else
            {
                if (parentTerminal != null) parentTerminal.RemovePowerSource(this);
            }
        }

        if (nextConnectedPoint != null)
        {
            nextConnectedPoint.SetPowered(state);
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