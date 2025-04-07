using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;

public class LevelGen : MonoBehaviour
{
    public LevelGen_Theme LG_Theme;
    private NavMeshSurface[] nm_Surfaces;

    private List<LevelGen_Block> LG_Blocks = new List<LevelGen_Block>();
    // Start is called before the first frame update
    void Start()
    {
        Setup();
    }

    void Setup()
    {
        nm_Surfaces = GetComponents<NavMeshSurface>();
        GenerateLayout(LG_Theme);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerateLayout(LevelGen_Theme _theme)
    {
        LG_Blocks = new List<LevelGen_Block>();
        Transform lHolder = new GameObject("LayoutHolder").transform;
        lHolder.parent = transform;
        lHolder.localPosition = Vector3.zero;

        LevelGen_Block _bridge = Instantiate(_theme.GetBlock(LevelGen_Block.blockTypeEnum.bridge, LevelGen_Block.entryTypeEnum.any),lHolder);
        _bridge.transform.localPosition = Vector3.zero;
        UpdateBounds(_bridge);
        LG_Blocks.Add(_bridge);
        List<LevelGen_Block> _firstGroup = GenerateRooms(_bridge, _theme, lHolder);
        List<LevelGen_Block> _secondGroup = GenerateRoomSeries(_firstGroup, _theme, lHolder);
        List<LevelGen_Block> _thirdGroup = GenerateRoomSeries(_secondGroup, _theme, lHolder);

        UpdateNavMeshes();
    }

    public void UpdateNavMeshes()
    {
        foreach (var item in nm_Surfaces)
        {
            item.BuildNavMesh();
        }
    }
    List<LevelGen_Block> GenerateRoomSeries(List<LevelGen_Block> _rooms, LevelGen_Theme _theme, Transform lHolder)
    {
        List<LevelGen_Block> _new = new List<LevelGen_Block>();
        foreach (var item in _rooms)
        {
            _new.AddRange(GenerateRooms(item, _theme, lHolder));
        }
        return _new;
    }
    List<LevelGen_Block> GenerateRooms(LevelGen_Block _room, LevelGen_Theme _theme, Transform lHolder)
    {
        List<LevelGen_Block> _blocks = new List<LevelGen_Block>();
        foreach (var item in _room.List_Entries)
        {
            if (item.connected)
                continue;
            Transform _holder = new GameObject().transform;
            _holder.gameObject.name = item.type.ToString() + " " + item.transform.position.ToString();
            _holder.parent = lHolder;

            LevelGen_Block _temp = GenerateRoom(_room, _theme, lHolder, _holder, item);
            if (_temp != null)
            {
                _blocks.Add(_temp);
                LG_Blocks.Add(_temp);
            }
            else
            {
                Destroy(_holder.gameObject);
            }
        }
        return _blocks;
    }

    LevelGen_Block GenerateRoom(LevelGen_Block _room, LevelGen_Theme _theme, Transform lholder, Transform _holder, LevelGen_Block.entryClass _entry)
    {
        for (int i = 0; i < 10; i++)
        {
            LevelGen_Block _corridor = Instantiate(_theme.GetBlock(LevelGen_Block.blockTypeEnum.corridor, _entry.type), lholder);
            List<LevelGen_Block.entryClass> potEntries = new List<LevelGen_Block.entryClass>();
            foreach (var item in _corridor.List_Entries)
            {
                if (item.type == _entry.type)
                    potEntries.Add(item);
            }
            if (potEntries.Count == 0)
            {
                DestroyImmediate(_corridor.gameObject);
                continue;
            }
            var entry = potEntries[Random.Range(0, potEntries.Count)];
            
            _holder.position = entry.transform.position;
            _holder.transform.forward = -entry.transform.forward;
            _corridor.transform.parent = _holder;
            _holder.position = _entry.transform.position;
            _holder.rotation = _entry.transform.rotation;

            UpdateBounds(_corridor);
            bool _overlap = CheckBounds(_corridor);
            if (!_overlap)
            {
                entry.connected = true;
                _entry.connected = true;
                return _corridor;
            }
            else
                DestroyImmediate(_corridor.gameObject);
        }
        return null;
    }

    void UpdateBounds(LevelGen_Block _corridor)
    {
        for (int i = 0; i < _corridor.B_bounds.Count; i++)
        {
            Bounds _temp = _corridor.B_bounds[i];
            _temp.center += _corridor.transform.position;
            _corridor.B_bounds[i] = _temp;
        }
    }

    bool CheckBounds(LevelGen_Block _corridor)
    {
        foreach (var _bounds1 in _corridor.B_bounds)
        {
            foreach (var _blocks in LG_Blocks)
            {
                foreach (var _bounds2 in _blocks.B_bounds)
                {
                    if (_bounds1.Intersects(_bounds2))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
}
