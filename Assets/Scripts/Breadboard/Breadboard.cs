using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Breadboard : ElectronicComponent
{
    public Dictionary<int, Terminal> terminals = new Dictionary<int, Terminal>();
    public Dictionary<int, Rail> rails = new Dictionary<int, Rail>();

    public int numberOfColumns = 30;

    public new void Start()
    {
        base.Start();
        SetTerminals();
        SetRails();
    }

    // Update is called once per frame
    private new void Update()
    {
        base.Update();
    }

    private void SetTerminals()
    {
        List<Transform> children = transform.Cast<Transform>().ToList();
        children.RemoveAll(x =>
        {
            string name = x.gameObject.name;
            char row = name[0];

            if (name == "Breadboard" || row == 'R' || row == 'L')
                return true;

            return false;
        });

        for (int i = 1; i <= numberOfColumns * 2; i++)
        {
            terminals.Add(i, new Terminal());
        }

        foreach (var child in children)
        {
            string[] a = child.gameObject.name.Split(".");
            int column = int.Parse(a[1].Substring(1).Trim());
            char row = a[0][0];

            Hole hole = child.AddComponent<Hole>();

            hole.rowChar = row;
            hole.column = column;
            hole.charge = Charge.None;

            int indexOfTerminal = hole.row <= 5 ? column : column + numberOfColumns;

            Terminal parentTerminal = terminals[indexOfTerminal];

            hole.parentTerminal = parentTerminal;
            hole.parentBreadboard = this;

            parentTerminal.holes.Add(hole);
        }
    }

    private void SetRails()
    {
        List<Transform> children = transform.Cast<Transform>().ToList();
        children.RemoveAll(x =>
        {
            string name = x.gameObject.name;
            char row = name[0];

            if (name == "Breadboard" || (row != 'R' && row != 'L'))
                return true;

            return false;
        });

        for (int i = 1; i <= 4; i++)
        {
            rails.Add(i, new Rail(i % 2 == 0 ? Charge.Negative : Charge.Positive));
        }

        foreach (var child in children)
        {
            string[] a = child.gameObject.name.Split(".");
            int column = int.Parse(a[1].Substring(1).Trim());

            string row = a[0];

            char rowPosition = row[0];
            char charge = row[1];

            Hole hole = child.AddComponent<Hole>();

            hole.hasConstantCharge = true;
            hole.rowChar = rowPosition;
            hole.column = column;
            hole.charge = charge == '+' ? Charge.Positive : Charge.Negative;

            int indexOfRail;

            if (rowPosition == 'L')
            {
                if (charge == '+')
                    indexOfRail = 1;
                else
                    indexOfRail = 2;
            }
            else
            {
                if (charge == '+')
                    indexOfRail = 3;
                else
                    indexOfRail = 4;
            }

            Rail parentRail = rails[indexOfRail];
            hole.parentRail = parentRail;
            hole.parentBreadboard = this;

            parentRail.holes.Add(hole);
        }
    }
}