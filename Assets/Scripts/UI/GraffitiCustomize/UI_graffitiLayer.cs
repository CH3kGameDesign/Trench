using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_graffitiLayer : MonoBehaviour
{
    public TextMeshProUGUI TM_layerNum;
    public Image I_layerSprite;

    public Button B_button;

    [HideInInspector] public RectTransform RT_mover;
    private Image I_mover;

    private int i_num;
    [HideInInspector] public bool B_imageSet = false;

    public void Setup(int _num)
    {
        TM_layerNum.text = _num.ToString();
        i_num = _num;
    }

    public void OnClick()
    {
        GraffitiCustomize.Instance.SelectBuild(this);
    }

    public void Grab(GraffitiCustomize _GC)
    {
        Setup(_GC);
        _GC.SetCursorPos(RT_mover.anchoredPosition);
        RT_mover.parent = _GC.RT_cursor;
        RT_mover.anchoredPosition = Vector2.zero;
    }
    void Setup(GraffitiCustomize _GC)
    {
        if (I_mover != null) return;
        I_mover = Instantiate(_GC.PF_image, _GC.RT_cursor);
        RT_mover = I_mover.rectTransform;
        I_mover.enabled = false;
    }
    public void SetImage(Sprite _sprite)
    {
        I_layerSprite.sprite = _sprite; 
        I_mover.sprite = _sprite;
        B_imageSet = true;
        I_mover.enabled = true;
    }
    public void SetColor(Color _color)
    {
        if (I_mover == null)
            return;
        I_mover.color = _color;

        if (I_layerSprite == null)
            return;
        I_layerSprite.color = _color;
    }
    public Color GetColor()
    {
        if (I_mover != null)
            return I_mover.color;
        return I_layerSprite.color;
    }
    public void Place(GraffitiCustomize _GC)
    {
        RT_mover.parent = _GC.RT_holder;
    }
}
