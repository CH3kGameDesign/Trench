using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGen_Block : MonoBehaviour
{
    public blockTypeEnum BlockType = blockTypeEnum.corridor;
    public enum blockTypeEnum { corridor, bridge, hangar, deadend, engine, foodhall, crewQuarters, captain, vault, ship }

    public List<Transform> T_architecture = new List<Transform>();
    public List<LevelGen_Bounds> B_bounds = new List<LevelGen_Bounds>();
    public LevelGen_Door[] LGD_Entries = new LevelGen_Door[0];
    public LevelGen_Spawn[] LGS_Spawns = new LevelGen_Spawn[0];
    public Treasure_Point[] TP_TreasurePoints = new Treasure_Point[0];

    [HideInInspector] public int I_roomNum = -1;

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
    }
    void UpdateEntryList()
    {
        LGD_Entries = GetComponentsInChildren<LevelGen_Door>();
    }
    void UpdateDecorList()
    {
        LGS_Spawns = GetComponentsInChildren<LevelGen_Spawn>();
    }
    void UpdateTreasurePoints()
    {
        TP_TreasurePoints = GetComponentsInChildren<Treasure_Point>();
    }

    public void Setup(int levelGenNum, int roomNum)
    {
        I_roomNum = roomNum;
        for (int i = 0; i < B_bounds.Count; i++)
            B_bounds[i].SetID(new Vector3Int(levelGenNum, roomNum, i));
    }

    public void UpdateBoundingBox()
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
    public Vector3 GetRandomPoint(bool _3D = false)
    {
        int _bound = UnityEngine.Random.Range(0, B_bounds.Count);
        return B_bounds[_bound].B_Bounds.bounds.GetRandomPoint(_3D);
    }
}
