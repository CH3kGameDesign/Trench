using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;

public class ButtonGeneric : MonoBehaviour
{
    private Action _event;
    public Image I_BG;
    public Image I_Sprite;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Setup(Action onPress, Sprite _sprite, Color _bg)
    {
        _event = onPress;
        I_BG.color = _bg;
        I_Sprite.sprite = _sprite;
    }
    public void OnPress()
    {
        _event.Invoke();
    }
}
