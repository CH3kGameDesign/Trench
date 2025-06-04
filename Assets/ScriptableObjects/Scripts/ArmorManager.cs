using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Trench/AssetLists/ArmorManager", fileName = "New Armor Manager")]
public class ArmorManager : ScriptableObject
{
    public static ArmorManager Instance;
    [System.Serializable]
    public class ArmorClass
    {
        public string _id;
        public string name;
        public Texture2D image;

        public virtual void Equip(RagdollManager _RM, bool _left = true)
        {

        }
        public virtual Armor_Type _enum()
        {
            string _string = _id.Replace('/', '_');
            if (Armor_Type.TryParse(_string, out Armor_Type _temp))
                return _temp;
            Debug.LogError("Can't find Armor_Type Enum:" +  _string);
            return Armor_Type.Helmet_Empty;
        }
        public virtual Transform[] Hooks(RagdollManager _RM, bool _left = true)
        {
            return new Transform[0];
        }
        public virtual void GenerateTexture(Camera _camera, Vector3 pos, Vector3 rot, Texture2D onEmpty = null, GameObject _model = null, string filePath = "Assets/Art/Sprites/Armor/")
        {
            if (_model != null)
            {
                GameObject GO;
                string targetPath = filePath + name + ".png";
                GO = Instantiate(_model);
                GO.transform.position = pos;
                GO.transform.eulerAngles = rot;
                _camera.Render();
                _camera.activeTexture.SaveToFile(targetPath, SetImage);
                DestroyImmediate(GO);
            }
            else
                image = onEmpty;
        }
        public void SetImage(bool set, Texture2D _texture)
        {
            if (set)
                image = _texture;
        }
    }
    [System.Serializable]
    public class HelmetClass : ArmorClass
    {
        public GameObject modelHelmet;

        public override void Equip(RagdollManager _RM, bool _left = true)
        {
            _RM.T_armorPoints[0].DeleteChildren();
            if (modelHelmet != null) Instantiate(modelHelmet, _RM.T_armorPoints[0]);
        }
        public override Transform[] Hooks(RagdollManager _RM, bool _left = true)
        {
            return new Transform[]
                {
                    _RM.T_armorPoints[0]
                };
        }
        public override void GenerateTexture(Camera _camera, Vector3 pos, Vector3 rot, Texture2D onEmpty = null, GameObject _model = null, string filePath = "Assets/Art/Sprites/Armor/")
        {
            filePath += "Helmet/";
            pos = new Vector3(0, -0.16f, 2);
            rot = new Vector3(-90, 90, 0);
            base.GenerateTexture(_camera, pos, rot, onEmpty, modelHelmet, filePath);
        }
    }
    [System.Serializable]
    public class ChestClass : ArmorClass
    {
        public GameObject modelChest;
        public GameObject modelBelt;
        public override void Equip(RagdollManager _RM, bool _left = true)
        {
            _RM.T_armorPoints[1].DeleteChildren();
            _RM.T_armorPoints[2].DeleteChildren();
            if (modelChest != null) Instantiate(modelChest, _RM.T_armorPoints[1]);
            if (modelBelt != null) Instantiate(modelBelt, _RM.T_armorPoints[2]);
        }
        public override Transform[] Hooks(RagdollManager _RM, bool _left = true)
        {
            return new Transform[]
                {
                    _RM.T_armorPoints[2]
                };
        }
        public override void GenerateTexture(Camera _camera, Vector3 pos, Vector3 rot, Texture2D onEmpty = null, GameObject _model = null, string filePath = "Assets/Art/Sprites/Armor/")
        {
            pos = new Vector3(0, -1.33f, 3.3f);
            rot = new Vector3(0, 180, 0);
            filePath += "Chest/";
            base.GenerateTexture(_camera, pos, rot, onEmpty, modelChest, filePath);
        }
    }
    [System.Serializable]
    public class ArmClass : ArmorClass
    {
        public GameObject modelArm;
        public GameObject modelWrist;
        public override void Equip(RagdollManager _RM, bool _left = true)
        {
            if (_left)
            {
                _RM.T_armorPoints[3].DeleteChildren();
                _RM.T_armorPoints[4].DeleteChildren();
                if (modelArm != null) Instantiate(modelArm, _RM.T_armorPoints[3]);
                if (modelWrist != null) Instantiate(modelWrist, _RM.T_armorPoints[4]);
            }
            else
            {
                _RM.T_armorPoints[5].DeleteChildren();
                _RM.T_armorPoints[6].DeleteChildren();
                if (modelArm != null) Instantiate(modelArm, _RM.T_armorPoints[5]);
                if (modelWrist != null) Instantiate(modelWrist, _RM.T_armorPoints[6]);
            }
        }
        public override Transform[] Hooks(RagdollManager _RM, bool _left = true)
        {
            if (_left)
                return new Transform[]
                    {
                        _RM.T_armorPoints[3],
                        _RM.T_armorPoints[4]
                    };
            else
                return new Transform[]
                    {
                        _RM.T_armorPoints[5],
                        _RM.T_armorPoints[6]
                    };
        }
        public override void GenerateTexture(Camera _camera, Vector3 pos, Vector3 rot, Texture2D onEmpty = null, GameObject _model = null, string filePath = "Assets/Art/Sprites/Armor/")
        {
            filePath += "Arm/";
            base.GenerateTexture(_camera, pos, rot, onEmpty, modelArm, filePath);
        }
    }
    [System.Serializable]
    public class LegClass : ArmorClass
    {
        public GameObject modelKnee;
        public GameObject modelShin;
        public GameObject modelFoot;
        public override void Equip(RagdollManager _RM, bool _left = true)
        {
            _RM.T_armorPoints[7].DeleteChildren();
            _RM.T_armorPoints[8].DeleteChildren();
            _RM.T_armorPoints[9].DeleteChildren();
            _RM.T_armorPoints[10].DeleteChildren();
            _RM.T_armorPoints[11].DeleteChildren();
            _RM.T_armorPoints[12].DeleteChildren();
            if (modelKnee != null) Instantiate(modelKnee, _RM.T_armorPoints[7]);
            if (modelShin != null) Instantiate(modelShin, _RM.T_armorPoints[8]);
            if (modelFoot != null) Instantiate(modelFoot, _RM.T_armorPoints[9]);
            if (modelKnee != null) Instantiate(modelKnee, _RM.T_armorPoints[10]);
            if (modelShin != null) Instantiate(modelShin, _RM.T_armorPoints[11]);
            if (modelFoot != null) Instantiate(modelFoot, _RM.T_armorPoints[12]);
        }
        public override Transform[] Hooks(RagdollManager _RM, bool _left = true)
        {
            return new Transform[]
                {
                    _RM.T_armorPoints[7],
                    _RM.T_armorPoints[8],
                    _RM.T_armorPoints[9],
                    _RM.T_armorPoints[10],
                    _RM.T_armorPoints[11],
                    _RM.T_armorPoints[12]
                };
        }
        public override void GenerateTexture(Camera _camera, Vector3 pos, Vector3 rot, Texture2D onEmpty = null, GameObject _model = null, string filePath = "Assets/Art/Sprites/Armor/")
        {
            filePath += "Leg/";
            base.GenerateTexture(_camera, pos, rot, onEmpty, modelKnee, filePath);
        }
    }
    [System.Serializable]
    public class MaterialClass : ArmorClass
    {
        public Material modelMaterial;
        public override void Equip(RagdollManager _RM, bool _left = true)
        {
            _RM.MR_skinnedMeshRenderer.material = modelMaterial;
        }
        public override Transform[] Hooks(RagdollManager _RM, bool _left = true)
        {
            return new Transform[]
                {
                    _RM.T_armorPoints[0],
                    _RM.T_armorPoints[2]
                };
        }

