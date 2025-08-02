using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LayoutCustomize : MonoBehaviour
{
    public static LayoutCustomize Instance;
    protected bool b_active;

    public saveClass save;
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
    [System.Serializable]
    public class saveClass
    {
        public LevelGen_Theme _theme;
        public Layout_Bounds _bounds;
        public List<saveClass_Object> _objects = new List<saveClass_Object>();
    }
    [System.Serializable]
    public class saveClass_Object
    {
        public LevelGen_Block _block;
        public Vector2Int _pos;
        public int _rot = 0;
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
        CanvasSize_Update();
        BuildMenu_Update(save._theme);
        GenerateSquares(save._bounds);
        LoadObjects();
    }

    void LoadObjects()
    {
        foreach (var item in save._objects)
        {
            LayoutModuleObject MO = Instantiate(PF_moduleObject, RT_holder);
            MO.Setup(item._block);
            MoveLMO(item._pos, MO, true);
            if (!SetDownModule(MO, item._pos))
            {
                Destroy(MO.gameObject);
                continue;
            }
        }
    }
    public void OnUpdate(PlayerController _PC)
    {
        if (!b_active) return;
        MoveCursor(_PC);
        ClickManager(_PC);
    }
    void MoveCursor(PlayerController _PC)
    {
        if (_PC.Inputs.b_isGamepad)
            MoveCursor_Gamepad(_PC.Inputs.v2_inputDir);
        else
            MoveCursor_Mouse();
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
        if (!SetDownModule(LMO_active, v2_selSquare))
            return false;
        LMO_list.Add(LMO_active);
        LMO_active = null;

        return true;
    }
    bool LMO_InactiveClick(PlayerController _PC)
    {
        if (LMO_active != null)
            return false;
        LMO_active = GetModuleObject(v2_selSquare);
        if (LMO_active == null)
            return false;
        PickUpModule(LMO_active);
        LMO_list.Remove(LMO_active);

        return true;
    }
    LayoutModuleObject GetModuleObject(Vector2Int pos)
    {
        if (!CheckPos(pos)) return null;
        return i_grid[pos.x][pos.y]._moduleObject;
    }
    public void LeftTab()
    {
        if (!G_buildMenu.activeSelf) ShowBuild(PlayerManager.main);
        else HideBuild(PlayerManager.main);
    }
    public void RightTab()
    {

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
        StartCoroutine(RT_holder.Move(new Vector2(v2_canvasHalf.x + i_buildMenuSlide, 0), Quaternion.identity, true, 0.1f));
        G_leftColumn.SetActive(false);
        G_buildMenu.SetActive(true);
        if (_PC.Inputs.b_isGamepad)
            EventSystem.current.SetSelectedGameObject(B_defaultSelected.gameObject);
    }
    void HideBuild(PlayerController _PC)
    {
        StartCoroutine(RT_holder.Move(new Vector2(v2_canvasHalf.x, 0), Quaternion.identity, true, 0.1f));
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
        Vector2 size = LMO.Block.size;
        v2.x -= ((size.y - 1) % 2) / 2f;
        v2.y -= ((size.x - 1) % 2) / 2f;

        v2 *= I_spacing;
        v2 -= v2_areaHalf;
        v2 = new Vector2(v2.y, -v2.x);

        if (_instant)
            LMO.RT.localPosition = v2;
        else
            StartCoroutine(LMO.RT.Move(v2, Quaternion.identity, true, 0.1f));
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
        return CheckPos(_min, _max);
    }
    bool CheckPos(Vector2Int _min, Vector2Int _max)
    {
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
                    return false;
            }
        }
        return true;
    }
    bool SetDownModule(LayoutModuleObject _obj, Vector2Int _center)
    {
        Vector2Int _min = _obj.GetMinPos(_center);
        Vector2Int _max = _obj.GetMaxPos(_center);
        if (!CheckPos(_min, _max)) return false;
        for (int x = _min.x; x <= _max.x; x++)
        {
            for (int y = _min.y; y <= _max.y; y++)
            {
                i_grid[x][y]._moduleObject = _obj;
            }
        }
        _obj.I_BG.color = Color.white;
        _obj.SetBounds(_min, _max);
        return true;
    }
    void PickUpModule(LayoutModuleObject _obj)
    {
        for (int x = _obj.minPos.x; x <= _obj.maxPos.x; x++)
        {
            for (int y = _obj.minPos.y; y <= _obj.maxPos.y; y++)
            {
                i_grid[x][y]._moduleObject = null;
            }
        }
        _obj.SetBounds(-Vector2Int.one, -Vector2Int.one);
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
    void MoveCursor_Gamepad(Vector2 dir)
    {
        v2_cursorPosition += dir * f_cursorSpeed;

        v2_cursorPosition.x = Mathf.Clamp(v2_cursorPosition.x, -v2_canvasHalf.x, v2_canvasHalf.x);
        v2_cursorPosition.y = Mathf.Clamp(v2_cursorPosition.y, -v2_canvasHalf.y, v2_canvasHalf.y);

        RT_cursor.anchoredPosition = v2_cursorPosition;
    }

    void UpdateSelected()
    {
        Vector2 _v = new Vector2(-v2_cursorPosition.y, v2_cursorPosition.x);
        
        _v.y -= RT_holder.anchoredPosition.x;
        _v.x -= RT_holder.anchoredPosition.y;

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
    }

    public void Hide()
    {
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
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
