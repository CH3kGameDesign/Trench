using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelGen_Materials))]
public class LevelGen_Materials_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        LevelGen_Materials myTarget = (LevelGen_Materials)target;
        DrawDefaultInspector();
        if (GUILayout.Button("Generate Sprites"))
        {
            myTarget.GenerateSprites();
        }
    }
}