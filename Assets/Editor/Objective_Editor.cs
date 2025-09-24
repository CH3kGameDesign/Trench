using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Objective))]
public class Objective_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        Objective myTarget = (Objective)target;
        DrawDefaultInspector();
        if (GUILayout.Button("Update"))
        {
            myTarget.Update();
        }
    }
}