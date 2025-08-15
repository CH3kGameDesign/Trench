using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelGen_Placeables))]
public class LevelGen_Placeables_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        LevelGen_Placeables myTarget = (LevelGen_Placeables)target;
        DrawDefaultInspector();
        if (GUILayout.Button("Gather Prefabs"))
        {
            myTarget.GatherPrefabs();
        }
        if (GUILayout.Button("Generate Sprites"))
        {
            myTarget.GenerateSprites();
        }
    }
}