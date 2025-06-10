using UnityEngine;
using System.Collections.Generic;
using System;
using static Consumable;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "New Consumable List", menuName = "Trench/Consumables/List")]
public class Consumable : ScriptableObject
{
    public static Consumable Instance;
    public Pickup_Object PF_ConsumablePrefab;
    [System.Serializable]
    public class consumableDrop
    {
        public List<consumableDrop_Single> list = new List<consumableDrop_Single>();

        public List<Consumable_Type> GetDrops()
        {
            List<Consumable_Type> _temp = new List<Consumable_Type>();
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
                CreateConsumableObject(_pos, item, _parent);
            }
        }
    }

    [System.Serializable]
    public class consumableDrop_Single
    {
        public Consumable_Type _type;
        [Range(0,100)] public float _chance = 100;
    }

    [System.Serializable]
    public class consumableClass
    {
        public Consumable_Type _type;
        private consumableType type = null;
        public consumableType GetType(Consumable _consumable = null)
        {
            if (type == null)
            {
                type = _consumable.GetConsumableType(_type);
            }
            return type;
        }
        public int amt = 0;
        public string GetString()
        {
            string _temp = "<b>" + amt.ToString() + "</b> " + type.name;
            return _temp;
        }
        public static consumableClass Create(Consumable_Type _type, int _amt = 1, Consumable _consumable = null)
        {
            consumableClass _temp = new consumableClass();
            _temp._type = _type;
            if (_consumable != null)
                _temp.type = _consumable.GetConsumableType(_type);
            else
                _temp.type = Instance.GetConsumableType(_type);
            _temp.amt = _amt;
            return _temp;
        }
        public consumableClass Clone(Consumable _consumable = null)
        {
            consumableClass _temp = new consumableClass();
            _temp._type = _type;
            if (_consumable != null)
                _temp.type = _consumable.GetConsumableType(_type);
            else
                _temp.type = Instance.GetConsumableType(_type);
            _temp.amt = amt;
            return _temp;
        }
    }
    public List<consumableType> types = new List<consumableType>();
    [System.Serializable]
    public class consumableType
    {
        public string _id;
        public string name;
        public Sprite image;
        public GameObject model;
    }
    public void Setup()
    {
        Instance = this;
    }
    public static consumableType GetConsumableType_Static(Consumable_Type _type)
    {
        string _id = _type.ToString().Replace('_', '/');
        foreach (var item in Instance.types)
        {
            if (_id == item._id)
                return item;
        }
        Debug.LogError("Couldn't find Consumable ID: " + _id);
        return null;
    }
    public consumableType GetConsumableType(Consumable_Type _type)
    {
        string _id = _type.ToString().Replace('_', '/');
        foreach (var item in types)
        {
            if (_id == item._id)
                return item;
        }
        Debug.LogError("Couldn't find Consumable ID: " + _id);
        return null;
    }

    public static void CreateConsumableObject(Vector3 _pos, Consumable_Type _type, Transform _parent = null)
    {
        Pickup_Object GO = Instantiate(Instance.PF_ConsumablePrefab, _parent);
        GO.transform.position = _pos;

        GO.Setup(_type);
    }

    public static bool Use(BaseController _agent, consumableClass _consumable)
    {
        switch (_consumable._type)
        {
            case Consumable_Type.Potion_Health:
                if (_agent.F_curHealth >= _agent.F_maxHealth)
                    return false;
                _agent.OnHeal(_agent.F_maxHealth);
                break;
            case Consumable_Type.Potion_Revive:
                break;
            default:
                return false;
        }
        _consumable.amt -= 1;
        return true;
    }

#if UNITY_EDITOR
    [ContextMenu("Tools/GenerateEnum")]
    public void GenerateEnum()
    {
        string enumName = "Consumable_Type";
        List<string> enumEntries = new List<string>();
        List<consumableType> typeEntries = new List<consumableType>();
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
