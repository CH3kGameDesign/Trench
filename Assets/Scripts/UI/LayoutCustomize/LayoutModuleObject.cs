using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static LayoutCustomize;

public class LayoutModuleObject : MonoBehaviour
{
    [HideInInspector] public LevelGen_Block Block;
    public TextMeshProUGUI TM_name;
    public RectTransform RT;
    public Image I_BG;
    public Vector2Int minPos;
    public Vector2Int maxPos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Setup(LevelGen_Block _block)
    {
        TM_name.text = _block._name;
        RT.sizeDelta = _block.size * 100;
        Block = _block;
    }

    public Vector2Int GetMinPos(Vector2Int _center)
    {
        _center.x -= Mathf.FloorToInt((float)Block.size.y / 2);
        _center.y -= Mathf.FloorToInt((float)Block.size.x / 2);
        return _center;
    }
    public Vector2Int GetMaxPos(Vector2Int _center)
    {
        _center.x += Mathf.CeilToInt((float)Block.size.y / 2) - 1;
        _center.y += Mathf.CeilToInt((float)Block.size.x / 2) - 1;
        return _center;
    }
    public void SetBounds(Vector2Int _min, Vector2Int _max)
    {
        minPos = _min;
        maxPos = _max;
    }
}
