using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_graffitiLayer : MonoBehaviour
{
    public TextMeshProUGUI TM_layerNum;
    public Image I_layerSprite;
    string s_stampID = "";

    public GameObject G_onSelectedObject;

    public Button B_button;


    [HideInInspector] public bool B_selected = false;

    [HideInInspector] public RectTransform RT_mover;
    private Image I_mover;

    private int i_num;
    [HideInInspector] public bool B_imageSet = false;

    private void Start()
    {
        UpdateSelected();
    }


    private void Update()
    {

    }
    public void FollowCursor(GraffitiCustomize _GC)
    {
        if (_GC.RT_cursor == null)
            return;
        RT_mover.position = _GC.RT_cursor.position;
        RT_mover.localEulerAngles = _GC.RT_cursor.localEulerAngles;
    }
    public void Setup(int _num)
    {
        TM_layerNum.text = _num.ToString();
        i_num = _num;
    }

    public void OnClick()
    {
        GraffitiCustomize.Instance.SelectBuild(this);
        UpdateSelected(false);
    }

    public void Grab(GraffitiCustomize _GC)
    {
        Setup(_GC);

        _GC.SetCursorPos(RT_mover.anchoredPosition, RT_mover.localEulerAngles.z);
        RT_mover.anchoredPosition = Vector2.zero;
        RT_mover.localEulerAngles = Vector3.zero;
    }
    void Setup(GraffitiCustomize _GC)
    {
        if (I_mover != null) return;
        I_mover = Instantiate(_GC.PF_image, _GC.RT_stampParent);
        RT_mover = I_mover.rectTransform;
        I_mover.enabled = false;
    }
    public void Setup(GraffitiManager _GM)
    {
        if (I_mover != null) return;
        I_mover = Instantiate(_GM.PF_image, _GM.RT_layers);
        RT_mover = I_mover.rectTransform;
        I_mover.enabled = false;
    }
    public void SetImage(Stamp_Scriptable _stamp)
    {
        I_layerSprite.sprite = _stamp._sprite; 
        I_mover.sprite = _stamp._sprite;
        s_stampID = _stamp._stampID;
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

    }

    public void OnSelect(BaseEventData _event) { UpdateSelected(true); }
    public void OnDeselect(BaseEventData _event) { UpdateSelected(false); }
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

    //Deprecated
    public string GetExport()
    {
        string _temp = "";
        if (I_mover == null || !B_imageSet)
            return "{ },";
        _temp += "\n{\n";
        _temp += s_stampID + ",\n";
        _temp += RT_mover.anchoredPosition + ",\n";
        _temp += RT_mover.localScale + ",\n";
        _temp += RT_mover.localEulerAngles.z + ",\n";
        _temp += I_mover.color + "\n";
        _temp += "},";
        return _temp;
    }

    public GraffitiManager.layerClass GetLayerInfo()
    {
        GraffitiManager.layerClass _temp = new GraffitiManager.layerClass();
        if (!B_imageSet)
            return _temp;
        _temp._stampID = s_stampID;
        _temp._position = Vector2Int.RoundToInt(RT_mover.anchoredPosition);
        _temp._scale = new Vector2(RT_mover.localScale.x, RT_mover.localScale.y);
        _temp._rotation = RT_mover.localEulerAngles.z;
        _temp._color = I_mover.color;
        return _temp;
    }
    public void SetLayerInfo(GraffitiManager.layerClass _temp)
    {
        Stamp_Scriptable _stamp = GraffitiManager.Instance.GetStamp(_temp._stampID);
        if (_stamp == null)
            return;
        SetImage(_stamp);
        RT_mover.anchoredPosition = _temp._position;
        RT_mover.localScale = new Vector3(_temp._scale.x, _temp._scale.y, 1);
        RT_mover.localEulerAngles = new Vector3(0, 0, _temp._rotation);
        SetColor(_temp._color);
    }
}
