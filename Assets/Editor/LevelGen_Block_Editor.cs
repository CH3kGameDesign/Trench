using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(LevelGen_Block))]
public class LevelGen_Block_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        LevelGen_Block myTarget = (LevelGen_Block)target;
        DrawDefaultInspector();


        if (GUILayout.Button("Update Lists"))
        {
            myTarget.UpdateLists_Editor();
            SaveChanges(myTarget);
        }
    }
    void SaveChanges(LevelGen_Block myTarget)
    {
        EditorUtility.SetDirty(myTarget);
        AssetDatabase.SaveAssetIfDirty(myTarget);
    }
}
