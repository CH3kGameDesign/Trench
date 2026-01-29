using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.UI;

public class GraffitiCustomize : MonoBehaviour
{
    public static GraffitiCustomize Instance;
    protected bool b_active;

    public int I_spacing = 1;

    [Header("Menus")]
    public GameObject G_leftColumn;
    public GameObject G_buildMenu;
    public GameObject G_stampMenu;
    public Button B_defaultSelected;
    public Button B_defaultStamp;

    [Header("References")]
    public RectTransform RT_layerGrid;
    public RectTransform RT_stampGrid;
    public RectTransform RT_holder;
    [Space(10)]
    public UI_graffitiLayer PF_UI_graffitiLayer;
    public GameObject PF_graffitiLayer;
    public LayoutModuleGroup PF_moduleGroup;
    public Image PF_image;
    [Space(10)]
    public Image I_color;
    public Slider S_colorHue;
    public Slider S_colorSat;
    public Slider S_colorVig;
    public Image I_colorSatBG;
    public Image I_colorVigBG;
    private Vector3 V3_colorValues;
    private Color C_color;
    [Space(10)]
    public UI_ValueSlider VS_scaleX;
    public UI_ValueSlider VS_scaleY;
    public UI_ValueSlider VS_rotation;
    public UI_ValueSlider VS_positionX;
    public UI_ValueSlider VS_positionY;

    [Header("Cursor")]
    public RectTransform RT_cursor;
    private Vector2 v2_cursorPosition;
    private Vector2 v2_canvasSize;
    private Vector2 v2_canvasHalf;
    private Vector2 v2_areaHalf;

    private GraffitiManager.graffitiClass curGraffiti = new GraffitiManager.graffitiClass();
    private List<UI_graffitiLayer> layers = new List<UI_graffitiLayer>();

    private UI_graffitiLayer curLayer;

    public float F_canvasPosSpeed = 50f;
    private Vector2 v2_canvasPos;
    private Vector2 v2_canvasPos_Min;
    private Vector2 v2_canvasPos_Max;

