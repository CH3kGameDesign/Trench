using PurrNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.AI.Navigation;
using Unity.Mathematics;
using UnityEngine;
using static Themes;


public class LevelGen : MonoBehaviour
{
    private Unity.Mathematics.Random Random_Seeded;

    [SerializeField] private holderTypesClass holderTypes = new holderTypesClass();

    public Themes themeHolder;
    [HideInInspector] public LevelGen_Theme LG_Theme;

    [HideInInspector] public Transform T_Holder;
    [HideInInspector] public Ship ship = null;

    private NavMeshSurface[] nm_Surfaces;

    [HideInInspector] public List<LevelGen_Block> LG_Blocks = new List<LevelGen_Block>();

    [HideInInspector] public List<AgentController> AC_agents = new List<AgentController>();

    public LayerMask LM_mask;

    private int i_series = 2;
    private int i_attempts = 10;
    [Space(10)]
    [HideInInspector] public bool isHost = false;
    [HideInInspector] public int id = -1;

    [System.Serializable]
    private class holderTypesClass
    {
        public GameObject layoutHolder_Default;
        public GameObject layoutHolder_Ship;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    public enum spawnType { _default, friendlyOnly, enemyOnly};
    public void Setup(uint _seed, int _id, Vector3 _pos, bool _player = true, BoxCollider _BC = null)
    {
        id = _id;

        Random_Seeded = new Unity.Mathematics.Random(_seed);

        LG_Theme = themeHolder.GetTheme(SaveData.themeCurrent);
        MusicHandler.Instance.SetupPlaylist(LG_Theme.playlist);
        if (SaveData.shipLayout._objects.Count > 0 && SaveData.themeCurrent == themeEnum.ship)
        {
            GenerateLayout_Specific(SaveData.shipLayout, _pos,
                _player ? spawnType.friendlyOnly : spawnType.enemyOnly, _BC);
        }
        else if (LG_Theme.Layouts.Count > 0)
        {
            Layout_Basic _layout = LG_Theme.GetLayout_Basic(Random_Seeded);
            StartCoroutine(GenerateLayout_Smart(LG_Theme, _layout, _pos));
        }
        else
            GenerateLayout_Series(LG_Theme, _pos);
    }
    public void Setup(uint _seed, int _id, Vector3 _pos, Layout_Defined _layoutDefined, bool _player = true, BoxCollider _BC = null)
    {
        id = _id;

        Random_Seeded = new Unity.Mathematics.Random(_seed);

        LG_Theme = themeHolder.GetTheme(SaveData.themeCurrent);
        MusicHandler.Instance.SetupPlaylist(LG_Theme.playlist);
        if (_layoutDefined._objects.Count > 0 && SaveData.themeCurrent == themeEnum.ship)
        {
            GenerateLayout_Specific(_layoutDefined, _pos,
                _player ? spawnType.friendlyOnly : spawnType.enemyOnly, _BC);
        }
        else if (LG_Theme.Layouts.Count > 0)
        {
            Layout_Basic _layout = LG_Theme.GetLayout_Basic(Random_Seeded);
            StartCoroutine(GenerateLayout_Smart(LG_Theme, _layout, _pos));
        }
        else
            GenerateLayout_Series(LG_Theme, _pos);
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

    int _gridSize = 5;
    public void GenerateLayout_Specific(Layout_Defined _save, Vector3 _pos, spawnType _type = spawnType._default, BoxCollider _BC = null)
    {
        LG_Blocks = new List<LevelGen_Block>();
        T_Holder = Instantiate(GetHolderTypePrefab(SaveData.themeCurrent)).transform;

        T_Holder.parent = transform;
        UpdatePosition(T_Holder, _pos);
        Physics.SyncTransforms();

        nm_Surfaces = T_Holder.GetComponents<NavMeshSurface>();

        foreach (var item in _save._objects)
        {
            Transform _holder = new GameObject().transform;
            _holder.gameObject.name = "Room: " + item._block._name;
            _holder.parent = T_Holder;

            LevelGen_Block _prefab =  item._block;
            LevelGen_Block _temp = Instantiate(_prefab, _holder);

            _temp.transform.localEulerAngles = new Vector3(0, -item._rot * 90, 0);
            switch (item._rot)
            {
                case 1:
                    _temp.transform.localPosition = Vector3.zero;
                    break;
                case 2:
                    _temp.transform.localPosition = new Vector3(0, 0, _temp.size.y) * _gridSize;
                    break;
                case 3:
                    _temp.transform.localPosition = new Vector3(-_temp.size.y, 0, _temp.size.x) * _gridSize;
                    break;
                default:
                    _temp.transform.localPosition = new Vector3(-_temp.size.x, 0, 0) * _gridSize;
                    break;
            }

            _temp.Setup(id, LG_Blocks.Count);
            LG_Blocks.Add(_temp);
            foreach (var bound in _temp.B_bounds)
                bound.B_Bounds.enabled = true;

            _holder.localPosition = new Vector3(
                -item._pos.y + 1 + ((float)_save._bounds.data[0].d.Count / 2f), 
                0, 
                item._pos.x - 1 - ((float)_save._bounds.data.Count / 2f)) 
                * _gridSize;
            _holder.localRotation = Quaternion.identity;
        }

        SetDoors_Auto();
        UpdateNavMeshes();
        SetupVehicle(T_Holder, _save, _type, _BC);
        SpawnObjects(_type);
        LevelGen_Holder.Instance.IsReady();
    }
    void SetDoors_Auto()
    {
        List<LevelGen_Door> _doors = new List<LevelGen_Door>();
        foreach (var item in LG_Blocks)
        {
            foreach(var door in item.LGD_Entries)
            {
                bool _continue = false;
                for (int i = _doors.Count - 1; i >= 0; i--)
                {
                    if (_doors[i].entryType != door.entryType) continue;
                    if (Vector3.Distance(_doors[i].transform.position, door.transform.position) > 0.2f) continue;

                    door.B_connected = true;
                    _doors[i].B_connected = true;

                    _doors.RemoveAt(i);
                    _continue = true;
                    break;
                }
                if (_continue) continue;
                _doors.Add(door);
            }
        }
        SetDoors();
    }

    public void GenerateLayout_Series(LevelGen_Theme _theme, Vector3 _pos)
    {
        LG_Blocks = new List<LevelGen_Block>();
        T_Holder = Instantiate(GetHolderTypePrefab(SaveData.themeCurrent)).transform;

        T_Holder.parent = transform;
        UpdatePosition(T_Holder, _pos);
        nm_Surfaces = T_Holder.GetComponents<NavMeshSurface>();

        LevelGen_Block _bridge = Instantiate(_theme.GetBlock(LevelGen_Block.blockTypeEnum.bridge, LevelGen_Block.entryTypeEnum.any, Random_Seeded), T_Holder);
        _bridge.transform.localPosition = Vector3.zero;
        LG_Blocks.Add(_bridge);

        GenerateBuildings_Series(LG_Blocks, _theme, T_Holder, i_series);
        SetDoors();
        UpdateNavMeshes();
        SpawnObjects();
    }

    public IEnumerator GenerateLayout_Smart(LevelGen_Theme _theme, Layout_Basic _layout, Vector3 _pos)
    {
        LG_Blocks = new List<LevelGen_Block>();
        T_Holder = Instantiate(GetHolderTypePrefab(SaveData.themeCurrent)).transform;

        T_Holder.parent = transform;
        UpdatePosition(T_Holder, _pos);

        nm_Surfaces = T_Holder.GetComponents<NavMeshSurface>();

        yield return GenerateBuildings_Smart_FirstRoom(_layout.recipe, _theme, T_Holder);
        if (LG_Blocks.Count < _layout.totalRoomAmount)
        {
            Debug.Log("Retry Generation");
            GameObject.Destroy(T_Holder.gameObject);
            StartCoroutine(GenerateLayout_Smart(_theme, _layout, _pos));
        }
        else
        {
            yield return StartCoroutine(GenerateBuildings_Extra(_theme, _layout, T_Holder));
            SetDoors();
            yield return new WaitForEndOfFrame();
            UpdateNavMeshes();
            yield return new WaitForEndOfFrame();
            SetupVehicle(T_Holder);
            yield return new WaitForEndOfFrame();
            SpawnObjects();
            yield return new WaitForEndOfFrame();
            LevelGen_Holder.Instance.IsReady();
        }
    }
    void UpdatePosition(Transform lHolder, Vector3 _pos)
    {
        if (!NetworkManager.isServerStatic)
            return;
        lHolder.position = _pos;
        LevelGen_Holder.Instance.UpdateTransform(this);
    }

    void SetupVehicle(Transform lHolder, Layout_Defined _save, spawnType _type, BoxCollider _BC = null)
    {
        if (_save == null) SetupVehicle(T_Holder, _type);
        if (SaveData.themeCurrent == themeEnum.ship)
        {
            ship = lHolder.GetComponent<Ship>();
            ship.LG = this;
            ship.AIVar.wanderBounds = _BC;
            Vector3 offset = Vector3.zero;
            ship.T_camHook.localPosition = new Vector3(
                0,
                2.5f,
                0);
            ship.V3_camOffset = new Vector3(
                0, 
                10, 
                -_save._bounds.data.Count * 5);

            if (_type == spawnType.enemyOnly)
            {
                AgentController AC = SpawnDriver(ship.T_pilotSeat);
                ship.OnEnter(AC);
            }
        }
    }
    void SetupVehicle(Transform lHolder, spawnType _type = spawnType._default, BoxCollider _BC = null)
    {
        if (SaveData.themeCurrent == themeEnum.ship)
        {
            ship = lHolder.GetComponent<Ship>();
            ship.LG = this;
            ship.AIVar.wanderBounds = _BC;
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
            ship.T_camHook.position = offset;

            if (_type == spawnType.enemyOnly)
            {
                AgentController AC = SpawnDriver(ship.T_pilotSeat);
                ship.OnEnter(AC);
            }
        }
    }

    AgentController SpawnDriver (Transform _spawn)
    {
        if (!isHost)
            return null;

        GameObject prefab;
        GameObject GO;
        
        prefab = LG_Theme.GetEnemy(Random_Seeded);

        Random_Seeded.NextInt();
        if (prefab != null)
        {
            GO = Instantiate(prefab);
            AgentController AC = GO.GetComponent<AgentController>();
            AC.NMA.transform.position = _spawn.position;
            AC.NMA.transform.rotation = _spawn.rotation;
            AC.ChangeState(BaseController.stateEnum.vehicle);
            AC_agents.Add(AC);
            return AC;
        }
        return null;
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

    private void SpawnObjects(spawnType _type = spawnType._default)
    {
        GameObject prefab;
        GameObject GO;
        List<Transform> spawnPoints = new List<Transform>();
        foreach (var item in LG_Blocks)
        {
            foreach (var spawn in item.LGS_Spawns)
            {
                switch (spawn.spawnType)
                {
                    case LevelGen_Spawn.spawnTypeEnum.player:
                        if (_type == spawnType.enemyOnly)
                            break;
                        spawnPoints.Add(spawn.transform);
                        break;
                    case LevelGen_Spawn.spawnTypeEnum.companion:
                        if (!isHost)
                            break;
                        if (_type == spawnType.enemyOnly)
                            break;
                        if (spawn.PF_override != null) prefab = spawn.PF_override;
                        else prefab = LG_Theme.GetCompanion(Random_Seeded);

                        Random_Seeded.NextInt();
                        if (prefab != null)
                        {
                            GO = Instantiate(prefab);
                            AgentController AC = GO.GetComponent<AgentController>();
                            AC.NMA.transform.position = spawn.transform.position;
                            AC.NMA.transform.rotation = spawn.transform.rotation;
                            AC.ChangeState(spawn._state);
                            AC_agents.Add(AC);
                        }
                        break;
                    case LevelGen_Spawn.spawnTypeEnum.enemy:
                        if (!isHost)
                            break;
                        if (_type == spawnType.friendlyOnly)
                            break;
                        if (spawn.PF_override != null) prefab = spawn.PF_override;
                        else prefab = LG_Theme.GetEnemy(Random_Seeded); 

                        Random_Seeded.NextInt();
                        if (prefab != null)
                        {
                            GO = Instantiate(prefab);
                            AgentController AC = GO.GetComponent<AgentController>();
                            AC.NMA.transform.position = spawn.transform.position;
                            AC.NMA.transform.rotation = spawn.transform.rotation;
                            AC.ChangeState(spawn._state);
                            AC_agents.Add(AC);
                        }
                        break;
                    case LevelGen_Spawn.spawnTypeEnum.treasure:
                        if (!isHost)
                            break;
                        if (_type == spawnType.friendlyOnly)
                            break;
                        if (spawn.PF_override != null) prefab = spawn.PF_override;
                        else prefab = LG_Theme.GetTreasure(Random_Seeded);

                        Random_Seeded.NextInt();
                        if (prefab != null)
                            GO = Instantiate(prefab, spawn.transform.position, spawn.transform.rotation, transform);
                        break;
                    case LevelGen_Spawn.spawnTypeEnum.boss:
                        if (_type == spawnType.friendlyOnly)
                            break;
                        EnemyTimer.Instance.Setup(spawn.transform);
                        break;
                    default:
                        break;
                }
            }
        }
        LevelGen_Holder.Instance.playerSpawner.SetSpawns(spawnPoints);
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
    IEnumerator GenerateBuildings_Smart_FirstRoom(Layout_Basic.room _room, LevelGen_Theme _theme, Transform lHolder)
    {
        Transform _holder = new GameObject().transform;
        _holder.gameObject.name = "First Room " + lHolder.position.ToString();
        _holder.parent = lHolder;
        _holder.localPosition = Vector3.zero;

        LevelGen_Block _prefab = _theme.GetBlock(_room.roomType.ConvertToLevelGen(), LevelGen_Block.entryTypeEnum.any, Random_Seeded);
        LevelGen_Block _temp = Instantiate(_prefab, _holder);
        _temp.transform.localPosition = Vector3.zero;

        _temp.Setup(id, LG_Blocks.Count);
        LG_Blocks.Add(_temp);
        foreach (var bound in _temp.B_bounds)
            bound.B_Bounds.enabled = true;
        yield return new WaitForEndOfFrame();
        for (int j = 0; j < _room.connectedRooms.Count; j++)
        {
            yield return StartCoroutine(GenerateBuildings_Smart(_room.connectedRooms[j], LG_Theme, lHolder, _temp, _room.entryTypes[j]));
        }
    }
    IEnumerator GenerateBuildings_Smart(Layout_Basic.room _room, LevelGen_Theme _theme, Transform lHolder, LevelGen_Block _parent, Layout_Basic.entryTypeEnum _entryType = Layout_Basic.entryTypeEnum.any)
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

                    _temp.Setup(id, LG_Blocks.Count);
                    LG_Blocks.Add(_temp);
                    foreach (var bound in _temp.B_bounds)
                        bound.B_Bounds.enabled = true;
                    for (int j = 0; j < _room.connectedRooms.Count; j++)
                        yield return StartCoroutine(GenerateBuildings_Smart(_room.connectedRooms[j], LG_Theme, lHolder, _temp, _room.entryTypes[j]));
                    _completed = true;
                    break;
                }
            }
            yield return new WaitForEndOfFrame();
            if (_completed)
                break;
        }
    }

    IEnumerator GenerateBuildings_Extra(LevelGen_Theme _theme, Layout_Basic _layout, Transform lHolder)
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
                    _temp.Setup(id, LG_Blocks.Count);
                    LG_Blocks.Add(_temp);
                    foreach (var bound in _temp.B_bounds)
                        bound.B_Bounds.enabled = true;
                    break;
                }
            }
            yield return new WaitForEndOfFrame();
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
