using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.Text;

[CreateAssetMenu(menuName = "Trench/AssetLists/ArmorManager", fileName = "New Armor Manager")]
public class ArmorManager : ScriptableObject
{
    public static ArmorManager Instance;

    [System.Serializable]
    public class SetClass
    {
        public Armor_Helmet helmet;
        public Armor_Chest chest;
        public Armor_Arm armL;
        public Armor_Arm armR;
        public Armor_Leg legs;
        public Armor_Material material;

        public void Equip(RagdollManager RM)
        {
            if (helmet != null) helmet.Equip(RM);
            if (chest != null) chest.Equip(RM);
            if (armL != null) armL.Equip(RM);
            if (armR != null) armR.Equip(RM, false);
            if (legs != null) legs.Equip(RM);
            if (material != null) material.Equip(RM);
        }
    }

    public ArmorOptionButton PF_ArmorOptionPrefab;
    public List<Armor_Helmet> helmets = new List<Armor_Helmet>();
    public List<Armor_Chest> chests = new List<Armor_Chest>();
    public List<Armor_Arm> arms = new List<Armor_Arm>();
    public List<Armor_Leg> legs = new List<Armor_Leg>();
    public List<Armor_Material> materials = new List<Armor_Material>();
    [Space(10)]
    public Camera PF_Camera;
    public GameObject PF_Sphere;
    public Texture2D T_empty;
    public static ArmorPiece GetArmorType_Static(Armor_Type _type)
    {
        return Instance.GetArmorType(_type);
    }
    public ArmorPiece GetArmorType(Armor_Type _type)
    {
        string _id = _type.ToString().Replace('_', '/');
        foreach (var item in helmets)
        {
            if (_id == item._id)
                return item;
        }
        foreach (var item in chests)
        {
            if (_id == item._id)
                return item;
        }
        foreach (var item in arms)
        {
            if (_id == item._id)
                return item;
        }
        foreach (var item in legs)
        {
            if (_id == item._id)
                return item;
        }
        foreach (var item in materials)
        {
            if (_id == item._id)
                return item;
        }
        Debug.LogError("Couldn't find Armor ID: " + _id);
        return null;
    }
    public void Setup()
    {
        Instance = this;
        foreach (var item in helmets)
            item.Setup();
        foreach (var item in chests)
            item.Setup();
        foreach (var item in arms)
            item.Setup();
        foreach (var item in legs)
            item.Setup();
        foreach (var item in materials)
            item.Setup();
        UpdatePlayerArmorSet();
    }
    public static void UpdatePlayerArmorSet()
    {
        SaveData.equippedArmorSet.helmet = (Armor_Helmet)GetArmorType_Static(SaveData.equippedArmor[0]);
        SaveData.equippedArmorSet.chest = (Armor_Chest)GetArmorType_Static(SaveData.equippedArmor[1]);
        SaveData.equippedArmorSet.armL = (Armor_Arm)GetArmorType_Static(SaveData.equippedArmor[2]);
        SaveData.equippedArmorSet.armR = (Armor_Arm)GetArmorType_Static(SaveData.equippedArmor[3]);
        SaveData.equippedArmorSet.legs = (Armor_Leg)GetArmorType_Static(SaveData.equippedArmor[4]);
        SaveData.equippedArmorSet.material = (Armor_Material)GetArmorType_Static(SaveData.equippedArmor[5]);
    }

    public static void EquipArmor_Static(RagdollManager _RM, Armor_Type[] _AT)
    {
        Instance.EquipArmor(_RM, _AT);
    }
    public void EquipArmor(RagdollManager _RM, Armor_Type[] _AT)
    {
        for (int i = 0; i < _AT.Length; i++)
        {
            GetArmorType(_AT[i]).Equip(_RM, i != 3);
        }
    }
    public static void EquipArmorSpecific_Static(RagdollManager _RM, Armor_Type _AT, bool _left = true)
    {
        Instance.EquipArmorSpecific(_RM, _AT, _left);
    }
    public void EquipArmorSpecific(RagdollManager _RM, Armor_Type _AT, bool _left = true)
    {
        GetArmorType(_AT).Equip(_RM, _left);
    }

