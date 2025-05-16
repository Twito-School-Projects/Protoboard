using System.Collections.Generic;
using UnityEngine;

public class Breadboard : ElectronicComponent
{
    [Header("Terminals")]
    public GameObject topTerminalConnectionObject;

    public GameObject bottomTerminalConnectionObject;
    public GameObject terminalConnectionsParent;

    [Header("Rails")]
    public GameObject topRailConnectionObject;

    public GameObject bottomRailConnectionObject;

    public GameObject railConnectionsParent;

    [HideInInspector]
    public Hole[] topPositiveRail = new Hole[50];

    [HideInInspector]
    public Hole[] topNegativeRail = new Hole[50];

    [HideInInspector]
    public Hole[] bottomNegativeRail = new Hole[50];

    [HideInInspector]
    public Hole[] bottomPositiveRail = new Hole[50];

    [HideInInspector]
    public Dictionary<int, LinkedList<Hole>> topTerminalStrips = new Dictionary<int, LinkedList<Hole>>();

    [HideInInspector]
    public Dictionary<int, LinkedList<Hole>> bottomTerminalStrips = new Dictionary<int, LinkedList<Hole>>();

    private float spacingX = 7.645f - 7.393f;
    private float spacingZ = Mathf.Abs(1.132f - 1.378f);
    private float sectionSpacing = Mathf.Abs(5.672f - 7.161f);

    private int rows = 2;
    private int sections = 10;
    private int numberInSection = 5;

    private Hole[,] topTerminal = new Hole[5, 63];
    private Hole[,] bottomTerminal = new Hole[5, 63];

    public new void Start()
    {
        base.Start();
        //top
        AddTerminalConnectionPoints(true);
        //bottom
        AddTerminalConnectionPoints(false);

        //top
        AddRailConnectionPoints(true);
        //bottom
        AddRailConnectionPoints(false);

        SortTerminals(topTerminal, bottomTerminal);
    }

    // Update is called once per frame
    private new void Update()
    {
        base.Start();
    }

    public void AddTerminalConnectionPoints(bool top)
    {
        Vector3 startingPosition;
        string section;

        if (!top)
        {
            section = "a";
            startingPosition = topTerminalConnectionObject.transform.position;
        }
        else
        {
            section = "b";
            startingPosition = bottomTerminalConnectionObject.transform.position;
        }

        for (int row = 0; row < topTerminal.GetLength(0); row++)
        {
            Transform parent = transform.GetChild(2).GetChild(top ? 0 : 1);
            Hole[,] holesToAddTo = top ? topTerminal : bottomTerminal;

            string rowName;

            if (top)
            {
                rowName = row switch
                {
                    0 => "A",
                    1 => "B",
                    2 => "C",
                    3 => "D",
                    4 => "E",
                    _ => "errror",
                };
            }
            else
            {
                rowName = row switch
                {
                    0 => "F",
                    1 => "G",
                    2 => "H",
                    3 => "I",
                    4 => "J",
                    _ => "errror",
                };
            }

            for (int col = 0; col < topTerminal.GetLength(1); col++)
            {
                Vector3 newPosition = new Vector3(startingPosition.x - (spacingX * col), startingPosition.y, startingPosition.z - (spacingZ * row));
                GameObject hole = Instantiate(topTerminalConnectionObject, newPosition, Quaternion.identity, parent);

                hole.name = (col + 1) + section + ":" + rowName;
                hole.tag = "Hole";

                Hole component = hole.GetComponent<Hole>();
                holesToAddTo[row, col] = component;
            }
        }
    }

    public void AddRailConnectionPoints(bool top)
    {
        Vector3 startingPosition;
        string rowName;

        Hole[] positiveRail;
        Hole[] negativeRail;

        Transform rail;
        int columns = sections * numberInSection;

        if (top)
        {
            startingPosition = topRailConnectionObject.transform.position;
            positiveRail = topPositiveRail;
            negativeRail = topNegativeRail;
        }
        else
        {
            startingPosition = bottomRailConnectionObject.transform.position;
            positiveRail = bottomPositiveRail;
            negativeRail = bottomNegativeRail;
        }

        for (int row = 0; row < rows; row++)
        {
            Hole[] railToAddTo;
            Transform parent;

            //positive or negative rail
            parent = transform.GetChild(3).GetChild(top ? 0 : 1).GetChild(row == 0 ? 0 : 1);
            railToAddTo = row == 0 ? positiveRail : negativeRail;

            for (int section = 0; section < sections; section++)
            {
                for (int col = 0; col < numberInSection; col++)
                {
                    Vector3 newPosition = new Vector3(
                        startingPosition.x - (col * spacingX) - (section * sectionSpacing),
                        startingPosition.y,
                        startingPosition.z + (spacingZ * row)
                    );

                    GameObject hole = Instantiate(topRailConnectionObject, newPosition, Quaternion.identity, parent);
                    hole.name = (row == 0 ? "P" : "N") + ":" + ((section * numberInSection) + col + 1);
                    hole.tag = "Hole";

                    Hole component = hole.GetComponent<Hole>();
                    railToAddTo[col + (section * numberInSection)] = component;
                }
            }
        }
    }

    private void SortTerminals(Hole[,] topHoles, Hole[,] bottomHoles)
    {
        for (int i = 0; i < topHoles.GetLength(0); i++)
        {
            for (int j = 0; j < topHoles.GetLength(1); j++)
            {
                if (!topTerminalStrips.ContainsKey(j))
                {
                    topTerminalStrips.Add(j, new LinkedList<Hole>());
                }

                if (!bottomTerminalStrips.ContainsKey(j))
                {
                    bottomTerminalStrips.Add(j, new LinkedList<Hole>());
                }

                if (topTerminalStrips[j].Count == 5)
                    continue;
                if (bottomTerminalStrips[j].Count == 5)
                    continue;

                topTerminalStrips[j].AddLast(topHoles[i, j]);
                bottomTerminalStrips[j].AddLast(bottomHoles[i, j]);

                //Debug.Log(topHoles[i, j].gameObject.name + " added to top terminal strip " + j);
                //Debug.Log(bottomHoles[i, j].gameObject.name + " added to bottom terminal strip " + j);
            }
        }
    }
}