using System.Collections;
using System.Collections.Generic;
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
}
