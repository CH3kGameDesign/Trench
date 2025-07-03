using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;
using Unity.Mathematics;
using static Layout_Basic;
using static UnityEngine.EventSystems.EventTrigger;
using static Themes;

public class LevelGen : MonoBehaviour
{
    public uint seed = new uint();
    private Unity.Mathematics.Random Random_Seeded;

    [SerializeField] private holderTypesClass holderTypes = new holderTypesClass();

    public Themes themeHolder;
    [HideInInspector] public LevelGen_Theme LG_Theme;

    private NavMeshSurface[] nm_Surfaces;

    private List<LevelGen_Block> LG_Blocks = new List<LevelGen_Block>();

    public SpaceGenerator spaceGenerator;

    [HideInInspector] public List<AgentController> AC_agents = new List<AgentController>();

    public LayerMask LM_mask;

    private int i_series = 2;
    private int i_attempts = 10;

    [System.Serializable]
    private class holderTypesClass
    {
        public GameObject layoutHolder_Default;
        public GameObject layoutHolder_Ship;
    }

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

        LG_Theme = themeHolder.GetTheme(SaveData.themeCurrent);
        if (LG_Theme.Layouts.Count > 0)
        {
            Layout_Basic _layout = LG_Theme.GetLayout_Basic(Random_Seeded);
            StartCoroutine(GenerateLayout_Smart(LG_Theme, _layout));
            MusicHandler.Instance.SetupPlaylist(LG_Theme.playlist);
        }
        else
            GenerateLayout_Series(LG_Theme);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector3 GetRandomPoint(int _block = -1, bool _3D = false)
    {
        if (_block < 0)
            _block = UnityEngine.Random.Range(0, LG_Blocks.Count);
        return LG_Blocks[_block].GetRandomPoint(_3D);
    }

    public void GenerateLayout_Series(LevelGen_Theme _theme)
    {
        LG_Blocks = new List<LevelGen_Block>();
        Transform lHolder = Instantiate(GetHolderTypePrefab(SaveData.themeCurrent)).transform;
        lHolder.parent = transform;
        lHolder.localPosition = Vector3.zero;
        nm_Surfaces = lHolder.GetComponents<NavMeshSurface>();

        LevelGen_Block _bridge = Instantiate(_theme.GetBlock(LevelGen_Block.blockTypeEnum.bridge, LevelGen_Block.entryTypeEnum.any, Random_Seeded), lHolder);
        _bridge.transform.localPosition = Vector3.zero;
        LG_Blocks.Add(_bridge);

        GenerateBuildings_Series(LG_Blocks, _theme, lHolder, i_series);
        SetDoors();
        UpdateNavMeshes();
        SpawnObjects();
    }

    public IEnumerator GenerateLayout_Smart(LevelGen_Theme _theme, Layout_Basic _layout)
    {
        LG_Blocks = new List<LevelGen_Block>();
        Transform lHolder = Instantiate(GetHolderTypePrefab(SaveData.themeCurrent)).transform;
        
        lHolder.parent = transform;
        UpdatePosition(lHolder);

        nm_Surfaces = lHolder.GetComponents<NavMeshSurface>();

        GenerateBuildings_Smart_FirstRoom(_layout.recipe, _theme, lHolder);
        if (LG_Blocks.Count < _layout.totalRoomAmount)
        {
            yield return new WaitForEndOfFrame();
            Debug.Log("Retry Generation");
            GameObject.Destroy(lHolder.gameObject);
            StartCoroutine(GenerateLayout_Smart(_theme, _layout));
        }
        else
        {
            GenerateBuildings_Extra(_theme, _layout, lHolder);
            SetDoors();
            UpdateNavMeshes();
            SpawnObjects();
            SetupVehicle(_theme, _layout, lHolder);
        }
    }

    void UpdatePosition(Transform lHolder)
    {
        if (SaveData.themeCurrent == themeEnum.ship)
        {
            Vector3 pos;
            if (spaceGenerator.GetExitPos(out pos, SaveData.lastLandingSpot))
                lHolder.position = pos;
            else
                lHolder.localPosition = Vector3.zero;
        }
        else
            lHolder.localPosition = Vector3.zero;
    }

