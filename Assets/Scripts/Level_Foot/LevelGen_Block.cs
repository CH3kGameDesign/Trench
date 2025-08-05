using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ItemClass;

public class LevelGen_Block : MonoBehaviour
{
    [Header("Info")]
    public blockTypeEnum BlockType = blockTypeEnum.corridor;
    public enum blockTypeEnum { corridor, bridge, hangar, deadend, engine, foodhall, crewQuarters, captain, vault, ship }
    public string _name;
    public Sprite icon;
    public Texture2D layoutTexture;
    public Vector2Int size;
    public int weight;
    public costClass cost;
    [Space(10)]
    [Header("References")]
    public List<Transform> T_architecture = new List<Transform>();
    public List<LevelGen_Bounds> B_bounds = new List<LevelGen_Bounds>();
    public LevelGen_Door[] LGD_Entries = new LevelGen_Door[0];
    public LevelGen_Spawn[] LGS_Spawns = new LevelGen_Spawn[0];
    public Treasure_Point[] TP_TreasurePoints = new Treasure_Point[0];
    public Camera PF_Camera;

    [HideInInspector] public int I_roomNum = -1;

    public List<doorClass> doors = new List<doorClass>();
    [System.Serializable]
    public class doorClass
    {
        public Vector2Int _pos;
        public int _rot;
    }

    public enum entryTypeEnum { singleDoor, wideDoor, vent, shipDoor, shipPark, any}
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateLists_Editor()
    {
        UpdateEntryList();
        UpdateDecorList();
        UpdateBoundingBox();
        UpdateTreasurePoints();

        Vector3 _min;
        Vector3 _max;
        UpdateSize_Layout(out _min, out _max);
        UpdateDoors_Layout(_min);
        UpdateTexture_Layout(_min, _max);
    }
    void UpdateEntryList()
    {
        LGD_Entries = GetComponentsInChildren<LevelGen_Door>();
    }
    void UpdateDecorList()
    {
        LGS_Spawns = GetComponentsInChildren<LevelGen_Spawn>();
    }
    void UpdateBoundingBox()
    {
        //transform.position = Vector3.zero;
        foreach (var item in B_bounds)
        {
            if (item != null)
                DestroyImmediate(item.gameObject);
        }
        B_bounds.Clear();
        T_architecture.Clear();
        for (int i = 0; i < transform.GetChild(2).childCount; i++)
            T_architecture.Add(transform.GetChild(2).GetChild(i));
        for (int i = 0; i < T_architecture.Count; i++)
        {
            Bounds _temp;
            Collider[] _collider = T_architecture[i].GetComponentsInChildren<Collider>();
            _temp = _collider[0].bounds;
            foreach (var item in _collider)
                _temp.Encapsulate(item.bounds);
            _temp.Expand(-0.5f);

            GameObject GO = new GameObject();
            GO.transform.parent = transform.GetChild(3);
            GO.transform.position = _temp.center;
            BoxCollider BC = GO.AddComponent<BoxCollider>();
            BC.size = _temp.size;
            LevelGen_Bounds LGB = GO.AddComponent<LevelGen_Bounds>();
            LGB.Setup(BC);
            //GO.AddComponent<TriggerDisplay>();
            B_bounds.Add(LGB);
            /*
            BoxCollider GO = new GameObject().AddComponent<BoxCollider>();
            GO.transform.position = Vector3.zero;
            GO.center = _temp.center;
            GO.size = _temp.size;
            */
        }
    }
    void UpdateTreasurePoints()
    {
        TP_TreasurePoints = GetComponentsInChildren<Treasure_Point>();
    }

    int _gridSize = 5;
    void UpdateSize_Layout(out Vector3 _min, out Vector3 _max)
    {
        _min = Vector3.one * 10000;
        _max = Vector3.one * -10000;
        Vector3 _temp;

        foreach (var item in B_bounds)
        {
            _temp = item.B_Bounds.bounds.min;
            _min.x = Mathf.Min(_min.x, _temp.x);
            _min.y = Mathf.Min(_min.y, _temp.y);
            _min.z = Mathf.Min(_min.z, _temp.z);
            _temp = item.B_Bounds.bounds.max;
            _min.x = Mathf.Min(_min.x, _temp.x);
            _min.y = Mathf.Min(_min.y, _temp.y);
            _min.z = Mathf.Min(_min.z, _temp.z);
        }
        size.x = Mathf.RoundToInt((_max.x - _min.x)/ _gridSize);
        size.y = Mathf.RoundToInt((_max.z - _min.z) / _gridSize);
    }
    void UpdateDoors_Layout(Vector3 _min)
    {
        doors.Clear();
        foreach (var item in LGD_Entries)
        {
            doorClass DC = new doorClass();

            int _rot = Mathf.RoundToInt((item.transform.localEulerAngles.y / 90) % 4);
            if (_rot < 0) _rot += 4;
            DC._rot = _rot;

            Vector3 _pos = (item.transform.localPosition - _min) / _gridSize;
            DC._pos = Vector2Int.RoundToInt(new Vector2(_pos.x, _pos.z));
        }
    }
    void UpdateTexture_Layout(Vector3 _min, Vector3 _max)
    {
        Camera _camera = Instantiate(PF_Camera);
        Vector3 _pos = new Vector3(_min.x + _max.x, 0, _min.z + _max.z) / 2;
        _pos.y = _max.y + (Mathf.Max(size.x, size.y) * 5);
        _camera.transform.position = _pos;
        _camera.activeTexture.width = size.x * 64;
        _camera.activeTexture.height = size.y * 64;
        _camera.transform.localEulerAngles = new Vector3(90, 180, 0);
    }
    public virtual void GenerateTexture(Camera _camera, Vector3 pos, Vector3 rot, Texture2D onEmpty = null, GameObject _model = null,
        string filePath = "Assets/Art/Sprites/Layout/")
    {
        if (_model != null)
        {
            GameObject GO;
            string targetPath = filePath + name.Replace("/", "_") + ".png";
            GO = Instantiate(_model);
            GO.transform.position = pos;
            GO.transform.eulerAngles = rot;
            _camera.Render();
#if UNITY_EDITOR
            _camera.activeTexture.SaveToFile(targetPath, SetImage);
#endif
            DestroyImmediate(GO);
        }
        else
            layoutTexture = onEmpty;
    }
    public void SetImage(bool set, Texture2D _texture)
    {
        if (set)
            layoutTexture = _texture;
    }
    public void Setup(int levelGenNum, int roomNum)
    {
        I_roomNum = roomNum;
        for (int i = 0; i < B_bounds.Count; i++)
            B_bounds[i].SetID(new Vector3Int(levelGenNum, roomNum, i));
    }

    
    public Vector3 GetRandomPoint(bool _3D = false)
    {
        int _bound = UnityEngine.Random.Range(0, B_bounds.Count);
        return B_bounds[_bound].B_Bounds.bounds.GetRandomPoint(_3D);
    }
}
