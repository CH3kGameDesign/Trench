using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(menuName = "Trench/AssetLists/GunManager", fileName = "New Gun Manager")]
public class GunManager : ScriptableObject
{
    public static GunManager Instance;
    public List<GunClass> list = new List<GunClass>();
    [Space(10)]
    public Camera PF_Camera;
    public Texture2D T_empty;

    public void Setup()
    {
        Instance = this;
        foreach (var item in list)
            item.Setup();
    }

    [System.Serializable]
    public class bulletClass
    {
        public bool B_player = false;
        public PlayerController con_Player;
        public AgentController con_Agent;
        public Ship con_Ship;
        public GunClass con_Gun;

        public float F_speed = 100f;
        public float F_radius = 0.1f;
        public float F_lifeTime = 30f;

        public float F_lockOnTurnSpeed = 60f;

        public GameObject PF_impactHit;

        public float F_damage = 0;

        public HitObject.damageTypeEnum D_damageType = HitObject.damageTypeEnum.bullet;

        public bool breakOnImpact = true;
        public float ragdollHitTimer = -1;
        public float pushBackForce = -1;


        public bulletClass Clone()
        {
            bulletClass _temp = new bulletClass();
            _temp.B_player = B_player;
            _temp.con_Player = con_Player;
            _temp.con_Agent = con_Agent;
            _temp.con_Gun = con_Gun;

            _temp.F_speed = F_speed;
            _temp.F_radius = F_radius;
            _temp.F_lifeTime = F_lifeTime;

            _temp.F_lockOnTurnSpeed = F_lockOnTurnSpeed;
            _temp.PF_impactHit = PF_impactHit;
            _temp.F_damage = F_damage;
            _temp.D_damageType = D_damageType;

            _temp.breakOnImpact = breakOnImpact;
            _temp.ragdollHitTimer = ragdollHitTimer;
            _temp.pushBackForce = pushBackForce;
            return _temp;
        }
    }
    public GunClass GetGunByType(Gun_Type _type, PlayerController _player)
    {
        string _id = _type.ToString().Replace('_', '/');
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i]._id == _id)
                return list[i].Clone(_player);
        }
        Debug.LogError("Couldn't find gun of id: " + _id);
        return null;
    }
    public GunClass GetGunByType(Gun_Type _type, AgentController _player)
    {
        string _id = _type.ToString().Replace('_', '/');
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i]._id == _id)
                return list[i].Clone(_player);
        }
        Debug.LogError("Couldn't find gun of id: " + _id);
        return null;
    }
    public GunClass GetGunByID(string _id, PlayerController _player)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i]._id == _id)
                return list[i].Clone(_player);
        }
        Debug.LogError("Couldn't find gun of id: " + _id);
        return null;
    }
    public GunClass GetGunByInt(int _i, PlayerController _player)
    {
        if (_i >= list.Count)
        {
            Debug.LogError("Requested Int was out of range: " + _i);
            return null;
        }
        return list[_i].Clone(_player);
    }
    public GunClass GetGunByID(string _id, AgentController _agent)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i]._id == _id)
                return list[i].Clone(_agent);
        }
        Debug.LogError("Couldn't find gun of id: " + _id);
        return null;
    }
    public GunClass GetGunByInt(int _i, AgentController _agent)
    {
        if (_i >= list.Count)
        {
            Debug.LogError("Requested Int was out of range: " + _i);
            return null;
        }
        return list[_i].Clone(_agent);
    }
#if UNITY_EDITOR
    [ContextMenu("Tools/Collect")]
    public void Collect()
    {
        string mainPath = Application.dataPath + "/ScriptableObjects/Guns/";
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
                    case ItemClass.enumType.gun:
                        list.Add((GunClass)_temp);
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
        string enumName = "Gun_Type";
        List<string> enumEntries = new List<string>();
        List<GunClass> typeEntries = new List<GunClass>();
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
    [ContextMenu("Tools/GenerateSprites")]
    public void GenerateSprites()
    {
        Camera _camera = Instantiate(PF_Camera);
        _camera.transform.position = Vector3.zero;
        _camera.transform.forward = Vector3.forward;
        Vector3 pos = new Vector3(-0.05f, -0.15f, 6);
        Vector3 rot = new Vector3(0, 0, -45);
        foreach (var item in list)
            item.GenerateTexture(_camera, pos, rot, T_empty);
        DestroyImmediate(_camera.gameObject);
        AssetDatabase.Refresh();
    }
#endif
}
