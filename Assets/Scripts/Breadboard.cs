using UnityEngine;

public class Breadboard : MonoBehaviour
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
    public Hole[,] topTerminal = new Hole[5, 63];

    [HideInInspector]
    public Hole[,] bottomTerminal = new Hole[5, 63];

    [HideInInspector]
    public Hole[] topPositiveRail = new Hole[50];

    [HideInInspector]
    public Hole[] topNegativeRail = new Hole[50];

    [HideInInspector]
    public Hole[] bottomNegativeRail = new Hole[50];

    [HideInInspector]
    public Hole[] bottomPositiveRail = new Hole[50];

    private float spacingX = 7.645f - 7.393f;
    private float spacingZ = Mathf.Abs(1.132f - 1.378f);
    private float sectionSpacing = Mathf.Abs(5.672f - 7.161f);

    private int rows = 2;
    private int sections = 10;
    private int numberInSection = 5;

    public void Start()
    {
        //top
        AddTerminalConnectionPoints(true);
        //bottom
        AddTerminalConnectionPoints(false);

        //top
        AddRailConnectionPoints(true);
        //bottom
        AddRailConnectionPoints(false);
    }

    // Update is called once per frame
    private void Update()
    {
    }

    public void AddTerminalConnectionPoints(bool top)
    {
        Vector3 startingPosition;
        string rowName;

        if (top)
        {
            startingPosition = topTerminalConnectionObject.transform.position;
        }
        else
        {
            startingPosition = bottomTerminalConnectionObject.transform.position;
        }

        for (int row = 0; row < topTerminal.GetLength(0); row++)
        {
            Transform parent = transform.GetChild(2).GetChild(top ? 0 : 1);
            Hole[,] holesToAddTo = top ? topTerminal : bottomTerminal;

            for (int col = 0; col < topTerminal.GetLength(1); col++)
            {
                Vector3 newPosition = new Vector3(startingPosition.x + (spacingX * col), startingPosition.y, startingPosition.z - (spacingZ * row));
                GameObject hole = Instantiate(topTerminalConnectionObject, newPosition, Quaternion.identity, parent);
                hole.name = (row + 1) + ":" + (col + 1);

                Hole component = hole.AddComponent<Hole>();
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
                        startingPosition.x + (col * spacingX) + (section * sectionSpacing),
                        startingPosition.y,
                        startingPosition.z - (spacingZ * row)
                    );

                    GameObject hole = Instantiate(topRailConnectionObject, newPosition, Quaternion.identity, parent);
                    hole.name = (row == 0 ? "P" : "N") + ":" + ((section * numberInSection) + col + 1);

                    Hole component = hole.AddComponent<Hole>();
                    railToAddTo[col + (section * numberInSection)] = component;
                }
            }
        }
    }
}