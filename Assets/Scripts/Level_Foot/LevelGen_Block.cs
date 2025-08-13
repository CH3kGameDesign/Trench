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
    public Transform T_blockHolder;
    public List<Transform> T_architecture = new List<Transform>();
    public Transform T_triggerHolder;
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
        Vector3 _worldMin;
        Vector3 _worldMax;
        UpdateSize_Layout(out _min, out _max, out _worldMin, out _worldMax);
        UpdateDoors_Layout(_min, _max);
        UpdateTexture_Layout(_worldMin, _worldMax);
    }
    void UpdateEntryList()
    {
        LGD_Entries = GetComponentsInChildren<LevelGen_Door>();
    }
    void UpdateDecorList()
    {
        LGS_Spawns = GetComponentsInChildren<LevelGen_Spawn>();
    }
    float _boxShrink = 0.5f;
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
        for (int i = 0; i < T_blockHolder.childCount; i++)
            T_architecture.Add(T_blockHolder.GetChild(i));
        for (int i = 0; i < T_architecture.Count; i++)
        {
            Bounds _temp;
            Collider[] _collider = T_architecture[i].GetComponentsInChildren<Collider>();
            _temp = _collider[0].bounds;
            foreach (var item in _collider)
                _temp.Encapsulate(item.bounds);
            _temp.Expand(-_boxShrink);

            GameObject GO = new GameObject();
            GO.transform.parent = T_triggerHolder;
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
    void UpdateSize_Layout(out Vector3 _min, out Vector3 _max, out Vector3 _worldMin, out Vector3 _worldMax)
    {
        _worldMin = Vector3.one * 10000;
        _worldMax = Vector3.one * -10000;
        Vector3 _temp;

        foreach (var item in B_bounds)
        {
            _temp = item.B_Bounds.bounds.min;
            _worldMin.x = Mathf.Min(_worldMin.x, _temp.x);
            _worldMin.y = Mathf.Min(_worldMin.y, _temp.y);
            _worldMin.z = Mathf.Min(_worldMin.z, _temp.z);
            _temp = item.B_Bounds.bounds.max;
            _worldMax.x = Mathf.Max(_worldMax.x, _temp.x);
            _worldMax.y = Mathf.Max(_worldMax.y, _temp.y);
            _worldMax.z = Mathf.Max(_worldMax.z, _temp.z);
        }
        _min = Vector3Int.FloorToInt(_worldMin / _gridSize) * _gridSize;
        _max = Vector3Int.CeilToInt(_worldMax / _gridSize) * _gridSize;
        size.x = Mathf.RoundToInt((_max.x - _min.x) / _gridSize);
        size.y = Mathf.RoundToInt((_max.z - _min.z) / _gridSize);
    }
    void UpdateDoors_Layout(Vector3 _min, Vector3 _max)
    {
        doors.Clear();
        foreach (var item in LGD_Entries)
        {
            doorClass DC = new doorClass();

            int _rot = (Mathf.RoundToInt(item.transform.eulerAngles.y / 90) + 2) % 4;
            if (_rot < 0) _rot += 4;

            DC._rot = _rot;

            Vector3 _pos = (item.transform.position - _min) / _gridSize;

            switch (_rot)
            {
                case 0: _pos.x += 0.5f; _pos.z += 1f; break;
                case 1: _pos.x += 1f; _pos.z += 0.5f; break;
                case 2: _pos.x += 0.5f; break;
                case 3: _pos.z += 0.5f; break;
                default:
                    break;
            }

            _pos.x = size.x - _pos.x;

            DC._pos = Vector2Int.RoundToInt(new Vector2(_pos.x, size.y - _pos.z));
            doors.Add( DC);
        }
    }
    void UpdateTexture_Layout(Vector3 _min, Vector3 _max)
    {
        Camera _camera = Instantiate(PF_Camera, transform);
        Vector3 _pos = new Vector3(_min.x + _max.x, 0, _min.z + _max.z) / 2;
        _pos.y = _max.y + (Mathf.Max(size.x, size.y) * 2.5f);
        _camera.transform.position = _pos;
        _camera.transform.localEulerAngles = new Vector3(90, 180, 0);

        List<GameObject> ceilings = GetCeilings();
        foreach (GameObject g in ceilings)
            g.SetActive(false);

        GenerateTexture(_camera);

        foreach (GameObject g in ceilings)
            g.SetActive(true);

        DestroyImmediate(_camera.gameObject);
    }
    //FIX AFTER ROOM BUILDER IS USABLE
    List<GameObject> GetCeilings()
    {
        List<GameObject> list = new List<GameObject>();

        foreach (Transform t in T_architecture)
        {
            Transform[] GO = t.GetComponentsInChildren<Transform>();
            foreach(Transform go in GO)
            {
                if (go.name.Contains("Ceiling"))
                    list.Add(go.gameObject);
            }
        }

        return list;
    }

    public virtual void GenerateTexture(Camera _camera,
        string filePath = "Assets/Art/Sprites/Layout/")
    {
            string targetPath = filePath + _name.Replace("/", "_") + ".png";
            _camera.Render();
#if UNITY_EDITOR
            _camera.activeTexture.SaveToFile(targetPath, SetImage);
#endif
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
