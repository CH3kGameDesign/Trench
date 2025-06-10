using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GunManager))]
public class GunManager_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        GunManager myTarget = (GunManager)target;
        DrawDefaultInspector();
        if (GUILayout.Button("Update"))
        {
            myTarget.Collect();
            myTarget.GenerateEnum();
            myTarget.GenerateSprites();
        }
    }
}