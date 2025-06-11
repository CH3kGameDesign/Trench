using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;
using System.Linq;


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
    public class consumableClass
    {
        public Consumable_Type _type;
        public int _amt = 0;
        public static consumableClass Create(Consumable_Type _t, int _a)
        {
            consumableClass _temp = new consumableClass();
            _temp._type = _t;
            _temp._amt = _a;
            return _temp;
        }
        public consumableClass Clone()
        {
            consumableClass _temp = new consumableClass();
            _temp._type = _type;
            _temp._amt = _amt;
            return _temp;
        }
        public save CloneToSave()
        {
            save _temp = new save();
            _temp._type = _type;
            _temp._amt = _amt;
            _temp._totalAmt = _amt;
            return _temp;
        }
        public Item_Consumable Get_Item()
        {
            return Consumable.Instance.GetConsumableType(_type);
        }
    }
    [System.Serializable]
    public class save : consumableClass
    {
        public int _totalAmt = 0;
        new public static save Create(Consumable_Type _t, int _a)
        {
            save _temp = new save();
            _temp._type = _t;
            _temp._amt = _a;
            _temp._totalAmt = _a;
            return _temp;
        }
        new public save Clone()
        {
            save _temp = new save();
            _temp._type = _type;
            _temp._amt = _amt;
            _temp._totalAmt = _totalAmt;
            return _temp;
        }
    }
    [System.Serializable]
    public class consumableDrop_Single
    {
        public Consumable_Type _type;
        [Range(0,100)] public float _chance = 100;
    }
    public List<Item_Consumable> list = new List<Item_Consumable>();

    public void Setup()
    {
        Instance = this;
    }
    public static Item_Consumable GetConsumableType_Static(Consumable_Type _type)
    {
        string _id = _type.ToString().Replace('_', '/');
        foreach (var item in Instance.list)
        {
            if (_id == item._id)
                return item;
        }
        Debug.LogError("Couldn't find Consumable ID: " + _id);
        return null;
    }
    public Item_Consumable GetConsumableType(Consumable_Type _type)
    {
        string _id = _type.ToString().Replace('_', '/');
        foreach (var item in list)
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

#if UNITY_EDITOR
    [ContextMenu("Tools/Collect")]
    public void Collect()
    {
        string mainPath = Application.dataPath + "/ScriptableObjects/Items";
        string[] filePaths = Directory.GetFiles(mainPath, "*.asset",
                                         SearchOption.AllDirectories);
        list.Clear();
        foreach (var path in filePaths)
        {
            string _path = "Assets" + path.Substring(Application.dataPath.Length);
            ItemClass _temp = (ItemClass)AssetDatabase.LoadAssetAtPath(_path, typeof(ItemClass));
            if (_temp != null)
            {
                switch (_temp.GetEnumType())
                {
                    case ItemClass.enumType.consumable:
                        list.Add((Item_Consumable)_temp);
                        break;
                    default:
                        break;
                }
            }
        }
        list = list.OrderByDescending(h => h.sortOrder).ToList();
        EditorUtility.SetDirty(this);
    }
    [ContextMenu("Tools/GenerateEnum")]
    public void GenerateEnum()
    {
        string enumName = "Consumable_Type";
        List<string> enumEntries = new List<string>();
        List<Item_Consumable> typeEntries = new List<Item_Consumable>();
        foreach (var item in list)
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
