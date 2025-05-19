using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ConversationManager))]
public class ConversationManager_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        ConversationManager myTarget = (ConversationManager)target;
        DrawDefaultInspector();
        if (GUILayout.Button("Generate Enum"))
        {
            myTarget.GenerateEnum();
        }
    }
}