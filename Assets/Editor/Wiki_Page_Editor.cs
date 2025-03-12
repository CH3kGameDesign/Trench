using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(Wiki_Page))]
public class Wiki_Page_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        Wiki_Page myTarget = (Wiki_Page)target;

        GUILayout.Label("Sections");
        float screenWidth = EditorGUIUtility.currentViewWidth -40;
        for (int i = 0; i < myTarget.data.Count; i++)
        {
            Wiki_Page.section item = myTarget.data[i];
            Wiki_Page.sectionEnum _type = (Wiki_Page.sectionEnum)EditorGUILayout.EnumPopup("Segment Type", item.type);
            EditorGUILayout.BeginHorizontal();
            int columns = 1;
            switch (_type)
            {
                case Wiki_Page.sectionEnum.basic: columns = 1; break;
                case Wiki_Page.sectionEnum.twoSplit: columns = 2; break;
                case Wiki_Page.sectionEnum.threeSplit: columns = 3; break;
                case Wiki_Page.sectionEnum.moveUp: 
                    if (i > 0)
                    {
                        Wiki_Page.section item2 = myTarget.data[i-1];
                        myTarget.data[i - 1] = item;
                        myTarget.data[i] = item2;
                        SaveChanges(myTarget);
                    }
                    return;
                case Wiki_Page.sectionEnum.moveDown:
                    if (i < myTarget.data.Count - 1)
                    {
                        Wiki_Page.section item2 = myTarget.data[i + 1];
                        myTarget.data[i + 1] = item;
                        myTarget.data[i] = item2;
                        SaveChanges(myTarget);
                    }
                    return;
                case Wiki_Page.sectionEnum.remove: 
                    myTarget.data.Remove(item); 
                    i--;
                    SaveChanges(myTarget); 
                    continue;
                default: _type = item.type; break;
            }
            item.type = _type;
            float width = screenWidth / columns;
            GUIStyle style = new GUIStyle(EditorStyles.textArea);
            style.wordWrap = true;
            style.richText = true;
            for (int j = 0; j < columns; j++)
            {
                EditorGUILayout.BeginVertical();
                item.splits[j].splitType = (Wiki_Page.splitEnum)EditorGUILayout.EnumPopup("", item.splits[j].splitType, GUILayout.Width(width));
                switch (item.splits[j].splitType)
                {
                    case Wiki_Page.splitEnum.text:
                        item.splits[j].text = EditorGUILayout.TextArea(item.splits[j].text, style, GUILayout.Width(width));
                        break;
                    case Wiki_Page.splitEnum.image:
                        item.splits[j].image = (Sprite)EditorGUILayout.ObjectField(item.splits[j].image, typeof(Sprite), false, GUILayout.Width(width));
                        break;
                    default:
                        break;
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add"))
            myTarget.data.Add(new Wiki_Page.section());
        EditorGUILayout.EndHorizontal();
        if (GUI.changed)
            SaveChanges(myTarget);
    }
    void SaveChanges(Wiki_Page myTarget)
    {
        EditorUtility.SetDirty(myTarget);
        AssetDatabase.SaveAssetIfDirty(myTarget);
    }
}
/*
// IngredientDrawerUIE
[CustomPropertyDrawer(typeof(Wiki_Page.section))]
public class SectionDrawerUIE : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        // Create property container element.
        var container = new VisualElement();
        // Create property fields.
        var amountField = new PropertyField(property.FindPropertyRelative("type"), "Fancy Name");
        var unitField = new PropertyField(property.FindPropertyRelative("text"));
        var nameField = new PropertyField(property.FindPropertyRelative("image"));

        // Add fields to the container.
        container.Add(amountField);
        container.Add(unitField);
        container.Add(nameField);

        return container;
    }
}
*/