using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Stamp_List))]
public class Stamp_List_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        Stamp_List myTarget = (Stamp_List)target;
        DrawDefaultInspector();
        if (GUILayout.Button("Collect Stamps"))
        {
            myTarget.CollectStamps();
        }
    }
}