    public float F_canvasZoomSpeed_Mouse = 3;
    public float F_canvasZoomSpeed_Gamepad = 1;
    private float F_canvasZoom = 1;
    private Vector2 v2_zoomBounds = new Vector2(0.5f, 2f);
    public float f_cursorSpeed = 50f;
    public float f_cursorSpeed_Rotate = 60f;
    public float f_cursorSpeed_Scale = 1f;
    public Vector2 v2_scaleLimits = new Vector2(0.05f, 5);
    private bool b_maintainScale = true;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LoadSave();
        CanvasSize_Update();
        Layers_Setup();
        StampMenu_Setup();
        Color_Setup();
    }


    public void Layers_Setup()
    {
        RT_layerGrid.DeleteChildren();
        layers.Clear();

        for (int i = 18; i > 0; i--)
        {
            UI_graffitiLayer G = Instantiate(PF_UI_graffitiLayer, RT_layerGrid);
            G.Setup(i);
            layers.Add(G);
        }
    }
    void StampMenu_Setup()
    {
        RT_stampGrid.DeleteChildren();
        int _size = 300;
        
        _size += StampMenu_Setup("Primitives", GraffitiManager.stampTypeEnum.primitives);
        _size += StampMenu_Setup("Complex", GraffitiManager.stampTypeEnum.complex);
        _size += StampMenu_Setup("Letters", GraffitiManager.stampTypeEnum.letters);

        B_defaultStamp = RT_stampGrid.GetComponentInChildren<Button>();

        RT_stampGrid.sizeDelta = new Vector2(RT_stampGrid.sizeDelta.x, _size);
    }
    int StampMenu_Setup(string _name, GraffitiManager.stampTypeEnum _type)
    {
        List<Stamp_Scriptable> _list = GraffitiManager.Instance.GetList(_type);
        if (_list.Count == 0) return 0;
        LayoutModuleGroup LMG = Instantiate(PF_moduleGroup, RT_stampGrid);
        LMG.Setup(_name, _list);
        return
            50 +
            (Mathf.CeilToInt((float)_list.Count / 3) * 120);
    }

    void LoadSave()
    {

    }

    void Save()
    {

    }

    void LoadObjects()
    {

    }
    public void OnUpdate(PlayerController _PC)
    {
        if (!b_active) return;
        if (!G_buildMenu.activeSelf)
        {
            MoveCursor(_PC);
            RotateCursor(_PC);
            ScaleCursor(_PC);
            ClickManager(_PC);
        }
    }
    void MoveCursor(PlayerController _PC)
    {
        if (_PC.Inputs.b_isGamepad)
            MoveCursor_Gamepad(_PC.Inputs.v2_inputDir, f_cursorSpeed);
        else
            MoveCursor_Gamepad(_PC.Inputs.v2_inputDir, f_cursorSpeed);

        MoveCanvas(_PC);
        ZoomCanvas(_PC);
    }
    void RotateCursor(PlayerController _PC)
    {
        float _rotation = RT_cursor.localEulerAngles.z;
        _rotation -= _PC.Inputs.f_rotate * Time.unscaledDeltaTime * f_cursorSpeed_Rotate;
        _rotation = _rotation % 360;
        RT_cursor.localEulerAngles = new Vector3(0, 0, _rotation);

        VS_rotation.SetValue(_rotation, 0, 360);
    }
    void ScaleCursor(PlayerController _PC)
    {
        if (curLayer == null)
            return;
        Vector2 _scale = curLayer.RT_mover.localScale;
        if (b_maintainScale)
        {
            float _dif = _scale.x / _scale.y;

            _scale.x += _PC.Inputs.v2_scale.y * Time.unscaledDeltaTime * f_cursorSpeed_Scale;
            _scale.x = Mathf.Clamp(_scale.x, v2_scaleLimits.x, v2_scaleLimits.y);

            _scale.y = _scale.x / _dif;
            _scale.y = Mathf.Clamp(_scale.y, v2_scaleLimits.x, v2_scaleLimits.y);

            _scale.x = _scale.y * _dif;
        }
        else
            _scale += _PC.Inputs.v2_scale * Time.unscaledDeltaTime * f_cursorSpeed_Scale;

        SetScale(_scale);
    }

    void SetScale(Vector2 _scale)
    {
        _scale.x = Mathf.Clamp(_scale.x, v2_scaleLimits.x, v2_scaleLimits.y);
        _scale.y = Mathf.Clamp(_scale.y, v2_scaleLimits.x, v2_scaleLimits.y);

        VS_scaleX.SetValue(_scale.x, v2_scaleLimits);
        VS_scaleY.SetValue(_scale.y, v2_scaleLimits);

        curLayer.RT_mover.localScale = new Vector3(_scale.x, _scale.y, 1);
    }
    void ClickManager(PlayerController _PC)
    {
        if (!_PC.Inputs.b_confirm) return;
        if (LMO_ActiveClick(_PC))
            return;
        if (LMO_InactiveClick(_PC))
            return;
    }
    bool LMO_ActiveClick(PlayerController _PC)
    {

        return false;
    }
    bool LMO_InactiveClick(PlayerController _PC)
    {

        return false;
    }
    public void BuildMenu()
    {
        if (!G_buildMenu.activeSelf && !G_stampMenu.activeSelf) ShowBuild(PlayerManager.main);
        else if(!PlayerManager.main.Inputs.b_isGamepad)
            HideBuild(PlayerManager.main);
        PlaceImage();
    }

    public void SelectBuild(UI_graffitiLayer _graffiti)
    {
        PlaceImage();
        curLayer = _graffiti;
        curLayer.Grab(this);

        if (curLayer.B_imageSet)
        {
            HideBuild(PlayerManager.main);
            Color_Select(curLayer.GetColor());
        }
        else
        {
            G_stampMenu.SetActive(true);
            G_buildMenu.SetActive(false);
            if (PlayerManager.main.Inputs.b_isGamepad)
                EventSystem.current.SetSelectedGameObject(GetStampMenu_DefaultButton());
        }
    }
    public void SelectStamp(Stamp_Scriptable _graffiti)
    {
        HideBuild(PlayerManager.main);
        if (curLayer == null) return;
        curLayer.SetImage(_graffiti._sprite);
        curLayer.SetColor(C_color);
    }

    public void PlaceImage()
    {
        if (curLayer == null) return;
        curLayer.Place(this);
    }

    int i_buildMenuSlide = 400;
    void ShowBuild(PlayerController _PC)
    {
        StartCoroutine(RT_holder.Move(new Vector2(v2_canvasPos.x + i_buildMenuSlide, v2_canvasPos.y), Quaternion.identity, true, 0.1f));
        G_leftColumn.SetActive(false);
        G_buildMenu.SetActive(true);
        G_stampMenu.SetActive(false);
        if (_PC.Inputs.b_isGamepad)
            EventSystem.current.SetSelectedGameObject(GetBuildMenu_DefaultButton());
    }
    GameObject GetBuildMenu_DefaultButton()
    {
        GameObject _button = null;
        if (layers.Count > 0)
            _button = layers[layers.Count - 1].B_button.gameObject;
        B_defaultSelected = layers[layers.Count - 1].B_button;
        return _button;
    }
    GameObject GetStampMenu_DefaultButton()
    {
        GameObject _button = null;
        _button = B_defaultStamp.gameObject;
        B_defaultSelected = B_defaultStamp;
        return _button;
    }
    void HideBuild(PlayerController _PC)
    {
        StartCoroutine(RT_holder.Move(v2_canvasPos, Quaternion.identity, true, 0.1f));
        G_leftColumn.SetActive(true);
        G_buildMenu.SetActive(false);
        G_stampMenu.SetActive(false);
        if (_PC.Inputs.b_isGamepad)
            EventSystem.current.SetSelectedGameObject(null);
    }
    void MoveCursor_Mouse()
    {
        v2_cursorPosition = Input.mousePosition;

        v2_cursorPosition /= new Vector2(Screen.width, Screen.height);
        v2_cursorPosition *= v2_canvasSize;
        v2_cursorPosition -= v2_canvasHalf;

        v2_cursorPosition.x = Mathf.Clamp(v2_cursorPosition.x, -v2_canvasHalf.x, v2_canvasHalf.x);
        v2_cursorPosition.y = Mathf.Clamp(v2_cursorPosition.y, -v2_canvasHalf.y, v2_canvasHalf.y);

        RT_cursor.anchoredPosition = v2_cursorPosition;

        VS_positionX.SetValue(v2_cursorPosition.x, -v2_canvasHalf.x, v2_canvasHalf.x);
        VS_positionY.SetValue(v2_cursorPosition.y, -v2_canvasHalf.y, v2_canvasHalf.y);
    }
    void MoveCursor_Gamepad(Vector2 dir, float _speed)
    {
        v2_cursorPosition += dir * _speed * Time.unscaledDeltaTime;

        v2_cursorPosition.x = Mathf.Clamp(v2_cursorPosition.x, -v2_canvasHalf.x, v2_canvasHalf.x);
        v2_cursorPosition.y = Mathf.Clamp(v2_cursorPosition.y, -v2_canvasHalf.y, v2_canvasHalf.y);

        RT_cursor.anchoredPosition = v2_cursorPosition;

        VS_positionX.SetValue(v2_cursorPosition.x, -v2_canvasHalf.x, v2_canvasHalf.x);
        VS_positionY.SetValue(v2_cursorPosition.y, -v2_canvasHalf.y, v2_canvasHalf.y);
    }
    public void SetCursorPos(Vector3 pos, float? _rotation = null, Vector2? _scale = null)
    {
        v2_cursorPosition = pos;
        RT_cursor.anchoredPosition = v2_cursorPosition;
        if (_rotation != null)
            RT_cursor.localEulerAngles = new Vector3(0, 0, _rotation.Value);
        if (_scale != null)
        {
            Vector3 _newScale = _scale.Value;
            RT_cursor.localEulerAngles = new Vector3(_newScale.x, _newScale.y, 1);
        }

        VS_positionX.SetValue(v2_cursorPosition.x, -v2_canvasHalf.x, v2_canvasHalf.x);
        VS_positionY.SetValue(v2_cursorPosition.y, -v2_canvasHalf.y, v2_canvasHalf.y);
    }

    void MoveCanvas(PlayerController _PC)
    {
        Vector2 _temp = v2_canvasPos;
        if (v2_cursorPosition.x < -v2_canvasHalf.x / 2)
            _temp.x += F_canvasPosSpeed * Time.unscaledDeltaTime;
        if (v2_cursorPosition.x > v2_canvasHalf.x / 2)
            _temp.x -= F_canvasPosSpeed * Time.unscaledDeltaTime;
        _temp.x = Mathf.Clamp(_temp.x, (v2_canvasPos_Min.x * F_canvasZoom) + v2_canvasHalf.x, (v2_canvasPos_Max.x * F_canvasZoom) + v2_canvasHalf.x);

        if (v2_cursorPosition.y < -v2_canvasHalf.y / 2)
            _temp.y += F_canvasPosSpeed * Time.unscaledDeltaTime;
        if (v2_cursorPosition.y > v2_canvasHalf.y / 2)
            _temp.y -= F_canvasPosSpeed * Time.unscaledDeltaTime;
        _temp.y = Mathf.Clamp(_temp.y, v2_canvasPos_Min.y * F_canvasZoom, v2_canvasPos_Max.y * F_canvasZoom);

        if (_PC.Inputs.b_isGamepad)
            MoveCursor_Gamepad(_temp - v2_canvasPos, 1);

        v2_canvasPos = _temp;
        RT_holder.localPosition = v2_canvasPos;
    }

    public void ZoomCanvas(PlayerController _PC)
    {
        float _dir = _PC.Inputs.f_zoom;
        if (_PC.Inputs.b_isGamepad)
            F_canvasZoom += _dir * Time.unscaledDeltaTime * F_canvasZoomSpeed_Gamepad;
        else
            F_canvasZoom += _dir * Time.unscaledDeltaTime * F_canvasZoomSpeed_Mouse;
        F_canvasZoom = Mathf.Clamp(F_canvasZoom, v2_zoomBounds.x, v2_zoomBounds.y);

        RT_holder.localScale = Vector3.one * F_canvasZoom;
        RT_cursor.localScale = Vector3.one * F_canvasZoom;
    }
    public void Display()
    {
        b_active = true;
        HideBuild(PlayerManager.main);
    }

    void CanvasSize_Update()
    {
        float _w = 1920;
        float _h = _w / Screen.width;
        _h *= Screen.height;
        v2_canvasSize = new Vector2(_w, _h);
        v2_canvasHalf = v2_canvasSize / 2;
        v2_canvasPos = new Vector2(v2_canvasHalf.x, 0);
    }

    public void Hide()
    {
        Save();
        b_active = false;
        HideBuild(PlayerManager.main);
    }
    public void MaintainScale_SetBool(bool value)
    {
        b_maintainScale = value;
    }

    void Color_Setup()
    {
        V3_colorValues = new Vector3(0.5f, 0f, 1f);
        S_colorHue.value = V3_colorValues.x;
        S_colorSat.value = V3_colorValues.y;
        S_colorVig.value = V3_colorValues.z;
        Color_Update();
    }
    void Color_Select(Color _color)
    {
        float _h;
        float _s;
        float _v;
        Color.RGBToHSV(_color, out _h, out _s, out _v);
        V3_colorValues = new Vector3(_h, _s, _v);
        S_colorHue.value = _h;
        S_colorSat.value = _s;
        S_colorVig.value = _v;
        Color_Update();
    }
    public void PickColor_Hue(float _hue)
    {
        V3_colorValues.x = _hue;
        Color_Update();
    }
    public void PickColor_Saturation(float _sat)
    {
        V3_colorValues.y = _sat;
        Color_Update();
    }
    public void PickColor_Vignette(float _vig)
    {
        V3_colorValues.z = _vig;
        Color_Update();
    }
    public void PickScale_X(float _var)
    {
        _var = (_var + 1f) / 2f;
        if (curLayer == null)
            return;
        Vector2 _scale = curLayer.RT_mover.localScale;
        if (b_maintainScale)
        {
            float _dif = _scale.x / _scale.y;

            _scale.x = Mathf.Lerp(v2_scaleLimits.x, v2_scaleLimits.y, _var);
            _scale.x = Mathf.Clamp(_scale.x, v2_scaleLimits.x, v2_scaleLimits.y);

            _scale.y = _scale.x / _dif;
            _scale.y = Mathf.Clamp(_scale.y, v2_scaleLimits.x, v2_scaleLimits.y);

            _scale.x = _scale.y * _dif;
        }
        else
            _scale.x = Mathf.Lerp(v2_scaleLimits.x, v2_scaleLimits.y, _var);

        SetScale(_scale);
    }
    public void PickScale_Y(float _var)
    {
        _var = (_var + 1f) / 2f;
        if (curLayer == null)
            return;
        Vector2 _scale = curLayer.RT_mover.localScale;
        if (b_maintainScale)
        {
            float _dif = _scale.x / _scale.y;

            _scale.y = Mathf.Lerp(v2_scaleLimits.x, v2_scaleLimits.y, _var);
            _scale.y = Mathf.Clamp(_scale.x, v2_scaleLimits.x, v2_scaleLimits.y);

            _scale.x = _scale.y / _dif;
            _scale.x = Mathf.Clamp(_scale.y, v2_scaleLimits.x, v2_scaleLimits.y);

            _scale.y = _scale.x * _dif;
        }
        else
            _scale.y = Mathf.Lerp(v2_scaleLimits.x, v2_scaleLimits.y, _var);

        SetScale(_scale);
    }
    public void PickRotation(float _var)
    {
        _var = (_var + 1f) / 2f;

        float _rotation = Mathf.Lerp(-180, 180, _var);
        RT_cursor.localEulerAngles = new Vector3(0, 0, _rotation);
    }
    public void PickPosition_X(float _var)
    {
        _var = (_var + 1f) / 2f;

        v2_cursorPosition.x = Mathf.Lerp(-v2_canvasHalf.x, v2_canvasHalf.x, _var);
        RT_cursor.anchoredPosition = v2_cursorPosition;
    }
    public void PickPosition_Y(float _var)
    {
        _var = (_var + 1f) / 2f;

        v2_cursorPosition.y = Mathf.Lerp(-v2_canvasHalf.y, v2_canvasHalf.y, _var);
        RT_cursor.anchoredPosition = v2_cursorPosition;
    }
    void Color_Update()
    {
        C_color = Color.HSVToRGB(V3_colorValues.x, V3_colorValues.y, V3_colorValues.z);
        I_color.color = C_color;

        Color hue = Color.HSVToRGB(V3_colorValues.x, 1, 1);
        I_colorSatBG.color = hue;
        hue = Color.HSVToRGB(V3_colorValues.x, V3_colorValues.y, 1);
        I_colorVigBG.color = hue;

        if (curLayer == null) return;
        curLayer.SetColor(C_color);
    }
    // Update is called once per frame
    void Update()
    {

    }
}
