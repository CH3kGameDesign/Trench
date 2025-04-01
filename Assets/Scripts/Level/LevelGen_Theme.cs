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

    public LevelGen_Block GetBlock(LevelGen_Block.blockTypeEnum _type)
    {
        switch (_type)
        {
            case LevelGen_Block.blockTypeEnum.corridor: return GetBlockFromList(Corridors);
            case LevelGen_Block.blockTypeEnum.bridge:   return GetBlockFromList(Bridges);
            case LevelGen_Block.blockTypeEnum.hangar:   return GetBlockFromList(Hangars);
            default:
                break;
        }
        return null;
    }
    private LevelGen_Block GetBlockFromList(List<LevelGen_Block> _list)
    {
        int _temp = Random.Range(0, _list.Count);
        return _list[_temp];
    }
}
