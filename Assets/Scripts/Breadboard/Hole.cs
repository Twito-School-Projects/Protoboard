using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Hole : ConnectionPoint
{
    public char rowChar;
    public int row => rowChar - 'A' + 1; // Convert char to int (A=1, B=2, C=3, etc.)
    public int column;

    public Breadboard parentBreadboard;
    public Terminal parentTerminal;
    public Rail parentRail;
    
    public List<Resistor> Resistors = new List<Resistor>();

    public bool IsNegativeRail => parentRail != null && charge == Charge.Negative;
    public bool IsPositiveRail => parentRail != null && charge == Charge.Positive;
    public bool IsTerminal => parentTerminal != null && charge == Charge.None;
    
    public void ClearOccupancy()
    {
        isTaken = false;
        OccupiedBy = null;
    }

    private new void Start()
    {
        base.Start();
        powered = false;
    }

    private void Update()
    {
        // powered = parentTerminal != null ? parentTerminal.Powered :
        //     parentRail != null ? parentRail.Powered : false;

        if (isBeingHighlighted)
            return;

        if (powered)
        {
            Debug_SetPoweredOnColour();
            currentVoltage = 1.0f;
        }
            
        else
            Debug_SetPoweredOffColour();

        currentVoltage = Math.Clamp(startVoltage - Resistors.Sum(x => x.GetVoltageDrop()), 0, startVoltage);
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
            material.color = column % 2 == 0 ? Color.green : Color.darkOrchid;
        }
    }

    public void Debug_SetPoweredOffColour() => material.color = Color.clear;
    
    public override string ToString()
    {
        if (IsPositiveRail || IsNegativeRail)
        {
            return $"{(IsPositiveRail ? "+" : "-")}{(rowChar == 'R' ? "Top:" : "Bottom:")}{column}";
        }
        
        return $"{rowChar}:{column}";
    }
}