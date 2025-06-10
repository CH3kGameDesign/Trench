using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using TMPro;

public class ButtonCost : MonoBehaviour
{
    private Action _event;
    public Image I_BG;
    public Image I_Sprite;
    public TextMeshProUGUI TM_name;
    public TextMeshProUGUI TM_amt;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public bool Setup(Resource.resourceClass _resource)
    {
        _event = _resource.OnClick;
        Resource.resourceType _type = _resource.GetType();
        I_Sprite.sprite = _type.image;

        TM_name.text = _type._name;

        int _amt = 0;
        Resource.resourceClass _resourcePlayer = SaveData.GetResource(_resource._type);
        if (_resourcePlayer != null)
            _amt = _resourcePlayer.amt;
        TM_amt.text = _amt.ToString() + "/" + _resource.amt.ToString();
        if (_amt < _resource.amt)
        {
            I_BG.color = Color.red;
            return false;
        }
        else
        {
            I_BG.color = Color.white;
            return true;
        }
    }
    public void OnPress()
    {
        _event.Invoke();
    }
}
