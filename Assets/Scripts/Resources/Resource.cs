using UnityEngine;
using System.Collections.Generic;
using System;
using static UnityEditor.Progress;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "New Resource List", menuName = "Trench/Resource/List")]
public class Resource : ScriptableObject
{
    public static Resource Instance;
    public Pickup_Object PF_ResourcePrefab;
    [System.Serializable]
    public class resourceDrop
    {
        public List<resourceDrop_Single> list = new List<resourceDrop_Single>();

        public List<Resource_Type> GetDrops()
        {
            List<Resource_Type> _temp = new List<Resource_Type>();
            foreach (var item in list)
            {
                if (UnityEngine.Random.Range(0f, 1f) <= item._chance)
                    _temp.Add(item._type);
            }

            return _temp;
        }

        public void Drop(Vector3 _pos, Transform _parent = null)
        {
            foreach (var item in GetDrops())
            {
                CreateResourceObject(_pos, item, _parent);
            }
        }
    }

    [System.Serializable]
    public class resourceDrop_Single
    {
        public Resource_Type _type;
        [Range(0,100)] public float _chance = 100;
    }

    [System.Serializable]
    public class resourceClass
    {
        public Resource_Type _type;
        private resourceType type = null;
        public resourceType GetType(Resource _resource = null)
        {
            if (type == null)
            {
                type = _resource.GetResourceType(_type);
            }
            return type;
        }
        public int amt = 0;
        public string GetString()
        {
            string _temp = "<b>" + amt.ToString() + "</b> " + type.name;
            return _temp;
        }
        public static resourceClass Create(Resource_Type _type, int _amt = 1, Resource _resource = null)
        {
            resourceClass _temp = new resourceClass();
            _temp._type = _type;
            if (_resource != null)
                _temp.type = _resource.GetResourceType(_type);
            else
                _temp.type = Instance.GetResourceType(_type);
            _temp.amt = _amt;
            return _temp;
        }
        public resourceClass Clone(Resource _resource = null)
        {
            resourceClass _temp = new resourceClass();
            _temp._type = _type;
            if (_resource != null)
                _temp.type = _resource.GetResourceType(_type);
            else
                _temp.type = Instance.GetResourceType(_type);
            _temp.amt = amt;
            return _temp;
        }
    }
    public List<resourceType> types = new List<resourceType>();
    [System.Serializable]
    public class resourceType
    {
        public string _id;
        public string name;
        public Sprite image;
        public GameObject model;
    }
    public static resourceType GetResourceType_Static(Resource_Type _type)
    {
        string _id = _type.ToString().Replace('_', '/');
        foreach (var item in Instance.types)
        {
            if (_id == item._id)
                return item;
        }
        Debug.LogError("Couldn't find Resource ID: " + _id);
        return null;
    }
    public resourceType GetResourceType(Resource_Type _type)
    {
        string _id = _type.ToString().Replace('_', '/');
        foreach (var item in types)
        {
            if (_id == item._id)
                return item;
        }
        Debug.LogError("Couldn't find Resource ID: " + _id);
        return null;
    }

    public static void CreateResourceObject(Vector3 _pos, Resource_Type _type, Transform _parent = null)
    {
        Pickup_Object GO = Instantiate(Instance.PF_ResourcePrefab, _parent);
        GO.transform.position = _pos;

        GO.Setup(_type);
    }

#if UNITY_EDITOR
    [ContextMenu("Tools/GenerateEnum")]
    public void GenerateEnum()
    {
        string enumName = "Resource_Type";
        List<string> enumEntries = new List<string>();
        List<resourceType> typeEntries = new List<resourceType>();
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
                    "	" + "[Description (\"" + typeEntries[i].name + "\")]" +
                    "	" + "[InspectorName (\"" + typeEntries[i]._id + "\")]" +
                    "	" + typeEntries[i]._id.Replace('/', '_') + ","
                    );
            }
            streamWriter.WriteLine("}");
        }
        AssetDatabase.Refresh();
    }
#endif
}
