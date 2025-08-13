using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental.FileFormat;
using UnityEngine;

[CreateAssetMenu(fileName ="New LevelGen Theme", menuName = "Trench/LevelGen/Theme")]
public class LevelGen_Theme : ScriptableObject
{
    [Header("Playlist")]
    public Playlist playlist;
    [Header("Layouts")]
    public List<Layout_Basic> Layouts = new List<Layout_Basic>();
    [Space(10)]
    [Header("Rooms")]
    public List<LevelGen_Block> Corridors = new List<LevelGen_Block>();
    public List<LevelGen_Block> Hangars = new List<LevelGen_Block>();
    public List<LevelGen_Block> Bridges = new List<LevelGen_Block>();
    public List<LevelGen_Block> DeadEnds = new List<LevelGen_Block>();
    public List<LevelGen_Block> Engines = new List<LevelGen_Block>();
    public List<LevelGen_Block> FoodHalls = new List<LevelGen_Block>();
    public List<LevelGen_Block> CrewQuarters = new List<LevelGen_Block>();
    public List<LevelGen_Block> CaptainQuaters = new List<LevelGen_Block>();
    public List<LevelGen_Block> Vaults = new List<LevelGen_Block>();
    public List<LevelGen_Block> Ships = new List<LevelGen_Block>();
    [Space(10)]
    [Header("Spawns")]
    public List<GameObject> PF_Treasure = new List<GameObject>();
    public List<GameObject> PF_Enemies = new List<GameObject>();
    public List<GameObject> PF_Companions = new List<GameObject>();
    public GameObject PF_Player;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public LevelGen_Block GetBlock(LevelGen_Block.blockTypeEnum _type, LevelGen_Block.entryTypeEnum _entryType, Unity.Mathematics.Random _random)
    {
        switch (_type)
        {
            case LevelGen_Block.blockTypeEnum.corridor: return GetBlockFromList(Corridors, _entryType, _random);
            case LevelGen_Block.blockTypeEnum.bridge:   return GetBlockFromList(Bridges, _entryType, _random);
            case LevelGen_Block.blockTypeEnum.hangar:   return GetBlockFromList(Hangars, _entryType, _random);
            case LevelGen_Block.blockTypeEnum.deadend: return GetBlockFromList(DeadEnds, _entryType, _random);
            case LevelGen_Block.blockTypeEnum.engine: return GetBlockFromList(Engines, _entryType, _random);
            case LevelGen_Block.blockTypeEnum.foodhall: return GetBlockFromList(FoodHalls, _entryType, _random);
            case LevelGen_Block.blockTypeEnum.crewQuarters: return GetBlockFromList(CrewQuarters, _entryType, _random);
            case LevelGen_Block.blockTypeEnum.captain: return GetBlockFromList(CaptainQuaters, _entryType, _random);
            case LevelGen_Block.blockTypeEnum.vault: return GetBlockFromList(Vaults, _entryType, _random);
            case LevelGen_Block.blockTypeEnum.ship: return GetBlockFromList(Ships, _entryType, _random);
            default:
                break;
        }
        return null;
    }
    public void AddBlock(LevelGen_Block LGB)
    {
        switch (LGB.BlockType)
        {
            case LevelGen_Block.blockTypeEnum.corridor: AddBlock(LGB, Corridors); break;
            case LevelGen_Block.blockTypeEnum.bridge: AddBlock(LGB, Bridges); break;
            case LevelGen_Block.blockTypeEnum.hangar: AddBlock(LGB, Hangars); break;
            case LevelGen_Block.blockTypeEnum.deadend: AddBlock(LGB, DeadEnds); break;
            case LevelGen_Block.blockTypeEnum.engine: AddBlock(LGB, Engines); break;
            case LevelGen_Block.blockTypeEnum.foodhall: AddBlock(LGB, FoodHalls); break;
            case LevelGen_Block.blockTypeEnum.crewQuarters: AddBlock(LGB, CrewQuarters); break;
            case LevelGen_Block.blockTypeEnum.captain: AddBlock(LGB, CaptainQuaters); break;
            case LevelGen_Block.blockTypeEnum.vault: AddBlock(LGB, Vaults); break;
            case LevelGen_Block.blockTypeEnum.ship: AddBlock(LGB, Ships); break;
            default:
                break;
        }
    }

    void AddBlock(LevelGen_Block LGB, List<LevelGen_Block> LGBs)
    {
        if (LGBs.Contains(LGB))
            return;
        LGBs.Add(LGB);
    }
    private LevelGen_Block GetBlockFromList(List<LevelGen_Block> _list, LevelGen_Block.entryTypeEnum _entryType, Unity.Mathematics.Random _random)
    {
        int _temp;
        if (_entryType == LevelGen_Block.entryTypeEnum.any)
        {
            _temp = _random.NextInt(0, _list.Count);
            return _list[_temp];
        }
        List<LevelGen_Block> _listFiltered = new List<LevelGen_Block>();
        foreach (var item in _list)
        {
            foreach (var _entry in item.LGD_Entries)
            {
                if (_entryType.CheckEntry(_entry.entryType))
                {
                    _listFiltered.Add(item);
                    break;
                }
            }
        }
        if (_listFiltered.Count == 0)
            return null;
        _temp = _random.NextInt(0, _listFiltered.Count);
        return _listFiltered[_temp];
    }


    public GameObject GetEnemy(Unity.Mathematics.Random _random)
    {
        if (PF_Enemies.Count == 0)
            return null;
        int ran = _random.NextInt(0, PF_Enemies.Count);
        return PF_Enemies[ran];
    }

    public GameObject GetCompanion(Unity.Mathematics.Random _random)
    {
        if (PF_Companions.Count == 0)
            return null;
        int ran = _random.NextInt(0, PF_Companions.Count);
        return PF_Companions[ran];
    }

    public GameObject GetTreasure(Unity.Mathematics.Random _random)
    {
        if (PF_Treasure.Count == 0)
            return null;
        int ran = _random.NextInt(0, PF_Treasure.Count);
        return PF_Treasure[ran];
    }

    public Layout_Basic GetLayout_Basic(Unity.Mathematics.Random _random)
    {
        if (Layouts.Count == 0)
            return null;
        int ran = _random.NextInt(0, Layouts.Count);
        return Layouts[ran];
    }
}
