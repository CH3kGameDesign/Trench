using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;
using Unity.Mathematics;

public class LevelGen : MonoBehaviour
{
    public uint seed = new uint();
    private Unity.Mathematics.Random Random_Seeded;
    public LevelGen_Theme LG_Theme;
    private NavMeshSurface[] nm_Surfaces;

    private List<LevelGen_Block> LG_Blocks = new List<LevelGen_Block>();

    public LayerMask LM_mask;

    private int i_series = 2;
    private int i_attempts = 10;

    // Start is called before the first frame update
    void Start()
    {
        Setup();
    }

    void Setup()
    {
        if (seed == uint.MinValue)
            seed = (uint)UnityEngine.Random.Range(0, int.MaxValue);
        Random_Seeded = new Unity.Mathematics.Random(seed);
        nm_Surfaces = GetComponents<NavMeshSurface>();
        GenerateLayout(LG_Theme);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector3 GetRandomPoint()
    {
        int _block = UnityEngine.Random.Range(0, LG_Blocks.Count);
        int _bound = UnityEngine.Random.Range(0, LG_Blocks[_block].B_bounds.Count);
        return LG_Blocks[_block].B_bounds[_bound].B_Bounds.center + LG_Blocks[_block].B_bounds[_bound].B_Bounds.transform.position;
    }

    public void GenerateLayout(LevelGen_Theme _theme)
    {
        LG_Blocks = new List<LevelGen_Block>();
        Transform lHolder = new GameObject("LayoutHolder").transform;
        lHolder.parent = transform;
        lHolder.localPosition = Vector3.zero;

        LevelGen_Block _bridge = Instantiate(_theme.GetBlock(LevelGen_Block.blockTypeEnum.bridge, LevelGen_Block.entryTypeEnum.any, Random_Seeded), lHolder);
        _bridge.transform.localPosition = Vector3.zero;
        LG_Blocks.Add(_bridge);

        GenerateBuildings(LG_Blocks, _theme, lHolder, i_series);
        UpdateNavMeshes();
        SpawnObjects();
    }

    public void UpdateNavMeshes()
    {
        foreach (var item in nm_Surfaces)
        {
            item.BuildNavMesh();
        }
    }

    private void SpawnObjects()
    {
        GameObject prefab;
        GameObject GO;
        bool playerSpawned = false;
        foreach (var item in LG_Blocks)
        {
            foreach (var spawn in item.LGS_Spawns)
            {
                switch (spawn.spawnType)
                {
                    case LevelGen_Spawn.spawnTypeEnum.player:
                        prefab = LG_Theme.PF_Player;
                        if (prefab != null && playerSpawned == false)
                        {
                            GO = Instantiate(prefab, spawn.transform.position, Quaternion.Euler(Vector3.zero), transform);
                            GO.GetComponent<PlayerController>().NMA.transform.rotation = spawn.transform.rotation;
                            playerSpawned = true;
                        }
                        break;
                    case LevelGen_Spawn.spawnTypeEnum.companion:
                        prefab = LG_Theme.GetCompanion(Random_Seeded);
                        if (prefab != null)
                        {
                            GO = Instantiate(prefab, spawn.transform.position, Quaternion.Euler(Vector3.zero), transform);
                            GO.GetComponent<AgentController>().NMA.transform.rotation = spawn.transform.rotation;
                        }
                        break;
                    case LevelGen_Spawn.spawnTypeEnum.enemy:
                        prefab = LG_Theme.GetEnemy(Random_Seeded);
                        if (prefab != null)
                        {
                            GO = Instantiate(prefab, spawn.transform.position, Quaternion.Euler(Vector3.zero), transform);
                            GO.GetComponent<AgentController>().NMA.transform.rotation = spawn.transform.rotation;
                        }
                        break;
                    case LevelGen_Spawn.spawnTypeEnum.treasure:
                        prefab = LG_Theme.GetTreasure(Random_Seeded);
                        if (prefab != null)
                            GO = Instantiate(prefab, spawn.transform.position, spawn.transform.rotation, transform);
                        break;
                    default:
                        break;
                }
            }
        }
    }
    
    void GenerateBuildings(List<LevelGen_Block> _rooms, LevelGen_Theme _theme, Transform lHolder, int series = 3)
    {
        if (series > 0)
        {
            List<LevelGen_Block> newRooms = new List<LevelGen_Block>();
            foreach (var item in _rooms)
            {
                foreach (var entry in item.LGD_Entries)
                {
                    if (entry.B_connected)
                        continue;
                    Transform _holder = new GameObject().transform;
                    _holder.gameObject.name = entry.entryType.ToString() + " " + entry.transform.position.ToString();
                    _holder.parent = lHolder;

                    for (int i = 0; i < i_attempts; i++)
                    {
                        LevelGen_Block _temp = GenerateRoom(_theme, lHolder, _holder, entry);
                        if (_temp == null)
                            continue;
                        foreach (var bound in _temp.B_bounds)
                            bound.B_Bounds.enabled = false;
                        Physics.SyncTransforms();
                        int _tarOverlaps = 0;
                        if (entry.entryType == LevelGen_Block.entryTypeEnum.shipPark) _tarOverlaps = 1;
                        if (CheckBounds(_temp, _tarOverlaps))
                        {
                            DestroyImmediate(_temp.gameObject);
                        }
                        else
                        {
                            entry.OnConnect();
                            newRooms.Add(_temp);
                            foreach (var bound in _temp.B_bounds)
                                bound.B_Bounds.enabled = true;
                            break;
                        }
                    }

                }
            }
            LG_Blocks.AddRange(newRooms);
            GenerateBuildings(newRooms, _theme, lHolder, series - 1);
        }
        else
        {
            UpdateNavMeshes();
        }
    }

    LevelGen_Block GenerateRoom(LevelGen_Theme _theme, Transform lholder, Transform _holder, LevelGen_Door _entry)
    {
        LevelGen_Block _corridor = Instantiate(_theme.GetBlock(LevelGen_Block.blockTypeEnum.corridor, _entry.entryType, Random_Seeded), lholder);
        List<LevelGen_Door> potEntries = new List<LevelGen_Door>();
        foreach (var item in _corridor.LGD_Entries)
        {
            if (_entry.entryType.CheckEntry(item.entryType))
                potEntries.Add(item);
        }
        if (potEntries.Count == 0)
        {
            DestroyImmediate(_corridor.gameObject);
            return null;
        }
        var entry = potEntries[Random_Seeded.NextInt(0, potEntries.Count)];

        _holder.position = entry.transform.position;
        _holder.transform.forward = -entry.transform.forward;
        _corridor.transform.parent = _holder;
        _holder.position = _entry.transform.position;
        _holder.rotation = _entry.transform.rotation;

        entry.OnConnect();

        return _corridor;
    }

    bool CheckBounds(LevelGen_Block _temp, int _tarAmount = 0)
    {
        foreach (var item in _temp.B_bounds)
        {
            if (Physics.OverlapBox(item.B_Bounds.center + item.transform.position, item.B_Bounds.size / 2, item.transform.rotation, LM_mask).Length != _tarAmount)
            {
                return true;
            }

        }
        return false;
    }
}
