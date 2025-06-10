using UnityEngine;
using System.Collections.Generic;
using System;


#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "New Objective List", menuName = "Trench/Objective/List")]
public class Objective : ScriptableObject
{
    public static Objective Instance;
    public List<objectiveClass> list = new List<objectiveClass>();
    [System.Serializable]
    public class objectiveClass
    {
        public Objective_Type _type;
        public Resource_Type _resource;
        [HideInInspector] public objectiveType type;
        [HideInInspector] public Resource.resourceType resource = null;
        [HideInInspector] public int amt = 0;
        public int total = 0;
        [HideInInspector] public bool completed = false;
        public string GetString()
        {
            string _temp = "<b>" + amt.ToString() + "/" + total.ToString() + "</b> " + type.description;
            if (_type == Objective_Type.Collect_Resource)
            {
                if (resource == null)
                    resource = Resource.GetResourceType_Static(_resource);
                _temp += "<b>" + resource._name + "</b> ";
            }
            return _temp;
        }
        public objectiveClass Clone(Objective _objective)
        {
            objectiveClass _temp = new objectiveClass();
            _temp._type = _type;
            _temp.type = _objective.GetObjectiveType(_type);
            _temp._resource = _resource;
            _temp.resource = Resource.GetResourceType_Static(_resource);
            _temp.amt = 0;
            _temp.total = total;
            _temp.completed = false;
            return _temp;
        }
    }
    public List<objectiveType> types = new List<objectiveType>();
    [System.Serializable]
    public class objectiveType
    {
        public string _id;
        public string description;
        public Sprite image;
    }
    public objectiveClass GetObjective_Random()
    {
        int _id = UnityEngine.Random.Range(0, list.Count);
        return list[_id].Clone(this);
    }

    public objectiveType GetObjectiveType(Objective_Type _type)
    {
        string _id = _type.ToString().Replace('_','/');
        foreach (var item in types)
        {
            if (_id == item._id)
                return item;
        }
        Debug.LogError("Couldn't find Objective ID: " + _id);
        return null;
    }
    public void Setup()
    {
        Instance = this;
    }

    public void NewObjectives()
    {
        List<Objective.objectiveClass> _objectives = new List<Objective.objectiveClass>();
        for (int i = 0; i < 3; i++)
        {
            Objective.objectiveClass _temp = GetObjective_Random();
            _objectives.Add(_temp);
        }
        SaveData.objectives = _objectives;
        PlayerController.Instance.Update_Objectives();

        PlayerController.Instance.AH_agentAudioHolder.Play(AgentAudioHolder.type.objectiveGain);
    }

#if UNITY_EDITOR
    [ContextMenu("Tools/GenerateEnum")]
    public void GenerateEnum()
    {
        string enumName = "Objective_Type";
        List<string> enumEntries = new List<string>();
        List<objectiveType> typeEntries = new List<objectiveType>();
        foreach (var item in types)
        {
            if (!enumEntries.Contains(item._id))
            {
                enumEntries.Add(item._id);
                typeEntries.Add(item);
            }
            else
                Debug.LogError("Duplicate ID: " + item._id);
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
                    "	"  + "[Description (\""+ typeEntries[i].description +"\")]" +
                    "	" + "[InspectorName (\"" + typeEntries[i]._id + "\")]" +
                    "	" + typeEntries[i]._id.Replace('/','_') + ","
                    );
            }
            streamWriter.WriteLine("}");
        }
        AssetDatabase.Refresh();
    }
#endif
}
