using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Resource))]
public class Resource_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        Resource myTarget = (Resource)target;
        DrawDefaultInspector();
        if (GUILayout.Button("Generate Enum"))
        {
            myTarget.GenerateEnum();
        }
    }
}