    void SetupVehicle(LevelGen_Theme _theme, Layout_Basic _layout, Transform lHolder)
    {
        if (SaveData.themeCurrent == themeEnum.ship)
        {
            Ship _ship = lHolder.GetComponent<Ship>();
            Vector3 offset = Vector3.zero;
            foreach (var item in LG_Blocks)
            {
                foreach (var bound in item.B_bounds)
                {
                    offset += bound.B_Bounds.center;
                    offset += bound.transform.position;
                }
            }
            offset /= LG_Blocks.Count;
            _ship.T_camHook.position = offset;
        }
    }

    void SetDoors()
    {
        foreach (var item in LG_Blocks)
        {
            foreach (var entry in item.LGD_Entries)
            {
                entry.Set();
            }
        }
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
                            PlayerController PC = GO.GetComponent<PlayerController>();
                            PC.NMA.transform.rotation = spawn.transform.rotation;
                            PC.Ref.R_recall.SetRecallPos(spawn.transform);
                            playerSpawned = true;
                        }
                        break;
                    case LevelGen_Spawn.spawnTypeEnum.companion:
                        if (spawn.PF_override != null) prefab = spawn.PF_override;
                        else prefab = LG_Theme.GetCompanion(Random_Seeded);

                        Random_Seeded.NextInt();
                        if (prefab != null)
                        {
                            GO = Instantiate(prefab, spawn.transform.position, Quaternion.Euler(Vector3.zero), transform);
                            AgentController AC = GO.GetComponent<AgentController>();
                            AC.NMA.transform.rotation = spawn.transform.rotation;
                            AC.ChangeState(spawn._state);
                            AC_agents.Add(AC);
                        }
                        break;
                    case LevelGen_Spawn.spawnTypeEnum.enemy:
                        if (spawn.PF_override != null) prefab = spawn.PF_override;
                        else prefab = LG_Theme.GetEnemy(Random_Seeded); 

                        Random_Seeded.NextInt();
                        if (prefab != null)
                        {
                            GO = Instantiate(prefab, spawn.transform.position, Quaternion.Euler(Vector3.zero), transform);
                            AgentController AC = GO.GetComponent<AgentController>();
                            AC.NMA.transform.rotation = spawn.transform.rotation;
                            AC.ChangeState(spawn._state);
                            AC_agents.Add(AC);
                        }
                        break;
                    case LevelGen_Spawn.spawnTypeEnum.treasure:
                        if (spawn.PF_override != null) prefab = spawn.PF_override;
                        else prefab = LG_Theme.GetTreasure(Random_Seeded);

