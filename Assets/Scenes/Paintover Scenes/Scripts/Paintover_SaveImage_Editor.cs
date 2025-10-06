using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Paintover_SaveImage))]
public class Paintover_SaveImage_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        Paintover_SaveImage myTarget = (Paintover_SaveImage)target;
        DrawDefaultInspector();
        if (GUILayout.Button("Update"))
        {
            myTarget.SaveImage();
        }
    }
}