    public void CreateHelmetUI(Transform _holder, Armor_Type _enum)
    {
        foreach (var item in helmets)
        {
            if (item.ownedAmt > 0)
            {
                ArmorOptionButton AOB = Instantiate(PF_ArmorOptionPrefab, _holder);
                AOB.Setup(item);
                AOB.Equipped(item.GetEnum() == _enum);
            }
        }
    }
    public void CreateChestUI(Transform _holder, Armor_Type _enum)
    {
        foreach (var item in chests)
        {
            if (item.ownedAmt > 0)
            {
                ArmorOptionButton AOB = Instantiate(PF_ArmorOptionPrefab, _holder);
                AOB.Setup(item);
                AOB.Equipped(item.GetEnum() == _enum);
            }
        }
    }
    public void CreateArmUI(Transform _holder, Armor_Type _enum)
    {
        foreach (var item in arms)
        {
            if (item.ownedAmt > 0)
            {
                ArmorOptionButton AOB = Instantiate(PF_ArmorOptionPrefab, _holder);
                AOB.Setup(item);
                AOB.Equipped(item.GetEnum() == _enum);
            }
        }
    }
    public void CreateLegUI(Transform _holder, Armor_Type _enum)
    {
        foreach (var item in legs)
        {
            if (item.ownedAmt > 0)
            {
                ArmorOptionButton AOB = Instantiate(PF_ArmorOptionPrefab, _holder);
                AOB.Setup(item);
                AOB.Equipped(item.GetEnum() == _enum);
            }
        }
    }
    public void CreateMaterialUI(Transform _holder, Armor_Type _enum)
    {
        foreach (var item in materials)
        {
            if (item.ownedAmt > 0)
            {
                ArmorOptionButton AOB = Instantiate(PF_ArmorOptionPrefab, _holder);
                AOB.Setup(item);
                AOB.Equipped(item.GetEnum() == _enum);
            }
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Tools/Collect")]
    public void Collect()
    {
        string mainPath = Application.dataPath + "/ScriptableObjects/Armor";
        string[] filePaths = Directory.GetFiles(mainPath, "*.asset",
                                         SearchOption.AllDirectories);
        helmets.Clear();
        chests.Clear();
        arms.Clear();
        legs.Clear();
        materials.Clear();
        foreach (var path in filePaths)
        {
            string _path = "Assets" + path.Substring(Application.dataPath.Length);    
            ItemClass _temp = (ItemClass)AssetDatabase.LoadAssetAtPath(_path, typeof(ItemClass));
            if (_temp != null)
            {
                switch (_temp.GetEnumType())
                {
                    case ItemClass.enumType.armorHead:
                        helmets.Add((Armor_Helmet)_temp);
                        break;
                    case ItemClass.enumType.armorChest:
                        chests.Add((Armor_Chest)_temp);
                        break;
                    case ItemClass.enumType.armorArm:
                        arms.Add((Armor_Arm)_temp);
                        break;
                    case ItemClass.enumType.armorLeg:
                        legs.Add((Armor_Leg)_temp);
                        break;
                    case ItemClass.enumType.armorMat:
                        materials.Add((Armor_Material)_temp);
                        break;
                    default:
                        break;
                }
            }
        }
        helmets = helmets.OrderByDescending(h => h.sortOrder).ToList();
        chests = chests.OrderByDescending(h => h.sortOrder).ToList();
        arms = arms.OrderByDescending(h => h.sortOrder).ToList();
        legs = legs.OrderByDescending(h => h.sortOrder).ToList();
        materials = materials.OrderByDescending(h => h.sortOrder).ToList();
        EditorUtility.SetDirty(this);
    }
    [ContextMenu("Tools/GenerateEnum")]
    public void GenerateEnum()
    {
        string enumName = "Armor_Type";
        List<string> enumEntries = new List<string>();
        List<ArmorPiece> typeEntries = new List<ArmorPiece>();
        foreach (var item in helmets)
        {
            if (!enumEntries.Contains(item._id))
            {
                enumEntries.Add(item._id);
                typeEntries.Add(item);
            }
            else
                Debug.LogError("Duplicate ID: " + item._id);
        }
        foreach (var item in chests)
        {
            if (!enumEntries.Contains(item._id))
            {
                enumEntries.Add(item._id);
                typeEntries.Add(item);
            }
            else
                Debug.LogError("Duplicate ID: " + item._id);
        }
        foreach (var item in arms)
        {
            if (!enumEntries.Contains(item._id))
            {
                enumEntries.Add(item._id);
                typeEntries.Add(item);
            }
            else
                Debug.LogError("Duplicate ID: " + item._id);
        }
        foreach (var item in legs)
        {
            if (!enumEntries.Contains(item._id))
            {
                enumEntries.Add(item._id);
                typeEntries.Add(item);
            }
            else
                Debug.LogError("Duplicate ID: " + item._id);
        }
        foreach (var item in materials)
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
    [ContextMenu("Tools/GenerateSprites")]
    public void GenerateSprites()
    {
        Camera _camera = Instantiate(PF_Camera);
        _camera.transform.position = Vector3.zero;
        _camera.transform.forward = Vector3.forward;
        Vector3 pos = Vector3.forward * 2;
        Vector3 rot = Vector3.zero;
        foreach (var item in helmets)
            item.GenerateTexture(_camera, pos, rot, T_empty);
        foreach (var item in chests)
            item.GenerateTexture(_camera, pos, rot, T_empty);
        foreach (var item in arms)
            item.GenerateTexture(_camera, pos, rot, T_empty);
        foreach (var item in legs)
            item.GenerateTexture(_camera, pos, rot, T_empty);
        foreach (var item in materials)
            item.GenerateTexture(_camera, pos, rot, T_empty, PF_Sphere);
        DestroyImmediate(_camera.gameObject);
        AssetDatabase.Refresh();
    }
#endif

}
