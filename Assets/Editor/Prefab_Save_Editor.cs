using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Prefab_Save))]
public class Prefab_Save_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        Prefab_Save myTarget = (Prefab_Save)target;
        DrawDefaultInspector();
        if (GUILayout.Button("Save Prefabs"))
        {
            myTarget.SavePrefabs();
        }
    }
}
