using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LayoutModuleGroup : MonoBehaviour
{
    public TextMeshProUGUI TM_name;
    public RectTransform RT_holder;

    public LayoutModuleButton PF_ModuleButton;
    int _moduleButtonSize = 120;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup(string _name, List<LevelGen_Block> _blocks)
    {
        TM_name.text = _name;
        RT_holder.DeleteChildren();

        int _rows = Mathf.CeilToInt((float)_blocks.Count / 3f);
        RT_holder.sizeDelta = new (RT_holder.sizeDelta.x, _rows * _moduleButtonSize);
        GetComponent<RectTransform>().sizeDelta = new(GetComponent<RectTransform>().sizeDelta.x, 50 + (_rows * _moduleButtonSize));
        foreach (var item in _blocks)
        {
            LayoutModuleButton LMB = Instantiate(PF_ModuleButton, RT_holder);
            LMB.Setup(item);
        }
    }
    public void Setup(string _name, List<Stamp_Scriptable> _stamps)
    {
        TM_name.text = _name;
        RT_holder.DeleteChildren();

        int _rows = Mathf.CeilToInt((float)_stamps.Count / 3f);
        RT_holder.sizeDelta = new(RT_holder.sizeDelta.x, _rows * _moduleButtonSize);
        GetComponent<RectTransform>().sizeDelta = new(GetComponent<RectTransform>().sizeDelta.x, 50 + (_rows * _moduleButtonSize));
        foreach (var item in _stamps)
        {
            LayoutModuleButton LMB = Instantiate(PF_ModuleButton, RT_holder);
            LMB.Setup(item);
        }
    }
}
