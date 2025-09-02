using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using static LayoutCustomize;

public class LayoutModuleObject : MonoBehaviour
{
    [HideInInspector] public LevelGen_Block Block;
    public TextMeshProUGUI TM_name;
    public RectTransform RT;
    public RectTransform RT_nameBG;
    public RectTransform RT_lockBG;
    public RawImage I_BG;

    public LayoutModuleDoor PF_door;
    public List<LayoutModuleDoor> LMO_doorList = new List<LayoutModuleDoor>();
    [HideInInspector] public Vector2Int minPos;
    [HideInInspector] public Vector2Int maxPos;
    [HideInInspector] public int I_rot = 0;
    [HideInInspector] public bool B_locked = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Setup(LevelGen_Block _block, int _rot = 0, bool _locked = false)
    {
        Block = _block;
        TM_name.text = _block._name;
        RT.sizeDelta = _block.size * 100;

        B_locked = _locked;
        RT_lockBG.gameObject.SetActive(_locked);

        if (_block.layoutTexture)
        {
            I_BG.texture = _block.layoutTexture;
            I_BG.rectTransform.sizeDelta = Vector3.one * Mathf.Max(_block.size.x, _block.size.y) * 100;
        }
        else
            I_BG.rectTransform.sizeDelta = (_block.size * 100) - (Vector2.one * 10);

        SetupDoors();
        Rotate(_rot, true);
    }

    void SetupDoors()
    {
        foreach (var item in Block.doors)
        {
            LayoutModuleDoor _LMD = Instantiate(PF_door, RT);
            _LMD.Setup(item, this);
            LMO_doorList.Add(_LMD);
        }
    }
    public bool GetDoorFromWorldPos(Vector2Int _pos, int _rot, out LayoutModuleDoor _LMD)
    {
        _LMD = null;
        Vector2Int _pos2;
        int _rot2;
        foreach(var item in LMO_doorList)
        {
            item.GetWorldPos(out _pos2, out _rot2);
            if (_pos == _pos2 &&  _rot == _rot2)
            {
                _LMD = item;
                return true;
            }
        }
        return false;
    }
    public void PickedUp()
    {
        foreach (var item in LMO_doorList)
            item.SetConnected(null);
        SetBounds(-Vector2Int.one, -Vector2Int.one);
    }
    public Vector2Int GetCenter()
    {
        Vector2Int _center = minPos;
        if (I_rot % 2 == 0)
        {
            _center.x += Mathf.FloorToInt((float)Block.size.y / 2);
            _center.y += Mathf.FloorToInt((float)Block.size.x / 2);
        }
        else
        {
            _center.x += Mathf.FloorToInt((float)Block.size.x / 2);
            _center.y += Mathf.FloorToInt((float)Block.size.y / 2);
        }
        return _center;
    }
    public Vector2Int GetMinPos(Vector2Int _center)
    {
        if (I_rot % 2 == 0)
        {
            _center.x -= Mathf.FloorToInt((float)Block.size.y / 2);
            _center.y -= Mathf.FloorToInt((float)Block.size.x / 2);
        }
        else
        {
            _center.x -= Mathf.FloorToInt((float)Block.size.x / 2);
            _center.y -= Mathf.FloorToInt((float)Block.size.y / 2);
        }
        return _center;
    }
    public Vector2Int GetMaxPos(Vector2Int _center)
    {
        if (I_rot % 2 == 0)
        {
            _center.x += Mathf.CeilToInt((float)Block.size.y / 2) - 1;
            _center.y += Mathf.CeilToInt((float)Block.size.x / 2) - 1;
        }
        else
        {
            _center.x += Mathf.CeilToInt((float)Block.size.x / 2) - 1;
            _center.y += Mathf.CeilToInt((float)Block.size.y / 2) - 1;
        }
        return _center;
    }
    public Vector2Int GetSize()
    {
        if (I_rot % 2 == 0)
            return Block.size;
        else
            return new Vector2Int(Block.size.y, Block.size.x);
    }
    public void SetBounds(Vector2Int _min, Vector2Int _max)
    {
        minPos = _min;
        maxPos = _max;
    }
    public void Rotate(int _dir, bool _instant = false)
    {
        int _rot = (I_rot + _dir) % 4;
        if (_rot < 0) _rot += 4;
        I_rot = _rot;

        if (_instant)
            RT.localEulerAngles = new Vector3(0, 0, 90 * I_rot);
        else
            StartCoroutine(RT.Move(Quaternion.Euler(new Vector3(0, 0, 90 * I_rot)), true, 0.2f));
        RT_nameBG.localEulerAngles = new Vector3(0, 0, -90 * I_rot);
        RT_lockBG.localEulerAngles = new Vector3(0, 0, -90 * I_rot);
        Vector2 _anchor = new Vector2(0.5f, 1f);
        Vector2 _anchorLock = new Vector2(1f, 0f);
        switch (I_rot)
        {
            case 0:
                _anchor = new Vector2(0.5f, 1f);
                _anchorLock = new Vector2(1f, 0f);
                RT_nameBG.sizeDelta = new Vector2(RT.sizeDelta.x - 20, 30);
                break;
            case 1:
                _anchor = new Vector2(1f, 0.5f);
                _anchorLock = new Vector2(0f, 0f);
                RT_nameBG.sizeDelta = new Vector2(RT.sizeDelta.y - 20, 30);
                break;
            case 2:
                _anchor = new Vector2(0.5f, 0f);
                _anchorLock = new Vector2(0f, 1f);
                RT_nameBG.sizeDelta = new Vector2(RT.sizeDelta.x - 20, 30);
                break;
            case 3:
                _anchor = new Vector2(0f, 0.5f);
                _anchorLock = new Vector2(1f, 1f);
                RT_nameBG.sizeDelta = new Vector2(RT.sizeDelta.y - 20, 30);
                break;
            default:
                break;
        }
        RT_nameBG.anchorMin = _anchor;
        RT_nameBG.anchorMax = _anchor;
        RT_nameBG.anchoredPosition = Vector2.zero;

        RT_lockBG.anchorMin = _anchorLock;
        RT_lockBG.anchorMax = _anchorLock;
        RT_lockBG.anchoredPosition = Vector2.zero;
    }
}
