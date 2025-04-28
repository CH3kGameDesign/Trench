using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New LevelGen Theme", menuName = "Trench/LevelGen/Theme")]
public class LevelGen_Theme : ScriptableObject
{
    public List<LevelGen_Block> Corridors = new List<LevelGen_Block>();
    public List<LevelGen_Block> Hangars = new List<LevelGen_Block>();
    public List<LevelGen_Block> Bridges = new List<LevelGen_Block>();

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
            default:
                break;
        }
        return null;
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
}
