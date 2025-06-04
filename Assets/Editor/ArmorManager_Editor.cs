using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ArmorManager))]
public class ArmorManager_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        ArmorManager myTarget = (ArmorManager)target;
        DrawDefaultInspector();
        if (GUILayout.Button("Generate Enum"))
        {
            myTarget.GenerateEnum();
        }
        if (GUILayout.Button("Generate Sprites"))
        {
            myTarget.GenerateSprites();
        }
    }
}