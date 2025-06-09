using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ArmorManager))]
public class ArmorManager_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        ArmorManager myTarget = (ArmorManager)target;
        DrawDefaultInspector();
        if (GUILayout.Button("Update"))
        {
            myTarget.Collect();
            myTarget.GenerateEnum();
            myTarget.GenerateSprites();
        }
    }
}