                        Random_Seeded.NextInt();
                        if (prefab != null)
                            GO = Instantiate(prefab, spawn.transform.position, spawn.transform.rotation, transform);
                        break;
                    case LevelGen_Spawn.spawnTypeEnum.boss:
                        EnemyTimer.Instance.Setup(spawn.transform);
                        break;
                    default:
                        break;
                }
            }
        }
    }
    GameObject GetHolderTypePrefab(themeEnum _theme)
    {
        switch (_theme)
        { 
            case themeEnum.ship:    return holderTypes.layoutHolder_Ship;
            default:                return holderTypes.layoutHolder_Default;
        }
    }

    void GenerateBuildings_Series(List<LevelGen_Block> _rooms, LevelGen_Theme _theme, Transform lHolder, int series = 3)
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
            GenerateBuildings_Series(newRooms, _theme, lHolder, series - 1);
        }
        else
        {
            UpdateNavMeshes();
        }
    }
    void GenerateBuildings_Smart_FirstRoom(Layout_Basic.room _room, LevelGen_Theme _theme, Transform lHolder)
    {
        Transform _holder = new GameObject().transform;
        _holder.gameObject.name = "First Room " + lHolder.position.ToString();
        _holder.parent = lHolder;
        _holder.localPosition = Vector3.zero;

        LevelGen_Block _prefab = _theme.GetBlock(_room.roomType.ConvertToLevelGen(), LevelGen_Block.entryTypeEnum.any, Random_Seeded);
        LevelGen_Block _temp = Instantiate(_prefab, _holder);
        _temp.transform.localPosition = Vector3.zero;

        _temp.Setup(LG_Blocks.Count);
        LG_Blocks.Add(_temp);
        foreach (var bound in _temp.B_bounds)
            bound.B_Bounds.enabled = true;
        for (int j = 0; j < _room.connectedRooms.Count; j++)
            GenerateBuildings_Smart(_room.connectedRooms[j], LG_Theme, lHolder, _temp, _room.entryTypes[j]);
    }
    void GenerateBuildings_Smart(Layout_Basic.room _room, LevelGen_Theme _theme, Transform lHolder, LevelGen_Block _parent, Layout_Basic.entryTypeEnum _entryType = Layout_Basic.entryTypeEnum.any)
    {
        bool _completed = false;
        List<LevelGen_Door> _doors = new List<LevelGen_Door>();
        foreach (var item in _parent.LGD_Entries)
        {
            if (item.B_connected)
                continue;
            if (item.entryType.CompareEntries(_entryType.ConvertToLevelGen()))
                _doors.Add(item);
            _doors.Shuffle(Random_Seeded);
        }
        foreach (var entry in _doors)
        {
            Transform _holder = new GameObject().transform;
            _holder.gameObject.name = entry.entryType.ToString() + " " + entry.transform.position.ToString();
            _holder.parent = lHolder;

            for (int i = 0; i < i_attempts; i++)
            {
                LevelGen_Block _temp = GenerateRoom(_theme, lHolder, _holder, entry, _room.roomType.ConvertToLevelGen());
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

                    _temp.Setup(LG_Blocks.Count);
                    LG_Blocks.Add(_temp);
                    foreach (var bound in _temp.B_bounds)
                        bound.B_Bounds.enabled = true;
                    for (int j = 0; j < _room.connectedRooms.Count; j++)
                        GenerateBuildings_Smart(_room.connectedRooms[j], LG_Theme, lHolder, _temp, _room.entryTypes[j]);
                    _completed = true;
                    break;
                }
            }
            if (_completed)
                break;
        }
    }

    void GenerateBuildings_Extra(LevelGen_Theme _theme, Layout_Basic _layout, Transform lHolder)
    {
        List<LevelGen_Door> entries = new List<LevelGen_Door>();
        int roomAmt = 0;
        if (_layout.extraRoom_Amount.y > 0)
            roomAmt = Random_Seeded.NextInt(_layout.extraRoom_Amount.x, _layout.extraRoom_Amount.y);
        bool shipGenerated = false;
        LevelGen_Block.blockTypeEnum _type = LevelGen_Block.blockTypeEnum.deadend;
        foreach (var item in LG_Blocks)
        {
            foreach (var entry in item.LGD_Entries)
            {
                if (entry.B_connected)
                    continue;
                entries.Add(entry);
            }
        }
        entries.Shuffle(Random_Seeded);
        foreach (var entry in entries)
        {
            switch (entry.entryType)
            {
                case LevelGen_Block.entryTypeEnum.shipPark:
                    if (shipGenerated)
                        continue;
                    shipGenerated = true;
                    _type = LevelGen_Block.blockTypeEnum.ship;
                    break;
                default:
                    if (roomAmt <= 0)
                        continue;
                    roomAmt--;
                    break;
            }
            Transform _holder = new GameObject().transform;
            _holder.gameObject.name = entry.entryType.ToString() + " " + entry.transform.position.ToString();
            _holder.parent = lHolder;

            for (int i = 0; i < i_attempts; i++)
            {
                LevelGen_Block _temp = GenerateRoom(_theme, lHolder, _holder, entry, _type);
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
                    _temp.Setup(LG_Blocks.Count);
                    LG_Blocks.Add(_temp);
                    foreach (var bound in _temp.B_bounds)
                        bound.B_Bounds.enabled = true;
                    break;
                }
            }
        }
    }

    LevelGen_Block GenerateRoom(LevelGen_Theme _theme, Transform lholder, Transform _holder, LevelGen_Door _entry, LevelGen_Block.blockTypeEnum _blockType = LevelGen_Block.blockTypeEnum.corridor)
    {
        LevelGen_Block _prefab = _theme.GetBlock(_blockType, _entry.entryType, Random_Seeded);
        if (_prefab == null)
            return null;
        LevelGen_Block _corridor = Instantiate(_prefab, lholder);
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

    public int GetCollectedValue()
    {
        int _value = 0;
        foreach (var block in LG_Blocks)
        {
            foreach (var point in block.TP_TreasurePoints)
            {
                _value += Mathf.Max(0, point.I_value);
            }
        }
        return _value;
    }

    public void AgentDeath(AgentController _AC)
    {
        foreach (AgentController agent in AC_agents)
        {
            if (agent == _AC || agent == null)
                continue;
            agent.behaviour.OnDeath(agent, _AC);
        }
    }
}
