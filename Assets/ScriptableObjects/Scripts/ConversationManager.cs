using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Trench/AssetLists/ConversationManager", fileName = "New Conversation Manager")]
public class ConversationManager : ScriptableObject
{
    public static ConversationManager Instance;
    public enum languageEnum { english, spanish, japanese };
    public static languageEnum curLanguage = languageEnum.english;
    public List<dialogueClass> dialogueList = new List<dialogueClass>();
    public List<banterClass> banterList = new List<banterClass>();
    [System.Serializable]
    public class conversationClass
    {
        public string id = "";
    }
    [System.Serializable]
    public class dialogueClass : conversationClass
    {
        public List<dStringClass> strings = new List<dStringClass>();
        public List<responseClass> response = new List<responseClass>();
        public followUpClass followUp;
    }
    [System.Serializable]
    public class banterClass : conversationClass
    {
        public List<stringClass> strings = new List<stringClass>();
    }
    [System.Serializable]
    public class stringClass
    {
        public CharacterID speaker;
        [Space(10)]

        public string english = "";
        public string spanish = "";
        public string japanese = "";

        public float speed = 0.01f;

        public string GetName()
        {
            return GetSpeaker().name;
        }

        public characterClass GetSpeaker()
        {
            return Instance.GetCharacter(speaker);
        }

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
    [System.Serializable]
    public class dStringClass : stringClass
    {
        [Space(10)]
        public emotionEnum emotion;
        public bool leftSide = false;
        [Space(10)]
        public CharacterID other = CharacterID.Player;
        public emotionEnum otherEmotion;
        public Sprite GetFace(bool _speaker)
        {
            characterClass _char;
            emotionEnum _emotion;
            if (_speaker)   { _char = Instance.GetCharacter(speaker); _emotion = emotion; }
            else            { _char = Instance.GetCharacter(other); _emotion = otherEmotion; }
            return _char.emotion.Get(_emotion);
        }
        public characterClass GetListener()
        {
            return Instance.GetCharacter(other);
        }
    }
    public enum responseEnum { nothing, dialogue, unityEvent}
    [System.Serializable]
    public class responseClass
    {
        public string english = "";
        public string spanish = "";
        public string japanese = "";
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
        public followUpClass followUp;
    }
    [System.Serializable]
    public class followUpClass
    {
        public responseEnum type = responseEnum.nothing;
        public ConversationID _convo;
        public string _event = "";
    }
    public List<characterClass> characterList = new List<characterClass>();
    [System.Serializable]
    public class characterClass
    {
        public string id = "";
        public string name = "";
        public emotionClass emotion = new emotionClass();
        public RagdollManager PF_ragdoll;
        public ArmorManager.SetClass Armor;
    }
    public enum emotionEnum
    {
        neutral,
        happy,
        angry,
        sad,
    }
    [System.Serializable]
    public class emotionClass
    {
        public Sprite neutral;
        public Sprite happy;
        public Sprite angry;
        public Sprite sad;
        public Sprite Get(emotionEnum _emotion)
        {
            switch (_emotion)
            {
                case emotionEnum.neutral: return neutral;
                case emotionEnum.happy: return happy;
                case emotionEnum.angry: return angry;
                case emotionEnum.sad: return sad;
                default: return neutral;
            }
        }
    }
    public void Setup()
    {
        Instance = this;
    }
    public bool GetConversationByID(string _id, out conversationClass _convo)
    {
        foreach (var item in dialogueList)
            if (item.id == _id)
            {
                _convo = item;
                return true;
            }
        foreach (var item in banterList)
            if (item.id == _id)
            {
                _convo = item;
                return true;
            }
        Debug.LogError("Couldn't find conversation with id: " + _id);
        _convo = null;
        return false;
    }

    public bool GetConversation(ConversationID _type, out dialogueClass _convo)
    {
        string _id = _type.ToString().Replace('_', '/');
        foreach (var item in dialogueList)
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
    public bool GetConversation(ConversationID _type, out banterClass _convo)
    {
        string _id = _type.ToString().Replace('_', '/');
        foreach (var item in banterList)
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
    public characterClass GetCharacter(CharacterID _type)
    {
        string _id = _type.ToString().Replace('_', '/');
        foreach (var item in characterList)
        {
            if (_id == item.id)
                return item;
        }
        Debug.LogError("Couldn't find Character ID: " + _id);
        return new characterClass();
    }

#if UNITY_EDITOR
    [ContextMenu("Tools/GenerateEnum")]
    public void GenerateEnum()
    {
        string enumName = "ConversationID";
        List<string> enumEntries = new List<string>();
        foreach (var item in dialogueList)
        {
            if (!enumEntries.Contains(item.id)) enumEntries.Add(item.id);
            else Debug.LogError("Duplicate ID: " + item.id);
        }
        foreach (var item in banterList)
        {
            if (!enumEntries.Contains(item.id)) enumEntries.Add(item.id);
            else Debug.LogError("Duplicate ID: " + item.id);
        }
        GenerateEnum(enumName, enumEntries);

        enumName = "CharacterID";
        enumEntries = new List<string>();
        foreach (var item in characterList)
        {
            if (!enumEntries.Contains(item.id)) enumEntries.Add(item.id);
            else Debug.LogError("Duplicate ID: " + item.id);
        }
        GenerateEnum(enumName, enumEntries);
        AssetDatabase.Refresh();
    }
    public void GenerateEnum(string enumName, List<string> enumEntries)
    {
        string filePathAndName = "Assets/Scripts/Enums/" + enumName + ".cs"; //The folder Scripts/Enums/ is expected to exist

        using (StreamWriter streamWriter = new StreamWriter(filePathAndName))
        {
            streamWriter.WriteLine("using System.ComponentModel;");
            streamWriter.WriteLine("using UnityEngine;");
            streamWriter.WriteLine("public enum " + enumName);
            streamWriter.WriteLine("{");
            for (int i = 0; i < enumEntries.Count; i++)
            {
                streamWriter.WriteLine(
                    "	" + "[InspectorName (\"" + enumEntries[i] + "\")]" +
                    "	" + enumEntries[i].Replace('/', '_') + ","
                    );
            }
            streamWriter.WriteLine("}");
        }
    }
#endif
}
