using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using TMPro;

public class ButtonAdvanced : MonoBehaviour
{
    private Action _onSumbit = null;
    private Action _onSelect = null;
    private Action _onDeselect = null;
    public Image I_BG;
    public RawImage I_RawImage;
    public TextMeshProUGUI TM_text;
    public GameObject G_onSelectedObject;
    [HideInInspector] public bool B_selected = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Setup(Action onPress, Action onSelect = null, Action onDeselect = null, Texture _image = null, Color? _bg = null, string _text = null)
    {
        _onSumbit = onPress;
        _onSelect = onSelect;
        _onDeselect = onDeselect;
        
        if (_bg != null && I_BG != null)
            I_BG.color = _bg.Value;
        if (_image != null && I_RawImage != null)
            I_RawImage.texture = _image;
        if (_text != null && TM_text != null)
            TM_text.text = _text;
    }
    public void OnPress()
    {
        if(_onSumbit != null) _onSumbit.Invoke();
    }
    public void OnSelect(BaseEventData _event) { UpdateSelected(true); if(_onSelect != null) _onSelect.Invoke();}
    public void OnDeselect(BaseEventData _event) { UpdateSelected(false); if(_onDeselect != null) _onDeselect.Invoke();}
    void UpdateSelected(bool _sel)
    {
        B_selected = _sel;
        UpdateSelected();
    }
    void UpdateSelected()
    {
        if (G_onSelectedObject)
            G_onSelectedObject.SetActive(B_selected);
    }
}
