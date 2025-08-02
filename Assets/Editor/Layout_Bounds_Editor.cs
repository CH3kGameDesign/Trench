using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;
using System.Linq;
using System.Web;
using System.Runtime.InteropServices;
using UnityEditorInternal;

[CustomEditor(typeof(Layout_Bounds))]
public class Layout_Bounds_Editor : Editor
{
    Vector2 scrollPos = Vector2.zero;


    public override void OnInspectorGUI()
    {
        Layout_Bounds myTarget = (Layout_Bounds)target;
        //DrawDefaultInspector();
        GUILayout.Label("Layout");
        CheckSize(myTarget);
        
        Color baseGUIColor = GUI.color;

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        EditorStyles.popup.alignment = TextAnchor.MiddleCenter;
        for (int y = 0; y < myTarget.data.Count; y++)
        {
            EditorGUILayout.BeginHorizontal();

            for (int x = 0; x < myTarget.data[y].d.Count; x++)
            {
                EditorGUILayout.BeginVertical(GUILayout.Width(50));

                Layout_Bounds.block item = myTarget.data[y].d[x];
                colorField(item, myTarget, x, y);
                GUI.color = baseGUIColor;
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
        if (GUI.changed)
            SaveChanges(myTarget);
    }

    void colorField(Layout_Bounds.block _room, Layout_Bounds myTarget, int x, int y)
    {
        if (_room != null)
        {
            switch (_room.roomtype)
            {
                case Layout_Bounds.roomEnum.empty:
                    GUI.color = new Color(0.1f,0.1f,0.1f,0.8f);
                    break;
                case Layout_Bounds.roomEnum.placeable:
                    GUI.color = Color.white;
                    break;
            }
            EditorStyles.popup.fixedHeight = 50;
            EditorStyles.popup.fontStyle = FontStyle.Bold;
            Layout_Bounds.roomEnum _type = (Layout_Bounds.roomEnum)EditorGUILayout.EnumPopup(_room.roomtype, GUILayout.Height(50));
            EditorStyles.popup.fontStyle = FontStyle.Normal; 
            EditorStyles.popup.fixedHeight = EditorStyles.popup.lineHeight;
            switch (_type)
            {
                case Layout_Bounds.roomEnum.addLeft:
                    foreach (var item2 in myTarget.data)
                    {
                        item2.d.Add(new Layout_Bounds.block());
                        for (int i = item2.d.Count - 1; i > x; i--)
                            item2.d[i] = item2.d[i - 1];
                        item2.d[x] = new Layout_Bounds.block();
                    }
                    return;
                case Layout_Bounds.roomEnum.addRight:
                    foreach (var item2 in myTarget.data)
                    {
                        item2.d.Add(new Layout_Bounds.block());
                        for (int i = item2.d.Count - 1; i > x + 1; i--)
                            item2.d[i] = item2.d[i - 1];
                        item2.d[x + 1] = new Layout_Bounds.block();
                    }
                    return;
                case Layout_Bounds.roomEnum.addAbove:
                    myTarget.data.Add(new Layout_Bounds.blockColumn());
                    for (int i = myTarget.data.Count - 1; i > y; i--)
                        myTarget.data[i] = myTarget.data[i - 1];
                    myTarget.data[y] = new Layout_Bounds.blockColumn();
                    return;
                case Layout_Bounds.roomEnum.addBelow:
                    myTarget.data.Add(new Layout_Bounds.blockColumn());
                    for (int i = myTarget.data.Count - 1; i > y + 1; i--)
                        myTarget.data[i] = myTarget.data[i - 1];
                    myTarget.data[y + 1] = new Layout_Bounds.blockColumn();
                    return;
                case Layout_Bounds.roomEnum.removeRow:
                    myTarget.data.RemoveAt(y);
                    return;
                case Layout_Bounds.roomEnum.removeColumn:
                    foreach (var item2 in myTarget.data)
                        item2.d.RemoveAt(x);
                    return;
                default: break;
            }
            _room.roomtype = _type;
        }
    }


    //Ensures the grid exists and has unilateral xCounts
    void CheckSize(Layout_Bounds myTarget)
    {
        //Ensure Not Empty
        if (myTarget.data.Count == 0)
            myTarget.data.Add(new Layout_Bounds.blockColumn());
        //Check Size
        int size = 1;
        foreach (var item in myTarget.data)
            size = Mathf.Max(item.d.Count, size);
        //Ensure All Columns are the same size
        for (int y = 0; y < myTarget.data.Count; y++)
        {
            for (int x = 0; x < size - myTarget.data[y].d.Count; x++)
            {
                myTarget.data[y].d.Add(new Layout_Bounds.block());
            }
        }
    }

    void SaveChanges(Layout_Bounds myTarget)
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