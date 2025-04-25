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

[CustomEditor(typeof(Layout_Basic))]
public class Layout_Basic_Editor : Editor
{
    Vector2 scrollPos = Vector2.zero;
    public override void OnInspectorGUI()
    {
        Layout_Basic myTarget = (Layout_Basic)target;
        GUILayout.Label("Layout");
        CheckSize(myTarget);
        
        float screenWidth = EditorGUIUtility.currentViewWidth -40;
        float screenHeight = 1000;
        float width = screenWidth / myTarget.data[0].d.Count;

        Color baseGUIColor = GUI.color;

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        for (int y = 0; y < myTarget.data.Count; y++)
        {
            EditorGUILayout.BeginHorizontal();
            
            for (int x = 0; x < myTarget.data[y].d.Count; x++)
            {
                if (x % 2 == 1)
                    EditorGUILayout.BeginVertical(GUILayout.Width(50));
                else
                    EditorGUILayout.BeginVertical();

                Layout_Basic.block item = myTarget.data[y].d[x];
                switch (item.type)
                {
                    case Layout_Basic.blockType.room:
                        roomField(item, myTarget, x, y);
                        break;
                    case Layout_Basic.blockType.entry:
                        entryField(item, myTarget, x, y);
                        break;
                    case Layout_Basic.blockType.empty:
                        emptyField();
                        break;
                    default:
                        break;
                }
                GUI.color = baseGUIColor;
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
        if (GUI.changed)
            SaveChanges(myTarget);
    }
    void roomField(Layout_Basic.block _room, Layout_Basic myTarget, int x, int y)
    {
        if (_room != null)
        {
            switch (_room.roomtype)
            {
                case Layout_Basic.roomEnum.empty:
                    GUI.color = new Color(0.1f,0.1f,0.1f,0.8f);
                    break;
                case Layout_Basic.roomEnum.bridge:
                    GUI.color = Color.white;
                    break;
                case Layout_Basic.roomEnum.hangar:
                    GUI.color = Color.white;
                    break;
                case Layout_Basic.roomEnum.corridor:
                    GUI.color = Color.grey;
                    break;
            }
            EditorStyles.popup.fixedHeight = 50;
            EditorStyles.popup.alignment = TextAnchor.MiddleCenter;
            EditorStyles.popup.fontStyle = FontStyle.Bold;
            Layout_Basic.roomEnum _type = (Layout_Basic.roomEnum)EditorGUILayout.EnumPopup(_room.roomtype, GUILayout.Height(50));
            EditorStyles.popup.fontStyle = FontStyle.Normal; 
            EditorStyles.popup.fixedHeight = EditorStyles.popup.lineHeight;
            switch (_type)
            {
                case Layout_Basic.roomEnum.addLeft:
                    foreach (var item2 in myTarget.data)
                    {
                        item2.d.Add(new Layout_Basic.block());
                        item2.d.Add(new Layout_Basic.block());
                        for (int i = item2.d.Count - 1; i > x; i--)
                            item2.d[i] = item2.d[i - 1];
                        for (int i = item2.d.Count - 1; i > x; i--)
                            item2.d[i] = item2.d[i - 1];
                        item2.d[x] = new Layout_Basic.block();
                        item2.d[x + 1] = new Layout_Basic.block();
                    }
                    return;
                case Layout_Basic.roomEnum.addRight:
                    foreach (var item2 in myTarget.data)
                    {
                        item2.d.Add(new Layout_Basic.block());
                        item2.d.Add(new Layout_Basic.block());
                        for (int i = item2.d.Count - 1; i > x + 1; i--)
                            item2.d[i] = item2.d[i - 1];
                        for (int i = item2.d.Count - 1; i > x + 1; i--)
                            item2.d[i] = item2.d[i - 1];
                        item2.d[x + 1] = new Layout_Basic.block();
                        item2.d[x + 2] = new Layout_Basic.block();
                    }
                    return;
                case Layout_Basic.roomEnum.addAbove:
                    myTarget.data.Add(new Layout_Basic.blockColumn());
                    myTarget.data.Add(new Layout_Basic.blockColumn());
                    for (int i = myTarget.data.Count - 1; i > y; i--)
                        myTarget.data[i] = myTarget.data[i - 1];
                    for (int i = myTarget.data.Count - 1; i > y; i--)
                        myTarget.data[i] = myTarget.data[i - 1];
                    myTarget.data[y] = new Layout_Basic.blockColumn();
                    myTarget.data[y + 1] = new Layout_Basic.blockColumn();
                    return;
                case Layout_Basic.roomEnum.addBelow:
                    myTarget.data.Add(new Layout_Basic.blockColumn());
                    myTarget.data.Add(new Layout_Basic.blockColumn());
                    for (int i = myTarget.data.Count - 1; i > y + 1; i--)
                        myTarget.data[i] = myTarget.data[i - 1];
                    for (int i = myTarget.data.Count - 1; i > y + 1; i--)
                        myTarget.data[i] = myTarget.data[i - 1];
                    myTarget.data[y + 1] = new Layout_Basic.blockColumn();
                    myTarget.data[y + 2] = new Layout_Basic.blockColumn();
                    return;
                case Layout_Basic.roomEnum.removeRow:
                    myTarget.data.RemoveAt(y);
                    if (myTarget.data.Count > 0)
                        myTarget.data.RemoveAt(y - 1);
                    return;
                case Layout_Basic.roomEnum.removeColumn:
                    foreach (var item2 in myTarget.data)
                        item2.d.RemoveAt(x);
                    if (myTarget.data[0].d.Count > 0)
                        foreach (var item2 in myTarget.data)
                            item2.d.RemoveAt(x - 1);
                    return;
                default: break;
            }
            _room.roomtype = _type;
        }
    }

    void entryField(Layout_Basic.block _entry, Layout_Basic myTarget, int x, int y)
    {
        if (_entry != null)
        {
            Color _tarColor = Color.white;
            switch (_entry.entryType)
            {
                case Layout_Basic.entryTypeEnum.empty:
                    _tarColor = new Color(0.1f,0.1f,0.1f,0.4f);
                    break;
                case Layout_Basic.entryTypeEnum.singleDoor:
                    _tarColor = new Color(0.1f, 1f, 0.1f, 0.75f);
                    break;
                case Layout_Basic.entryTypeEnum.wideDoor:
                    _tarColor = new Color(0.1f, 0.7f, 0.1f, 0.75f);
                    break;
                case Layout_Basic.entryTypeEnum.vent:
                    _tarColor = new Color(0.7f, 0.1f, 0.1f, 0.75f);
                    break;
                case Layout_Basic.entryTypeEnum.shipDoor:
                    break;
                case Layout_Basic.entryTypeEnum.shipPark:
                    break;
                case Layout_Basic.entryTypeEnum.any:
                    _tarColor = new Color(1f, 1f, 1f, 0.75f);
                    break;
                default:
                    break;
            }
            float _baseWidth = EditorStyles.popup.fixedWidth;
            bool _endHor = false;
            if (x % 2 == 1)
            {
                EditorGUILayout.Space(20);
                EditorStyles.popup.fixedWidth = 50;
                if (x > 0 && x < myTarget.data[y].d.Count - 1)
                {
                    if (myTarget.data[y].d[x - 1].roomtype == Layout_Basic.roomEnum.empty ||
                        myTarget.data[y].d[x + 1].roomtype == Layout_Basic.roomEnum.empty)
                    {
                        _tarColor = new Color(0.1f, 0.1f, 0.1f, 0.1f);
                        _entry.entryType = Layout_Basic.entryTypeEnum.empty;
                    }
                }
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                _endHor = true;
                GUI.color = Color.clear;
                EditorGUILayout.TextField("");
                if (y > 0 && y < myTarget.data.Count - 1)
                {
                    if (myTarget.data[y - 1].d[x].roomtype == Layout_Basic.roomEnum.empty ||
                    myTarget.data[y + 1].d[x].roomtype == Layout_Basic.roomEnum.empty)
                    {
                        _tarColor = new Color(0.1f, 0.1f, 0.1f, 0.1f);
                        _entry.entryType = Layout_Basic.entryTypeEnum.empty;
                    }
                }
            }
            GUI.color = _tarColor;
            Layout_Basic.entryTypeEnum _type = (Layout_Basic.entryTypeEnum)EditorGUILayout.EnumPopup(_entry.entryType);
            EditorStyles.popup.fixedWidth = _baseWidth;
            switch (_type)
            {
                case Layout_Basic.entryTypeEnum.shipDoor:
                    _type = _entry.entryType;
                    break;
                case Layout_Basic.entryTypeEnum.shipPark:
                    _type = _entry.entryType;
                    break;
                default:
                    break;
            }
            _entry.entryType = _type;

            if (_endHor)
            {
                GUI.color = Color.clear;
                EditorGUILayout.TextField("");
                EditorGUILayout.EndHorizontal();
            }
        }
    }

    void emptyField()
    {
        GUI.color = Color.clear;
        EditorGUILayout.TextField("");
    }

    //Ensures the grid exists and has unilateral xCounts
    void CheckSize(Layout_Basic myTarget)
    {
        //Ensure Not Empty
        if (myTarget.data.Count == 0)
            myTarget.data.Add(new Layout_Basic.blockColumn());
        //Check Size
        int size = 1;
        foreach (var item in myTarget.data)
            size = Mathf.Max(item.d.Count, size);
        //Ensure All Columns are the same size
        for (int y = 0; y < myTarget.data.Count; y++)
        {
            for (int x = 0; x < size - myTarget.data[y].d.Count; x++)
            {
                int id = 0;
                if (x % 2 == 1) id++;
                if (y % 2 == 1) id++;
                switch (id)
                {
                    case 0:
                        myTarget.data[y].d.Add(new Layout_Basic.block(Layout_Basic.blockType.room));
                        break;
                    case 1:
                        myTarget.data[y].d.Add(new Layout_Basic.block(Layout_Basic.blockType.entry));
                        break;
                    default:
                        myTarget.data[y].d.Add(new Layout_Basic.block(Layout_Basic.blockType.empty));
                        break;
                }
            }
        }
        //Remove Extra Entries
        if (myTarget.data.Count % 2 == 0)
            myTarget.data.RemoveAt(myTarget.data.Count - 1);
        if (myTarget.data[0].d.Count % 2 == 0)
        {
            foreach (var item in myTarget.data)
            {
                item.d.RemoveAt(item.d.Count - 1);
            }
        }
        CheckTypes(myTarget);
    }

    //Ensures all blocks are the right type, replaces errors with new blocks
    void CheckTypes(Layout_Basic myTarget)
    {
        for (int y = 0; y < myTarget.data.Count; y++)
        {
            for (int x = 0; x < myTarget.data[y].d.Count; x++)
            {
                int id = 0;
                if (x % 2 == 1) id++;
                if (y % 2 == 1) id++;
                if ((int)myTarget.data[y].d[x].type != id)
                switch (id)
                {
                    case 0:
                        myTarget.data[y].d[x] = new Layout_Basic.block(Layout_Basic.blockType.room);
                        break;
                    case 1:
                        myTarget.data[y].d[x] = new Layout_Basic.block(Layout_Basic.blockType.entry);
                        break;
                    default:
                        myTarget.data[y].d[x] = new Layout_Basic.block(Layout_Basic.blockType.empty);
                        break;
                }
            }
        }
    }

    void SaveChanges(Layout_Basic myTarget)
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