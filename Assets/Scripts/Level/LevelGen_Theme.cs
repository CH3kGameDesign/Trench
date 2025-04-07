using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New LevelGen Theme", menuName = "Trench/LevelGen/Theme")]
public class LevelGen_Theme : ScriptableObject
{
    public List<LevelGen_Block> Corridors = new List<LevelGen_Block>();
    public List<LevelGen_Block> Hangars = new List<LevelGen_Block>();
    public List<LevelGen_Block> Bridges = new List<LevelGen_Block>();

    public List<GameObject> PF_Decor = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public LevelGen_Block GetBlock(LevelGen_Block.blockTypeEnum _type, LevelGen_Block.entryTypeEnum _entryType)
    {
        switch (_type)
        {
            case LevelGen_Block.blockTypeEnum.corridor: return GetBlockFromList(Corridors, _entryType);
            case LevelGen_Block.blockTypeEnum.bridge:   return GetBlockFromList(Bridges, _entryType);
            case LevelGen_Block.blockTypeEnum.hangar:   return GetBlockFromList(Hangars, _entryType);
            default:
                break;
        }
        return null;
    }
    private LevelGen_Block GetBlockFromList(List<LevelGen_Block> _list, LevelGen_Block.entryTypeEnum _entryType)
    {
        int _temp;
        if (_entryType == LevelGen_Block.entryTypeEnum.any)
        {
            _temp = Random.Range(0, _list.Count);
            return _list[_temp];
        }
        List<LevelGen_Block> _listFiltered = new List<LevelGen_Block>();
        foreach (var item in _list)
        {
            foreach (var _entry in item.List_Entries)
            {
                if (_entry.type == _entryType)
                {
                    _listFiltered.Add(item);
                    break;
                }
            }
        }
        _temp = Random.Range(0, _listFiltered.Count);
        return _listFiltered[_temp];
    }
}
