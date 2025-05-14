using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Consumable))]
public class Consumable_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        Consumable myTarget = (Consumable)target;
        DrawDefaultInspector();
        if (GUILayout.Button("Generate Enum"))
        {
            myTarget.GenerateEnum();
        }
    }
}