using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using TMPro;

public class ButtonGeneric : MonoBehaviour
{
    private Action _event;
    public Image I_BG;
    public Image I_Sprite;
    public TextMeshProUGUI TM_text;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Setup(Action onPress, Sprite _sprite = null, Color? _bg = null, string _text = null)
    {
        _event = onPress;
        
        if (_bg != null && I_BG != null)
            I_BG.color = _bg.Value;
        if (_sprite != null && I_Sprite != null)
            I_Sprite.sprite = _sprite;
        if (_text != null && TM_text != null)
            TM_text.text = _text;
    }
    public void OnPress()
    {
        _event.Invoke();
    }
}