        public override void GenerateTexture(Camera _camera, Vector3 pos, Vector3 rot, Texture2D onEmpty = null, GameObject _model = null, string filePath = "Assets/Art/Sprites/Armor/")
        {
            GameObject GO;
            filePath += "Material/";
            string targetPath = filePath + name + ".png";
            GO = Instantiate(_model);
            GO.GetComponent<MeshRenderer>().material = modelMaterial;
            pos = Vector3.forward * 6;
            GO.transform.position = pos;
            _camera.Render();
            _camera.activeTexture.SaveToFile(targetPath, SetImage);
            DestroyImmediate(GO);
        }
    }
    public ArmorOptionButton PF_ArmorOptionPrefab;
    public List<HelmetClass> helmets = new List<HelmetClass>();
    public List<ChestClass> chests = new List<ChestClass>();
    public List<ArmClass> arms = new List<ArmClass>();
    public List<LegClass> legs = new List<LegClass>();
    public List<MaterialClass> materials = new List<MaterialClass>();
    [Space(10)]
    public Camera PF_Camera;
    public GameObject PF_Sphere;
    public Texture2D T_empty;
    public static ArmorClass GetArmorType_Static(Armor_Type _type)
    {
        return Instance.GetArmorType(_type);
    }
    public ArmorClass GetArmorType(Armor_Type _type)
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
            ArmorOptionButton AOB = Instantiate(PF_ArmorOptionPrefab, _holder);
            AOB.Setup(item);
            AOB.Equipped(item._enum() == _enum);
        }
    }
    public void CreateChestUI(Transform _holder, Armor_Type _enum)
    {
        foreach (var item in chests)
        {
            ArmorOptionButton AOB = Instantiate(PF_ArmorOptionPrefab, _holder);
            AOB.Setup(item);
            AOB.Equipped(item._enum() == _enum);
        }
    }
    public void CreateArmUI(Transform _holder, Armor_Type _enum)
    {
        foreach (var item in arms)
        {
            ArmorOptionButton AOB = Instantiate(PF_ArmorOptionPrefab, _holder);
            AOB.Setup(item);
            AOB.Equipped(item._enum() == _enum);
        }
    }
    public void CreateLegUI(Transform _holder, Armor_Type _enum)
    {
        foreach (var item in legs)
        {
            ArmorOptionButton AOB = Instantiate(PF_ArmorOptionPrefab, _holder);
            AOB.Setup(item);
            AOB.Equipped(item._enum() == _enum);
        }
    }
    public void CreateMaterialUI(Transform _holder, Armor_Type _enum)
    {
        foreach (var item in materials)
        {
            ArmorOptionButton AOB = Instantiate(PF_ArmorOptionPrefab, _holder);
            AOB.Setup(item);
            AOB.Equipped(item._enum() == _enum);
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Tools/GenerateEnum")]
    public void GenerateEnum()
    {
        string enumName = "Armor_Type";
        List<string> enumEntries = new List<string>();
        List<ArmorClass> typeEntries = new List<ArmorClass>();
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
