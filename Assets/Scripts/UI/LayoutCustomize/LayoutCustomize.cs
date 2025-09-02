using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LayoutCustomize : MonoBehaviour
{
    public static LayoutCustomize Instance;
    protected bool b_active;

    public Layout_Defined DEBUG_save;
    private Layout_Defined save;
    public RectTransform RT_holder;
    public RectTransform PF_square;
    public int I_spacing = 50;

    public LayoutModuleObject PF_moduleObject;
    private LayoutModuleObject LMO_active;
    private List<LayoutModuleObject> LMO_list = new List<LayoutModuleObject>();

    private Vector2Int v2_selSquare;
    private List<List<cellClass>> i_grid = new List<List<cellClass>>();
    public class cellClass
    {
        public Image _image;
        public RectTransform _rt;
        public LayoutModuleObject _moduleObject;


        public void Deselect()
        {
            if (!_image) return;
            _image.color = new Color32(0xc8, 0xc8, 0xc8, 0xFF);
            ColorModule(Color.white);
        }
        public void Select()
        {
            if (!_image) return;
            _image.color = Color.blue;
            ColorModule(Color.blue);
        }
        void ColorModule(Color32 _color)
        {
            if (!_moduleObject) return;

            _moduleObject.I_BG.color = _color;
        }
    }
    [Header("Menus")]
    public GameObject G_leftColumn;
    public GameObject G_buildMenu;
    public Button B_defaultSelected;

    [Header("References")]
    public RectTransform RT_buildGrid;
    public LayoutModuleGroup PF_moduleGroup;

    [Header("Cursor")]
    public RectTransform RT_cursor;
    private Vector2 v2_cursorPosition;
    private Vector2 v2_canvasSize;
    private Vector2 v2_canvasHalf;
    private Vector2 v2_areaHalf;


    public float F_canvasPosSpeed = 50f;
    private Vector2 v2_canvasPos;
    private Vector2 v2_canvasPos_Min;
    private Vector2 v2_canvasPos_Max;

    public float F_canvasZoomSpeed_Mouse = 3;
    public float F_canvasZoomSpeed_Gamepad = 1;
    private float F_canvasZoom = 1;
    private Vector2 v2_zoomBounds = new Vector2(0.5f, 2f);
    public float f_cursorSpeed = 50f;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LoadSave();
    }

    void LoadSave()
    {
        if (SaveData.shipLayout._theme == null)
            SaveData.shipLayout = new Layout_Defined(DEBUG_save);
        save = new Layout_Defined(SaveData.shipLayout);

        CanvasSize_Update();
        BuildMenu_Update(save._theme);
        GenerateSquares(save._bounds);
        LoadObjects();
    }

    void Save()
    {
        save._objects.Clear();
        foreach (var item in LMO_list)
        {
            Layout_Defined.objectClass _obj = new Layout_Defined.objectClass(
                item.Block, 
                item.GetCenter(), 
                item.I_rot,
                item.B_locked
                );
            save._objects.Add(_obj);
        }
        SaveData.shipLayout = new Layout_Defined(save);
    }

    void LoadObjects()
    {
        foreach (var item in save._objects)
        {
            LayoutModuleObject MO = Instantiate(PF_moduleObject, RT_holder);
            MO.Setup(item._block, item._rot, item._locked);
            MoveLMO(item._pos, MO, true);

            LayoutModuleObject _objOverlap;
            if (!SetDownModule(MO, item._pos, out _objOverlap))
            {
                Destroy(MO.gameObject);
                continue;
            }
            LMO_list.Add(MO);
        }
    }
    public void OnUpdate(PlayerController _PC)
    {
        if (!b_active) return;
        if (!G_buildMenu.activeSelf)
        {
            MoveCursor(_PC);
            ClickManager(_PC);
        }
    }
    void MoveCursor(PlayerController _PC)
    {
        if (_PC.Inputs.b_isGamepad)
            MoveCursor_Gamepad(_PC.Inputs.v2_inputDir, f_cursorSpeed);
        else
            MoveCursor_Mouse();

        MoveCanvas(_PC);
        ZoomCanvas(_PC);
        UpdateSelected();
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
        if (LMO_active == null)
            return false;

        LayoutModuleObject _objOverlap;
        if (!SetDownModule(LMO_active, v2_selSquare, out _objOverlap))
        {
            if (_objOverlap == null)
                return false;
            if (_objOverlap.B_locked)
                return false;
            PickUpModule(_objOverlap);
            LMO_list.Remove(_objOverlap);
            LayoutModuleObject _temp;
            SetDownModule(LMO_active, v2_selSquare, out _temp, true);
        }
        LMO_list.Add(LMO_active);
        LMO_active = _objOverlap;
        return true;
    }
    bool LMO_InactiveClick(PlayerController _PC)
    {
        if (LMO_active != null)
            return false;
        LMO_active = GetModuleObject(v2_selSquare);
        if (LMO_active == null)
            return false;
        if (LMO_active.B_locked)
        {
            LMO_active = null;
            return false;
        }
        PickUpModule(LMO_active);
        LMO_list.Remove(LMO_active);

        return true;
    }
    LayoutModuleObject GetModuleObject(Vector2Int pos)
    {
        if (!CheckPos(pos)) return null;
        return i_grid[pos.x][pos.y]._moduleObject;
    }
    public void Rotate(int _dir)
    {
        if (LMO_active == null) return;
        LMO_active.Rotate(_dir);
        MoveActiveLMO(v2_selSquare);
    }
    public void BuildMenu()
    {
        if (!G_buildMenu.activeSelf) ShowBuild(PlayerManager.main);
        else HideBuild(PlayerManager.main);
    }

    void BuildMenu_Update(LevelGen_Theme theme)
    {
        RT_buildGrid.DeleteChildren();
        int _size = 300;
        _size += BuildMenu_Update("Bridges", theme.Bridges);
        _size += BuildMenu_Update("Hangars", theme.Hangars);
        _size += BuildMenu_Update("Engines", theme.Engines);
        _size += BuildMenu_Update("Food Halls", theme.FoodHalls);
        _size += BuildMenu_Update("Crew Quarters", theme.CrewQuarters);
        _size += BuildMenu_Update("Captain Quarters", theme.CaptainQuaters);
        _size += BuildMenu_Update("Vaults", theme.Vaults);
        _size += BuildMenu_Update("Corridors", theme.Corridors);
        _size += BuildMenu_Update("Dead Ends", theme.DeadEnds);
        RT_buildGrid.sizeDelta = new Vector2(RT_buildGrid.sizeDelta.x, _size);
    }
    int BuildMenu_Update(string _name, List<LevelGen_Block> _blocks)
    {
        if (_blocks.Count == 0) return 0;
        LayoutModuleGroup LMG = Instantiate(PF_moduleGroup, RT_buildGrid);
        LMG.Setup(_name, _blocks);
        return
            50 + 
            (Mathf.CeilToInt((float)_blocks.Count / 3) * 120);
    }

    public void SelectBuild(LevelGen_Block _block)
    {
        LMO_active = Instantiate(PF_moduleObject, RT_holder);
        LMO_active.Setup(_block);
        HideBuild(PlayerManager.main);
    }

    int i_buildMenuSlide = 400;
    void ShowBuild(PlayerController _PC)
    {
        DestroyActiveLMO();
        StartCoroutine(RT_holder.Move(new Vector2(v2_canvasPos.x + i_buildMenuSlide, v2_canvasPos.y), Quaternion.identity, true, 0.1f));
        G_leftColumn.SetActive(false);
        G_buildMenu.SetActive(true);
        if (_PC.Inputs.b_isGamepad)
            EventSystem.current.SetSelectedGameObject(B_defaultSelected.gameObject);
    }
    void HideBuild(PlayerController _PC)
    {
        StartCoroutine(RT_holder.Move(v2_canvasPos, Quaternion.identity, true, 0.1f));
        G_leftColumn.SetActive(true);
        G_buildMenu.SetActive(false);
        if (_PC.Inputs.b_isGamepad)
            EventSystem.current.SetSelectedGameObject(null);
    }
    void DestroyActiveLMO()
    {
        if (LMO_active == null) return;
        Destroy(LMO_active.gameObject);
        LMO_active = null;
    }
    void MoveActiveLMO(Vector2Int v2)
    {
        if (LMO_active == null) return;

        if (CheckPos(LMO_active, v2_selSquare)) LMO_active.I_BG.color = Color.blue;
        else LMO_active.I_BG.color = Color.red;

        MoveLMO(v2, LMO_active);
    }
    void MoveLMO(Vector2Int sel, LayoutModuleObject LMO, bool _instant = false)
    {
        Vector2 v2 = sel;
        v2.x = Mathf.Clamp(v2.x, 0, i_grid.Count);
        v2.y = Mathf.Clamp(v2.y, 0, i_grid[0].Count);
        Vector2 size = LMO.GetSize();
        v2.x -= ((size.y - 1) % 2) / 2f;
        v2.y -= ((size.x - 1) % 2) / 2f;

        v2 *= I_spacing;
        v2 -= v2_areaHalf;
        v2 = new Vector2(v2.y, -v2.x);

        if (_instant)
            LMO.RT.localPosition = v2;
        else
            StartCoroutine(LMO.RT.Move(v2, true, 0.1f));
    }
    bool CheckPos(Vector2Int v2)
    {
        if (v2.x < 0 || v2.y < 0)
            return false;
        if (v2.x >= i_grid.Count || v2.y >= i_grid[0].Count)
            return false;

        return true;
    }
    bool CheckPos(LayoutModuleObject _obj, Vector2Int _center)
    {
        Vector2Int _min = _obj.GetMinPos(_center);
        Vector2Int _max = _obj.GetMaxPos(_center);

        LayoutModuleObject _objOverlap;
        return CheckPos(_min, _max, out _objOverlap);
    }
    bool CheckPos(Vector2Int _min, Vector2Int _max, out LayoutModuleObject _objOverlap)
    {
        _objOverlap = null;
        if (_min.x < 0 || _min.y < 0)
            return false;
        if (_max.x >= i_grid.Count || _max.y >= i_grid[0].Count)
            return false;

        for (int x = _min.x; x <= _max.x; x++)
        {
            for (int y = _min.y; y <= _max.y; y++)
            {
                if (i_grid[x][y]._rt == null)
                    return false;
                if (i_grid[x][y]._moduleObject != null)
                {
                    if (_objOverlap != null)
                    {
                        if (_objOverlap != i_grid[x][y]._moduleObject)
                        {
                            _objOverlap = null;
                            return false;
                        }
                    }
                    else
                        _objOverlap = i_grid[x][y]._moduleObject;
                }
            }
        }
        return _objOverlap == null;
    }
    bool SetDownModule(LayoutModuleObject _obj, Vector2Int _center, out LayoutModuleObject _objOverlap, bool _force = false)
    {
        _objOverlap = null;
        Vector2Int _min = _obj.GetMinPos(_center);
        Vector2Int _max = _obj.GetMaxPos(_center);
        if (!_force)
            if (!CheckPos(_min, _max, out _objOverlap))
                return false;
        for (int x = _min.x; x <= _max.x; x++)
            for (int y = _min.y; y <= _max.y; y++)
                i_grid[x][y]._moduleObject = _obj;

        _obj.I_BG.color = Color.white;
        _obj.SetBounds(_min, _max);
        CheckDoors(_obj);
        return true;
    }
    void PickUpModule(LayoutModuleObject _obj)
    {
        for (int x = _obj.minPos.x; x <= _obj.maxPos.x; x++)
            for (int y = _obj.minPos.y; y <= _obj.maxPos.y; y++)
                i_grid[x][y]._moduleObject = null;

        _obj.PickedUp();
    }

    void CheckDoors(LayoutModuleObject _obj)
    {
        Vector2Int _pos;
        int _rot;
        LayoutModuleDoor _door;
        LayoutModuleObject _other;
        foreach (var item in _obj.LMO_doorList)
        {
            item.GetWorldPos(out _pos, out _rot);
            switch (_rot)
            {
                case 0:
                    _pos += Vector2Int.left;
                    if (item.entryType == LevelGen_Block.entryTypeEnum.wideDoor)
                        _pos += Vector2Int.up;
                    break;
                case 1: 
                    _pos += Vector2Int.down;
                    if (item.entryType == LevelGen_Block.entryTypeEnum.wideDoor)
                        _pos += Vector2Int.left;
                    break;
                case 2: 
                    _pos += Vector2Int.right;
                    if (item.entryType == LevelGen_Block.entryTypeEnum.wideDoor)
                        _pos += Vector2Int.down;
                    break;
                case 3: 
                    _pos += Vector2Int.up;
                    if (item.entryType == LevelGen_Block.entryTypeEnum.wideDoor)
                        _pos += Vector2Int.right;
                    break;
                default: Debug.LogError("Out Of Range!"); continue;
            }
            CheckPos(_pos, _pos, out _other);

            _rot = (_rot + 2) % 4;

            if (!_other)
                continue;
            if (!_other.GetDoorFromWorldPos(_pos, _rot, out _door))
                continue;

            item.SetConnected(_door);
            _door.SetConnected(item);
        }
    }

    void MoveCursor_Mouse ()
    {
        v2_cursorPosition = Input.mousePosition;

        v2_cursorPosition /= new Vector2(Screen.width, Screen.height);
        v2_cursorPosition *= v2_canvasSize;
        v2_cursorPosition -= v2_canvasHalf;

        v2_cursorPosition.x = Mathf.Clamp(v2_cursorPosition.x, -v2_canvasHalf.x, v2_canvasHalf.x);
        v2_cursorPosition.y = Mathf.Clamp(v2_cursorPosition.y, -v2_canvasHalf.y, v2_canvasHalf.y);

        RT_cursor.anchoredPosition = v2_cursorPosition;
    }
    void MoveCursor_Gamepad(Vector2 dir, float _speed)
    {
        v2_cursorPosition += dir * _speed;

        v2_cursorPosition.x = Mathf.Clamp(v2_cursorPosition.x, -v2_canvasHalf.x, v2_canvasHalf.x);
        v2_cursorPosition.y = Mathf.Clamp(v2_cursorPosition.y, -v2_canvasHalf.y, v2_canvasHalf.y);

        RT_cursor.anchoredPosition = v2_cursorPosition;
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
    }

    void UpdateSelected()
    {
        Vector2 _v = new Vector2(-v2_cursorPosition.y, v2_cursorPosition.x);
        
        _v.y -= RT_holder.anchoredPosition.x;
        _v.x += RT_holder.anchoredPosition.y;

        _v /= F_canvasZoom;

        _v += v2_areaHalf;
        _v /= I_spacing;
        Vector2Int v2 = Vector2Int.RoundToInt(_v);
        if (v2 != v2_selSquare)
            UpdateSelected(v2);
    }
    void UpdateSelected(Vector2Int v2)
    {
        if (InBounds(v2_selSquare))
            i_grid[v2_selSquare.x][v2_selSquare.y].Deselect();
        v2_selSquare = v2;
        if (InBounds(v2_selSquare))
            i_grid[v2_selSquare.x][v2_selSquare.y].Select();
        MoveActiveLMO(v2);
    }

    bool InBounds(Vector2Int v2)
    {
        if (v2.x < 0) return false;
        if (v2.x >= i_grid.Count) return false;
        if (v2.y < 0) return false;
        if (v2.y >= i_grid[0].Count) return false;
        return true;
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

    void GenerateSquares(Layout_Bounds _bounds)
    {
        RT_holder.DeleteChildren();
        i_grid.Clear();
        Vector2 _offset = new Vector2(_bounds.data.Count, -_bounds.data[0].d.Count) * -0.5f * I_spacing;

        for (int x = 0; x < _bounds.data.Count; x++)
        {
            i_grid.Add(new List<cellClass>());
            for (int y = 0; y < _bounds.data[x].d.Count; y++)
            {
                cellClass CC = new cellClass();
                i_grid[x].Add(CC);
                switch (_bounds.data[x].d[y].roomtype)
                {
                    case Layout_Bounds.roomEnum.empty:
                        break;
                    case Layout_Bounds.roomEnum.placeable:
                        RectTransform RT = Instantiate(PF_square, RT_holder);
                        RT.anchoredPosition = (new Vector2(y, -x) * I_spacing) + _offset;

                        CC._rt = RT;
                        CC._image = RT.GetComponent<Image>();
                        break;
                    default:
                        break;
                }
            }
        }
        v2_areaHalf = new Vector2(_bounds.data[0].d.Count, _bounds.data.Count) * I_spacing / 2;
        v2_canvasPos_Min = -v2_areaHalf;
        v2_canvasPos_Max = v2_areaHalf;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
