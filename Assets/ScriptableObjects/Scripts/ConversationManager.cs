using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Trench/AssetLists/ConversationManager", fileName = "New Conversation Manager")]
public class ConversationManager : ScriptableObject
{
    public enum languageEnum { english, spanish, japanese };
    public static languageEnum curLanguage = languageEnum.english;
    public List<conversationClass> list = new List<conversationClass>();
    [System.Serializable]
    public class conversationClass
    {
        public string id = "";
        public List<stringClass> strings = new List<stringClass>();
        public bool singleUse = false;
        public bool used = false;

        public bool isUsable()
        {
            return (!singleUse || !used);
        }
    }
    [System.Serializable]
    public class stringClass
    {
        public string speaker = "";
        [Space(10)]

        public string english = "";
        public string spanish = "";
        public string japanese = "";

        public float speed = 0.01f;

        public string GetString()
        {
            switch (curLanguage)
            {
                case languageEnum.english: return english;
                case languageEnum.spanish: return spanish;
                case languageEnum.japanese: return japanese;
                default: return english;
            }
        }
    }
    public bool GetConversationByID(string _id, out conversationClass _convo)
    {
        foreach (var item in list)
            if (item.id == _id)
            {
                _convo = item;
                return true;
            }
        Debug.LogError("Couldn't find conversation with id: " + _id);
        _convo = null;
        return false;
    }

    public bool GetConversation(ConversationID _type, out conversationClass _convo)
    {
        string _id = _type.ToString().Replace('_', '/');
        foreach (var item in list)
        {
            if (_id == item.id)
            {
                _convo = item;
                return true;
            }
        }
        Debug.LogError("Couldn't find Conversation ID: " + _id);
        _convo = null;
        return false;
    }

#if UNITY_EDITOR
    [ContextMenu("Tools/GenerateEnum")]
    public void GenerateEnum()
    {
        string enumName = "ConversationID";
        List<string> enumEntries = new List<string>();
        List<conversationClass> typeEntries = new List<conversationClass>();
        foreach (var item in list)
        {
            if (!enumEntries.Contains(item.id))
            {
                enumEntries.Add(item.id);
                typeEntries.Add(item);
            }
            else
                Debug.LogError("Duplicate ID: " + item.id);
        }
        string filePathAndName = "Assets/Scripts/Enums/" + enumName + ".cs"; //The folder Scripts/Enums/ is expected to exist

        using (StreamWriter streamWriter = new StreamWriter(filePathAndName))
        {
            streamWriter.WriteLine("using System.ComponentModel;");
            streamWriter.WriteLine("using UnityEngine;");
            streamWriter.WriteLine("public enum " + enumName);
            streamWriter.WriteLine("{");
            for (int i = 0; i < typeEntries.Count; i++)
            {
                streamWriter.WriteLine(
                    "	" + "[InspectorName (\"" + typeEntries[i].id + "\")]" +
                    "	" + typeEntries[i].id.Replace('/', '_') + ","
                    );
            }
            streamWriter.WriteLine("}");
        }
        AssetDatabase.Refresh();
    }
#endif
}
