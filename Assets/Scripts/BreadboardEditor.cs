using UnityEditor;

[CustomEditor(typeof(Breadboard))]
public class BreadboardEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Breadboard myScript = (Breadboard)target;
        //if (GUILayout.Button("Add Terminals"))
        //{
        //    myScript.AddTerminalConnectionPoints(true);
        //    myScript.AddTerminalConnectionPoints(false);
        //}

        //if (GUILayout.Button("Add Rails"))
        //{
        //    myScript.AddRailConnectionPoints(true);
        //    myScript.AddRailConnectionPoints(false);
        //}
    }
}