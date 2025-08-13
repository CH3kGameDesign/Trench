using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.IO;
using Unity.VisualScripting;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class PointerBuilder : MonoBehaviour
{
    private string S_root = "Assets/Prefabs/Environment";
    public string S_themeName = "RoomBuilder";
    [Space(10)]
    public LevelGen_Materials _Materials;
    public LevelGen_Placeables _Placeables;
    public enum drawModes { square, point, stretch, move, extend }
    public enum beltModes { build, paint, place}
    [HideInInspector] public drawModes drawMode;
    [HideInInspector] public beltModes beltMode;

    public GameObject planeVisual;

    [HideInInspector]
    public Transform activeSquare;
    [HideInInspector]
    public SurfaceUpdater activeWall;
    [HideInInspector]
    public Transform activeArrow;
    public GameObject squarePrefab;

    public LevelGen_Block PF_blockTemplate;
    private LevelGen_Block lg_block;

    [HideInInspector]
    public float gridSize = 1;

    [HideInInspector]
    public float height = 0;

    public Transform grid;

    private Vector3 firstClickPos;
    private float _totalChange = 0;

    public GameObject arrow;


    public TextMeshProUGUI drawModeText;
    
    public Architraves architraves;
    public layerClass _layer;
    [System.Serializable]
    public class layerClass
    {
        public LayerMask gridMask;
        public LayerMask interactableMask;
        public LayerMask floorMask;
        public LayerMask wallMask;
        public LayerMask ceilingMask;
        public LayerMask arrowMask;
    }

    public CanvasClass _canvas;
    [System.Serializable]
    public class CanvasClass
    {
        public Canvas mainCanvas;
        public RectTransform RT_canvas;
        public Button[] toolBelt_Buttons;
        [HideInInspector] public ToolBeltButton[] toolBeltSub_Buttons;
        [HideInInspector] public ToolBeltButton[] toolBeltFull_Buttons;

        public ToolBeltButton PF_toolBeltSub;
        public ToolBeltButton PF_toolBeltFull;

        public RectTransform RT_toolBeltSub;
        public RectTransform RT_toolBeltFull;

        public RectTransform RT_cursorImage;
        public RawImage RI_cursorImage;

        public TMP_InputField TM_inputField;
    }
    public BuildClass _Build;
    public PaintClass _Paint;
    public PlaceClass _Place;

    #region Belt Classes
    [System.Serializable]
    public class BeltClass
    {
        [HideInInspector] public int i_lastSel = 0;
        public virtual void GenerateBelt(PointerBuilder PB, RectTransform _hook)
        {
            List<SubClass> _list = GetList();
            PB._canvas.toolBeltSub_Buttons = new ToolBeltButton[_list.Count];
            int i = 0;
            foreach (var item in _list)
            {
                ToolBeltButton TBB = Instantiate(PB._canvas.PF_toolBeltSub, _hook);
                TBB.Setup(this, item, PB);
                TBB.Selected(i == i_lastSel);
                PB._canvas.toolBeltSub_Buttons[i] = TBB;
                i++;
            }
            Vector2 _size = _hook.sizeDelta;
            GridLayoutGroup GLG = _hook.GetComponent<GridLayoutGroup>();
            _size.y = (1 + Mathf.Ceil(_list.Count / 3)) * (GLG.cellSize.y + GLG.spacing.y);
            _hook.sizeDelta = _size;
            PB.GenerateBelt_Full(this, _list[i_lastSel]);
        }
        public virtual List<SubClass> GetList()
        {
            return new List<SubClass>();
        }
        public void UpdateItemsList()
        {
            List<SubClass> _list = GetList();
            int i = 0;
            foreach (var item in _list)
            {
                item.index = i;
                item.UpdateItemsList();
                i++;
            }
        }
        public virtual void Setup(PointerBuilder PB)
        {
            UpdateItemsList();
        }
    }
    [System.Serializable]
    public class BuildClass : BeltClass
    {
        public List<BuildSubClass> list;
        public override void GenerateBelt(PointerBuilder PB, RectTransform _hook)
        {
            PB.beltMode = beltModes.build;
            base.GenerateBelt(PB, _hook);
        }
        public override List<SubClass> GetList()
        {
            List<SubClass> _list = new List<SubClass>();
            foreach (var item in list)
                _list.Add(item);
            return _list;
        }
    }
    [System.Serializable]
    public class PaintClass : BeltClass
    {
        public List<PaintSubClass> list;
        public Material activeMat;
        public override void GenerateBelt(PointerBuilder PB, RectTransform _hook)
        {
            PB.beltMode = beltModes.paint;
            base.GenerateBelt(PB, _hook);
        }
        public override List<SubClass> GetList()
        {
            List<SubClass> _list = new List<SubClass>();
            foreach (var item in list)
                _list.Add(item);
            return _list;
        }
        public override void Setup(PointerBuilder PB)
        {
            base.Setup(PB);
            SubClass SC = list[i_lastSel];
            SC.items[SC.i_lastSel].OnSelect(PB);
        }
    }
    [System.Serializable]
    public class PlaceClass : BeltClass
    {
        public List<PlaceSubClass> list;
        [HideInInspector] public Prefab_Environment activePrefab;
        [HideInInspector] public Prefab_Environment activeTransform;
        [HideInInspector] public Prefab_Environment lastTransform;
        [HideInInspector] public LayerMask activeLM;
        public RoomCreation_ItemInfo RC_itemInfo;
        public override void GenerateBelt(PointerBuilder PB, RectTransform _hook)
        {
            PB.beltMode = beltModes.place;
            base.GenerateBelt(PB, _hook);
        }
        public override List<SubClass> GetList()
        {
            List<SubClass> _list = new List<SubClass>();
            foreach (var item in list)
                _list.Add(item);
            return _list;
        }
        public override void Setup(PointerBuilder PB)
        {
            base.Setup(PB);
            SubClass SC = list[i_lastSel];
            if (SC.items.Count > 0)
                SC.items[SC.i_lastSel].OnSelect(PB);
        }

        public void UpdateLayerMask(PointerBuilder PB, Prefab_Environment.TypeEnum prefabType)
        {
            switch (prefabType)
            {
                case Prefab_Environment.TypeEnum.floor: activeLM = PB._layer.floorMask; break;
                case Prefab_Environment.TypeEnum.wall: activeLM = PB._layer.wallMask; break;
                case Prefab_Environment.TypeEnum.ceiling: activeLM = PB._layer.ceilingMask; break;
                case Prefab_Environment.TypeEnum.door: activeLM = PB._layer.wallMask; break;
                default: activeLM = PB._layer.interactableMask; break;
            }
        }
    }
    #endregion
    #region Sub Classes
    [System.Serializable]
    public class SubClass
    {
        public string name;
        public Sprite image;
        [HideInInspector] public List<SubItem> items;
        [HideInInspector] public int index;
        [HideInInspector] public int i_lastSel = 0;
        public virtual void GenerateBelt_Full(PointerBuilder PB, RectTransform _hook)
        {
            PB._canvas.toolBeltFull_Buttons = new ToolBeltButton[items.Count];
            int i = 0;
            foreach (var item in items)
            {
                ToolBeltButton TBB = Instantiate(PB._canvas.PF_toolBeltSub, _hook);
                TBB.Setup(this, item, PB);
                TBB.Selected(i == i_lastSel);
                PB._canvas.toolBeltFull_Buttons[i] = TBB;
                i++;
            }
            Vector2 _size = _hook.sizeDelta;
            GridLayoutGroup GLG = _hook.GetComponent<GridLayoutGroup>();
            _size.x = (1 + Mathf.Ceil(items.Count / 2)) * (GLG.cellSize.x + GLG.spacing.x);
            _hook.sizeDelta = _size;

            if (items.Count > i_lastSel)
                items[i_lastSel].OnSelect(PB);
            else
                PB.UpdateCursor_Image(null);
        }
        public virtual void UpdateItemsList()
        {
            int i = 0;
            foreach(var item in items)
            {
                item.index = i;
                i++;
            }
        }
        public virtual void OnSelect(PointerBuilder PB)
        {
            PB.Update_ArrowTypes();
        }
    }
    [System.Serializable]
    public class BuildSubClass : SubClass
    {
        public List<BuildItem> itemsOverride = new List<BuildItem>();
        public drawModes _mode;
        public override void UpdateItemsList()
        {
            items = new List<SubItem>();
            items.AddRange(itemsOverride);
            base.UpdateItemsList();
        }
        public override void OnSelect(PointerBuilder PB)
        {
            PB.drawMode = _mode;
            base.OnSelect(PB);
        }
    }
    [System.Serializable]
    public class PaintSubClass : SubClass
    {
        public string _id;
        public override void UpdateItemsList()
        {
            items = new List<SubItem>();
            items.AddRange(LevelGen_Materials.Instance.GetSubItemList(_id));
            base.UpdateItemsList();
        }
        public override void OnSelect(PointerBuilder PB)
        {
            base.OnSelect(PB);
        }
    }
    [System.Serializable]
    public class PlaceSubClass : SubClass
    {
        public string _id;
        public override void UpdateItemsList()
        {
            items = new List<SubItem>();
            items.AddRange(LevelGen_Placeables.Instance.GetSubItemList(_id));
            base.UpdateItemsList();
        }
        public override void OnSelect(PointerBuilder PB)
        {
            base.OnSelect(PB);
            PB._canvas.toolBeltFull_Buttons[i_lastSel].Selected(false);
        }
        public override void GenerateBelt_Full(PointerBuilder PB, RectTransform _hook)
        {
            base.GenerateBelt_Full(PB, _hook);
            if (i_lastSel < PB._canvas.toolBeltFull_Buttons.Length)
                PB._canvas.toolBeltFull_Buttons[i_lastSel].Selected(false);
            PB.UpdateCursor_Image(null);
            PB._Place.activePrefab = null;
        }
    }
    [System.Serializable]
    public class SubItem
    {
        public string name;
        public Texture2D image;
        [HideInInspector] public int index;
        public virtual void OnSelect(PointerBuilder PB)
        {
            if (index < PB._canvas.toolBeltFull_Buttons.Length)
                PB._canvas.toolBeltFull_Buttons[index].Selected(true);
            PB.UpdateCursor_Image(image);
        }
    }
    public class BuildItem : SubItem
    {
        public override void OnSelect(PointerBuilder PB)
        {
            base.OnSelect(PB);
        }
    }
    public class PaintItem : SubItem
    {
        public Material _mat;
        public override void OnSelect(PointerBuilder PB)
        {
            PB._Paint.activeMat = _mat;
            base.OnSelect(PB);
        }
    }
    public class PlaceItem : SubItem
    {
        public Prefab_Environment _prefab;
        public override void OnSelect(PointerBuilder PB)
        {
            if (PB._Place.activePrefab == _prefab)
            {
                PB._canvas.toolBeltFull_Buttons[index].Selected(false);
                PB._Place.activePrefab = null;
                PB.UpdateCursor_Image(null);
            }
            else
            {
                PB._Place.activePrefab = _prefab;
                PB._Place.UpdateLayerMask(PB, _prefab._type);
                base.OnSelect(PB);
            }
        }
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        gridSize = 1;
        LevelGen_Materials.Instance = _Materials;
        LevelGen_Placeables.Instance = _Placeables;
        GenerateBelt_Sub(0);
        Setup();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCursor_Pos();
        switch (beltMode)
        {
            case beltModes.build:
                Update_Build();
                break;
            case beltModes.paint:
                Update_Paint();
                break;
            case beltModes.place:
                Update_Place();
                break;
            default:
                break;
        }
        if (Input.GetMouseButtonUp(0))
            UnfocusGrid();
    }
    void UpdateCursor_Pos ()
    {
        Vector2 _tarPos = Input.mousePosition;
        _tarPos.x = (_tarPos.x/Screen.width) * _canvas.RT_canvas.sizeDelta.x;
        _tarPos.y = (_tarPos.y / Screen.height) * _canvas.RT_canvas.sizeDelta.y;
        _canvas.RT_cursorImage.anchoredPosition = _tarPos;
    }
    void UpdateCursor_Image(Texture _image)
    {
        _canvas.RI_cursorImage.texture = _image;
        _canvas.RI_cursorImage.gameObject.SetActive(_image != null);
    }
    void Update_Build ()
    {
        switch (drawMode)
        {
            case drawModes.square:
                Update_BuildDraw_Square();
                break;
            case drawModes.point:
                Update_BuildDraw_Point();
                break;
            default:
                Update_BuildEdit();
                break;
        }
    }

    void Update_BuildDraw_Square()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, 1000))
                {
                    int l = hit.transform.gameObject.layer;
                    if (_layer.gridMask.Check(l))
                    {
                        if (activeWall != null)
                            activeWall.GetComponentInParent<RoomUpdater>().HideArrows();
                        firstClickPos = GetPos();

                        CreateNewSquare();
                    }
                }
            }
        }
        //Update
        if (activeWall != null)
        activeWall.GetComponentInParent<RoomUpdater>().HideArrows();
        drawModeText.text = "Draw Mode";
        if (Input.GetMouseButton(0) && activeSquare != null)
        {
            SetPosScale(firstClickPos, GetPos());
        }
        if (Input.GetMouseButtonUp(0) && activeSquare != null)
        {
            Vector3 _compare = GetPos();
            if (firstClickPos.x == _compare.x || firstClickPos.z == _compare.z)
                GameObject.Destroy(activeSquare.gameObject);
            activeSquare = null;
        }
    }

    void CreateNewSquare(float _height = 4f)
    {
        GameObject GO;
        RoomUpdater RU;
        SurfaceUpdater SU;


        GO = Instantiate(squarePrefab, lg_block.T_blockHolder);

        Mesh tempMesh = new Mesh();
        tempMesh.vertices = new Vector3[]
        {
                                    Vector3.zero,
                                    new Vector3(0,0,1),
                                    new Vector3(1,0,1),
                                    new Vector3(1,0,0)
        };

        Vector2[] temp2Dsquare = new Vector2[tempMesh.vertices.Length];
        for (int i = 0; i < temp2Dsquare.Length; i++)
            temp2Dsquare[i] = new Vector2(tempMesh.vertices[i].x, tempMesh.vertices[i].z);
        Triangulator trSquare = new Triangulator(temp2Dsquare);
        int[] indicesSquare = trSquare.Triangulate();

        tempMesh.triangles = indicesSquare;
        tempMesh.uv = temp2Dsquare;

        tempMesh.RecalculateNormals();
        tempMesh.RecalculateBounds();


        GO.name = Time.time.ToString();
        //Mesh meshTemp = new Mesh();
        //Mesh meshTemp2 = GO.GetComponent<MeshFilter>().mesh;
        //meshTemp.vertices = meshTemp2.vertices;
        //meshTemp.uv = meshTemp2.uv;
        //meshTemp.triangles = meshTemp2.triangles;
        activeSquare = GO.transform;
        RU = GO.AddComponent<RoomUpdater>();
        SU = GO.AddComponent<SurfaceUpdater>();
        SU.Setup(RU, SurfaceUpdater.enumType.floor);
        RU.roomName = GO.name;
        RU.SU_Floor = SU;

        SU.mf.mesh = tempMesh;
        SU.mc.sharedMesh = tempMesh;
        RU.floor = GO.transform;
        RU.arrow = arrow;

        RU.height = _height;

        RU.architraves = architraves;

        //CEILING//////////
        GO = Instantiate(squarePrefab, activeSquare);
        SU = GO.AddComponent<SurfaceUpdater>();
        SU.Setup(RU, SurfaceUpdater.enumType.ceiling);
        RU.SU_Ceiling = SU;

        SU.mf.mesh = tempMesh;
        SU.mc.sharedMesh = tempMesh;
        AddWalls(RU, _height);
    }
    void Update_BuildDraw_Point()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, 1000))
                {
                    int l = hit.transform.gameObject.layer;
                    if (_layer.gridMask.Check(l))
                    {
                        GameObject GO;
                        RoomUpdater RU;
                        SurfaceUpdater SU;
                        Vector3 clickPos = GetPos();
                        if (activeWall != null)
                            activeWall.GetComponentInParent<RoomUpdater>().HideArrows();
                        if (activeSquare == null)
                        {
                            GO = Instantiate(squarePrefab, lg_block.T_blockHolder);
                            GO.name = Time.time.ToString();
                            Mesh meshTempPoint = new Mesh();
                            //Mesh meshTempPoint2 = GO1.GetComponent<MeshFilter>().mesh;
                            //meshTempPoint.vertices = meshTempPoint2.vertices;
                            //meshTempPoint.uv = meshTempPoint2.uv;
                            //meshTempPoint.triangles = meshTempPoint2.triangles;
                            activeSquare = GO.transform;
                            RU = GO.AddComponent<RoomUpdater>();
                            SU = GO.AddComponent<SurfaceUpdater>();
                            SU.Setup(RU, SurfaceUpdater.enumType.floor);
                            RU.roomName = GO.name;
                            RU.floor = GO.transform;
                            RU.arrow = arrow;

                            RU.architraves = architraves;

                            SU.mf.mesh = meshTempPoint;
                            RU.vertPos = new Vector3[0];
                        }
                        RoomUpdater ru = activeSquare.GetComponent<RoomUpdater>();

                        Vector3[] tempVerts = new Vector3[ru.vertPos.Length + 1];

                        for (int i = 0; i < ru.vertPos.Length; i++)
                            tempVerts[i] = ru.vertPos[i];

                        tempVerts[tempVerts.Length - 1] = clickPos;

                        ru.vertPos = tempVerts;
                        if (tempVerts.Length == 2)
                        {
                            AddWallSingle(ru);
                            ru.UpdateWall(ru.walls[ru.walls.Count - 1]);
                        }
                        if (tempVerts.Length >= 2)
                        {
                            ru.walls[0].verts = new Vector2Int(ru.vertPos.Length - 1, 0);
                            AddWallSingle(ru);
                            ru.UpdateWall(ru.walls[ru.walls.Count - 1]);
                            ru.UpdateWall(ru.walls[0]);
                        }

                        if (tempVerts.Length >= 3)
                        {
                            Vector2[] temp2D = new Vector2[tempVerts.Length];
                            for (int i = 0; i < temp2D.Length; i++)
                                temp2D[i] = new Vector2(tempVerts[i].x, tempVerts[i].z);
                            Triangulator tr = new Triangulator(temp2D);
                            int[] indices = tr.Triangulate();

                            ru.SU_Floor.mf.mesh.vertices = tempVerts;
                            ru.SU_Floor.mf.mesh.triangles = indices;
                            ru.SU_Floor.mf.mesh.uv = temp2D;

                            ru.SU_Floor.mf.mesh.RecalculateNormals();
                            ru.SU_Floor.mf.mesh.RecalculateBounds();

                            ru.SU_Floor.mc.sharedMesh = ru.SU_Floor.mf.mesh;
                        }
                    }
                }
            }
        }
        //UPDATE
        if (activeWall != null)
            activeWall.GetComponentInParent<RoomUpdater>().HideArrows();
        drawModeText.text = "Point Mode";
    }
    void Update_ArrowTypes()
    {
        if (activeWall != null)
        {
            if (beltMode != beltModes.build)
            {
                activeWall.RU.HideArrows();
                return;
            }
            switch (drawMode)
            {
                case drawModes.square:
                    activeWall.RU.HideArrows();
                    break;
                case drawModes.point:
                    activeWall.RU.HideArrows();
                    break;
                default:
                    activeWall.RU.ShowArrows(drawMode);
                    break;
            }
        }
    }
    
    void Update_BuildEdit()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _totalChange = 0;
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, 1000, _layer.arrowMask))
                {
                    activeArrow = hit.transform;
                    activeArrow.localScale = Vector3.one * 1.1f;
                    firstClickPos = GetPos();
                    FocusGrid(activeArrow);
                    RoomUpdater RM = activeWall.RU;
                    if (RM.upArrow == activeArrow.parent.gameObject)
                        activeWall = RM.SU_Ceiling;
                    if (RM.downArrow == activeArrow.parent.gameObject)
                        activeWall = RM.SU_Floor;
                    foreach (var item in RM.walls)
                        if (item.arrow == activeArrow.parent.gameObject)
                            activeWall = item.SU;
                    switch (drawMode)
                    {
                        case drawModes.extend:
                            CreateNewSquare(activeWall.RU.height);
                            break;
                        default:
                            break;
                    }
                    return;
                }
                if (Physics.Raycast(ray, out hit, 1000))
                {
                    int l = hit.transform.gameObject.layer;
                    if (_layer.interactableMask.Check(l))
                    {
                        SurfaceUpdater activeWallTemp = GetWall();
                        if (activeWallTemp != activeWall && activeWall != null)
                            activeWall.RU.HideArrows();
                        activeWall = activeWallTemp;
                        activeWall.RU.ShowArrows(drawMode);
                    }
                }
            }
        }
        drawModeText.text = "Edit Mode";
        if (activeArrow != null)
        {
            if (Input.GetMouseButton(0))
            {
                Vector3 temp = GetPos();
                //temp = new Vector3(temp.x, firstClickPos.y, temp.z);
                Vector3 changeFinal = temp - firstClickPos;
                changeFinal = ClampPoint(changeFinal, activeArrow.up * -1, activeArrow.up * 1);
                switch (drawMode)
                {
                    case drawModes.stretch:
                        Arrow_Stretch(activeWall.RU, changeFinal);
                        break;
                    case drawModes.move:
                        Arrow_Move(activeWall.RU, changeFinal);
                        break;
                    case drawModes.extend:
                        Arrow_Extend(activeWall.RU, changeFinal);
                        break;
                    default:
                        break;
                }

                activeWall.RU.UpdateMeshes();
                firstClickPos = temp;
            }
            if (Input.GetMouseButtonUp(0))
            {
                activeWall.RU.ShowArrows(drawMode);
                activeArrow.localScale = Vector3.one;
                switch (drawMode)
                {
                    case drawModes.stretch:
                        if (activeWall != null)
                        {
                            if (!activeWall.RU.SU_Floor.mr.bounds.IsValid())
                                DeleteSquare();
                        }
                        break;
                    case drawModes.extend:
                        if (activeSquare != null)
                        {
                            if (Mathf.Abs(_totalChange) <= 0.01f)
                                GameObject.Destroy(activeSquare.gameObject);
                            activeSquare = null;
                        }
                        break;
                    default:
                        break;
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.Delete))
            DeleteSquare();
    }
    void Update_Paint()
    {
        bool _deselect = true;
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 1000))
            {
                int l = hit.transform.gameObject.layer;
                if (_layer.interactableMask.Check(l))
                {
                    SurfaceUpdater SU;
                    if (hit.transform.TryGetComponent<SurfaceUpdater>(out SU))
                    {
                        _deselect = false;
                        if (SU != activeWall)
                        {
                            DeselectWall();
                            SU.OnHover_Paint(_Paint.activeMat);
                            activeWall = SU;
                        }
                        if (Input.GetMouseButton(0))
                        {
                            SU.OnClick_Paint(_Paint.activeMat);
                        }
                    }
                }
            }
        }
        if (_deselect)
            DeselectWall();
    }
    void Update_Place()
    {
        if (_Place.activePrefab)
        {
            if (Input.GetMouseButtonDown(0)) Place_CreateObject();
            if (Input.GetMouseButton(0)) Place_MoveObject();
            if (Input.GetMouseButtonUp(0)) Place_PlaceObject();
        }
        else
        {
            if (Input.GetMouseButtonDown(0)) Place_GetObject();
            if (Input.GetMouseButton(0)) Place_MoveObject();
            if (Input.GetMouseButtonUp(0)) Place_PlaceObject();
        }

        Place_ActiveInfo();
    }

    void Place_ActiveInfo()
    {
        if (_Place.lastTransform == null)
        { 
            _Place.RC_itemInfo.gameObject.SetActive(false);
            return;
        }
        _Place.RC_itemInfo.gameObject.SetActive(true);
        _Place.RC_itemInfo.RT.FollowObject(_canvas.mainCanvas, _Place.lastTransform.transform);
        switch (_Place.lastTransform._type)
        {
            case Prefab_Environment.TypeEnum.floor:
                _Place.RC_itemInfo.G_rotateButtons.SetActive(true);
                break;
            case Prefab_Environment.TypeEnum.ceiling:
                _Place.RC_itemInfo.G_rotateButtons.SetActive(true);
                break;
            default:
                _Place.RC_itemInfo.G_rotateButtons.SetActive(false);
                break;
        }
    }
    public void Place_ActiveInfo_Rotate(bool _left)
    {
        if (_Place.lastTransform == null)
            return;
        _Place.lastTransform.transform.localEulerAngles += new Vector3(0, _left ? -90f : 90f, 0);
    }
    public void Place_ActiveInfo_Delete()
    {
        if (_Place.lastTransform == null)
            return;
        _Place.lastTransform.SU_surface.RemoveFurniture(_Place.lastTransform);
        Destroy(_Place.lastTransform.gameObject);
        _Place.lastTransform = null;
    }
    void Place_CreateObject()
    {
        if (!_Place.activePrefab)
            return;
        if (EventSystem.current.IsPointerOverGameObject())
            return;
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 1000, _layer.interactableMask))
        {
            int l = hit.transform.gameObject.layer;
            if (_Place.activeLM.Check(l))
            {
                _Place.lastTransform = null;
                _Place.activeTransform = Instantiate(_Place.activePrefab, hit.transform);

                Vector3 _pos = hit.point;
                if (!Input.GetKey(KeyCode.LeftControl))
                    _pos = ClampToGrid(_pos);

                SurfaceUpdater _SU;
                if (hit.transform.TryGetComponent<SurfaceUpdater>(out _SU))
                {
                    _SU.AddFurniture(_Place.activeTransform);
                }

                switch (_Place.activeTransform._type)
                {
                    case Prefab_Environment.TypeEnum.wall:
                        _Place.activeTransform.transform.position = _pos;
                        _Place.activeTransform.transform.forward = -hit.normal;
                        break;
                    case Prefab_Environment.TypeEnum.door:
                        if (_SU)
                            _pos.y = _SU.RU.floor.transform.position.y;
                        _Place.activeTransform.transform.position = _pos;
                        _Place.activeTransform.transform.forward = -hit.normal;
                        if (_SU)
                            _SU.UpdateSurface();
                        break;
                    default:
                        _Place.activeTransform.transform.position = _pos;
                        break;
                }
            }
        }
    }
    void Place_MoveObject()
    {
        if (!_Place.activeTransform)
            return;
        if (EventSystem.current.IsPointerOverGameObject())
            return;
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 1000, _layer.interactableMask))
        {
            int l = hit.transform.gameObject.layer;
            if (_Place.activeLM.Check(l))
            {
                _Place.activeTransform.transform.parent = hit.transform;
                Vector3 _pos = hit.point;
                if (!Input.GetKey(KeyCode.LeftControl))
                    _pos = ClampToGrid(_pos);

                SurfaceUpdater _SU;
                if (hit.transform.TryGetComponent<SurfaceUpdater>(out _SU))
                {
                    if (_Place.activeTransform.SU_surface != _SU)
                    {
                        _Place.activeTransform.SU_surface.RemoveFurniture(_Place.activeTransform);
                        _SU.AddFurniture(_Place.activeTransform);
                    }
                }

                switch (_Place.activeTransform._type)
                {
                    case Prefab_Environment.TypeEnum.wall:
                        _Place.activeTransform.transform.position = _pos;
                        _Place.activeTransform.transform.forward = -hit.normal;
                        break;
                    case Prefab_Environment.TypeEnum.door:
                        if (_SU)
                            _pos.y = _SU.RU.floor.transform.position.y;
                        _Place.activeTransform.transform.position = _pos;
                        _Place.activeTransform.transform.forward = -hit.normal;
                        if (_SU)
                        {
                            _SU.SortHoles();
                            _SU.UpdateSurface();
                        }
                        break;
                    default:
                        _Place.activeTransform.transform.position = _pos;
                        break;
                }
            }
        }
    }
    void Place_PlaceObject()
    {
        if (!_Place.activeTransform)
            return;

        if (!Input.GetKey(KeyCode.LeftShift) || _Place.activePrefab == null)
        {
            UpdateCursor_Image(null);
            _Place.activePrefab = null;
            _canvas.toolBeltFull_Buttons[_Place.list[_Place.i_lastSel].i_lastSel].Selected(false);

            _Place.lastTransform = _Place.activeTransform;
        }
        _Place.activeTransform = null;
    }

    void Place_GetObject()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;
        _Place.lastTransform = null;
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 1000))
        {
            Prefab_Environment _PE = hit.transform.GetComponentInParent<Prefab_Environment>();
            if (_PE)
            {
                _Place.activeTransform = _PE;
                _Place.UpdateLayerMask(this, _PE._type);
            }
        }
    }

    #region Player Interactions
    void Arrow_Move(RoomUpdater RM, Vector3 changeFinal)
    {
        for (int i = 0; i < RM.moveArrows.Length; i++)
        {
            if (RM.moveArrows[i] == activeArrow.parent.gameObject)
            {
                RM.transform.position += changeFinal;
                return;
            }
        }
    }
    void Arrow_Stretch(RoomUpdater RM, Vector3 changeFinal)
    {
        RM.MoveFurniture(changeFinal);
        if (RM.upArrow == activeArrow.parent.gameObject)
        {
            float change = changeFinal.y;
            foreach (var item in RM.walls)
            {
                item.height += change;
            }
            RM.height += change;
            return;
        }
        if (RM.downArrow == activeArrow.parent.gameObject)
        {
            float change = changeFinal.y;
            foreach (var item in RM.walls)
            {
                item.height -= change;
            }
            RM.height -= change;
            RM.transform.position += changeFinal;
            return;
        }
        foreach (var item in RM.walls)
        {
            if (item.arrow == activeArrow.parent.gameObject)
            {
                RM.transform.position += changeFinal / 2;
                for (int i = 0; i < RM.vertPos.Length; i++)
                {
                    if (i == item.verts.x || i == item.verts.y)
                        RM.vertPos[i] += changeFinal / 2;
                    else
                        RM.vertPos[i] -= changeFinal / 2;
                }
                return;
            }
        }
    }
    void Arrow_Extend(RoomUpdater RM, Vector3 changeFinal)
    {
        if (RM.upArrow == activeArrow.parent.gameObject ||
            RM.downArrow == activeArrow.parent.gameObject)
        {
            float change = changeFinal.y;
            _totalChange += change;
            Vector3 _start = activeWall.mf.mesh.vertices[0] + activeWall.transform.position;
            Vector3 _end = activeWall.mf.mesh.vertices[2] + activeWall.transform.position;

            RoomUpdater ru = activeSquare.GetComponent<RoomUpdater>();
            ru.height = Mathf.Abs(_totalChange);
            if (_totalChange < 0)
            {
                _start += Vector3.up * _totalChange;
                _end += Vector3.up * _totalChange;
            }
            foreach (var item in ru.walls)
                item.height = Mathf.Abs(_totalChange);
            SetPosScale(_start, _end);
            return;
        }
        foreach (var item in RM.walls)
        {
            if (item.arrow == activeArrow.parent.gameObject)
            {
                float change = changeFinal.z + changeFinal.x;
                _totalChange += change;
                Vector3 _start = activeWall.mf.mesh.vertices[0] + activeWall.transform.position;
                Vector3 _end = activeWall.mf.mesh.vertices[3] + activeWall.transform.position;

                Vector3 _dir = new Vector3(
                    Mathf.Abs(activeWall.mf.mesh.normals[3].x),
                    Mathf.Abs(activeWall.mf.mesh.normals[3].y),
                    Mathf.Abs(activeWall.mf.mesh.normals[3].z));
                _end += _totalChange * _dir;
                SetPosScale(_start, _end);
                return;
            }
        }
    }
    void DeleteSquare()
    {
        if (activeSquare != null)
            GameObject.Destroy(activeSquare.gameObject);
        else if (activeWall != null)
            Destroy(activeWall.RU.gameObject);

        activeSquare = null;
        activeWall = null;
    }
    void DeselectSquare()
    {
        if (activeSquare != null)
        {
            activeSquare.GetComponent<RoomUpdater>().HideArrows();
        }
        activeSquare = null;
        DeselectWall();
    }
    void DeselectWall()
    {
        if (activeWall != null)
        {
            activeWall.OffHover();
            activeWall.RU.HideArrows();
        }
        activeWall = null;
    }
    void SelectSquare(Transform _newSquare)
    {
        if (activeSquare == _newSquare)
            return;
        DeselectSquare();
        activeSquare = _newSquare;
    }
    #endregion

    #region Edit Mesh
    private void SetPosScale(Vector3 start, Vector3 end)
    {
        activeSquare.position = (start + end) / 2 + new Vector3(0, 0.01f, 0);

        RoomUpdater ru = activeSquare.GetComponent<RoomUpdater>();

        Vector3 highs = start;
        Vector3 lows = start;
        if (start.x > end.x)
        {
            highs.x = end.x;
            lows.x = start.x;
        }
        else
        {
            highs.x = start.x;
            lows.x = end.x;
        }
        if (start.z > end.z)
        {
            highs.z = end.z;
            lows.z = start.z;
        }
        else
        {
            highs.z = start.z;
            lows.z = end.z;
        }
        ru.vertPos = new[]
        {
            lows + new Vector3(0, 0.01f, 0) - activeSquare.position,
            new Vector3(lows.x, start.y, highs.z) + new Vector3(0, 0.01f, 0) - activeSquare.position,
            highs + new Vector3(0, 0.01f, 0) - activeSquare.position,
            new Vector3(highs.x, start.y, lows.z) + new Vector3(0, 0.01f, 0) - activeSquare.position
        };

        ru.UpdateMeshes();
    }
    public void AddWalls(RoomUpdater ru, float _height = 3f)
    {
        for (int i = 0; i < 4; i++)
        {
            GameObject GO = Instantiate(squarePrefab, activeSquare);
            GO.tag = "Wall";
            RoomUpdater.wall wall = new RoomUpdater.wall();
            SurfaceUpdater SU = GO.AddComponent<SurfaceUpdater>();
            SU.Setup(ru, SurfaceUpdater.enumType.wall);
            wall.transform = GO.transform;
            wall.SU = SU;
            wall.height = _height;
            
            switch (i)
            {
                case 0:
                    wall.verts = new Vector2Int(0, 1);
                    GO.name = "West Wall a";
                    break;
                case 1:
                    wall.verts = new Vector2Int(1, 2);
                    GO.name = "South Wall a";
                    break;
                case 2:
                    wall.verts = new Vector2Int(2, 3);
                    GO.name = "East Wall a";
                    break;
                case 3:
                    wall.verts = new Vector2Int(3, 0);
                    GO.name = "North Wall a";
                    break;
                default:
                    break;
            }
            ru.walls.Add(wall);
        }

    }

    public void AddWallSingle(RoomUpdater ru, float _height = 3f)
    {
        GameObject GO = Instantiate(squarePrefab, activeSquare);
        GO.tag = "Wall";
        RoomUpdater.wall wall = new RoomUpdater.wall();
        SurfaceUpdater SU = GO.AddComponent<SurfaceUpdater>();
        SU.Setup(ru, SurfaceUpdater.enumType.wall);
        wall.SU = SU;
        wall.transform = GO.transform;
        wall.height = _height;

        wall.verts = new Vector2Int(ru.vertPos.Length - 2, ru.vertPos.Length - 1);
        GO.name = "Wall " + (ru.vertPos.Length - 1).ToString() + " a";
        ru.walls.Add(wall);
    }
    #endregion

    #region Tool Belt
    void Setup()
    {
        if (lg_block == null)
            lg_block = Instantiate(PF_blockTemplate, transform);

        _Build.Setup(this);
        _Paint.Setup(this);
        _Place.Setup(this);

        UpdateCursor_Image(null);
    }
    public void GenerateBelt_Sub(int _belt)
    {
        ClearBelt();
        
        DeselectSquare();
        switch (_belt)
        {
            case 0:
                _Build.GenerateBelt(this, _canvas.RT_toolBeltSub);
                break;
            case 1:
                _Paint.GenerateBelt(this, _canvas.RT_toolBeltSub);
                break;
            case 2:
                _Place.GenerateBelt(this, _canvas.RT_toolBeltSub);
                break;
            default:
                break;
        }
    }

    public void GenerateBelt_Full(BeltClass _belt, int _subInt)
    {
        SubClass _sub = _belt.GetList()[_subInt];
        GenerateBelt_Full(_belt, _sub);
    }
    public void BeltButtonTap_Sub(BeltClass _belt, SubClass _sub)
    {
        _canvas.toolBeltSub_Buttons[_belt.i_lastSel].Selected(false);
        GenerateBelt_Full(_belt, _sub);

        _sub.OnSelect(this);
    }
    public void BeltButtonTap_Full(SubClass _sub, SubItem _item)
    {
        _canvas.toolBeltFull_Buttons[_sub.i_lastSel].Selected(false);
        _sub.i_lastSel = _item.index;
        _item.OnSelect(this);
    }
    void GenerateBelt_Full(BeltClass _belt, SubClass _sub)
    {
        ClearBeltFull();
        _belt.i_lastSel = _sub.index;
        _sub.GenerateBelt_Full(this, _canvas.RT_toolBeltFull);
    }

    public void ClearBelt()
    {
        foreach (var item in _canvas.toolBeltSub_Buttons)
            Destroy(item.gameObject);
        _canvas.toolBeltSub_Buttons = new ToolBeltButton[0];

        _Place.lastTransform = null;
        Place_ActiveInfo();
        ClearBeltFull();
    }
    public void ClearBeltFull()
    {
        foreach (var item in _canvas.toolBeltFull_Buttons)
            Destroy(item.gameObject);
        _canvas.toolBeltFull_Buttons = new ToolBeltButton[0];
    }
    #endregion

    #region Grid Move
    void FocusGrid(Transform pos)
    {
        grid.position = pos.position;
        grid.LookAt(Camera.main.transform);
        grid.up = grid.forward;
    }
    void UnfocusGrid()
    {
        grid.rotation = new Quaternion();
        grid.position = new Vector3(0, height, 0);
    }
    #endregion
    #region Get Methods
    private SurfaceUpdater GetWall()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        SurfaceUpdater SU;
        if (Physics.Raycast(ray, out hit, 1000, _layer.interactableMask))
        {
            if (hit.transform.TryGetComponent<SurfaceUpdater>(out SU))
                return SU;
            /*
            Vector3Int vecInt = new Vector3Int(Mathf.RoundToInt(hit.point.x / (gridSize / 100)), Mathf.RoundToInt(hit.point.y / (gridSize / 100)), Mathf.RoundToInt(hit.point.z / (gridSize / 100)));
            Vector3 tempVec = vecInt;
            */
        }
        return null;
    }

    private Vector3 GetPos()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 1000, _layer.gridMask))
        {
            //Vector3Int vecInt = new Vector3Int(Mathf.RoundToInt(hit.point.x / gridSize), 0, Mathf.RoundToInt(hit.point.z / gridSize));
            //Vector3 tempVec = vecInt;
            Vector3 tempVec = new Vector3Int(Mathf.RoundToInt(hit.point.x / gridSize), Mathf.RoundToInt(hit.point.y / gridSize), Mathf.RoundToInt(hit.point.z / gridSize));
            tempVec *= gridSize;
            //tempVec = new Vector3(tempVec.x, tempVec.y, tempVec.z);
            return tempVec;
        }
        return new Vector3(0, 40400, 0);
    }

    Vector3 ClampToGrid(Vector3 _pos)
    {
        return ClampToGrid(_pos, gridSize);
    }
    Vector3 ClampToGrid(Vector3 _pos, float _size)
    {
        _pos = Vector3Int.RoundToInt(_pos / _size);
        _pos *= _size;
        return _pos;
    }

    private Vector3 GetRoomPos()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 1000))
        {
            Vector3Int vecInt = new Vector3Int(Mathf.RoundToInt(hit.point.x / gridSize), 0, Mathf.RoundToInt(hit.point.z / gridSize));
            Vector3 tempVec = vecInt;
            tempVec *= gridSize;
            tempVec = new Vector3(tempVec.x, height / 100, tempVec.z);
            return tempVec;
        }
        return new Vector3(0, 40400, 0);
    }
    #endregion
    #region Clamp Math
    public static Vector3 ClampPoint(Vector3 point, Vector3 segmentStart, Vector3 segmentEnd)
    {
        return ClampProjection(ProjectPoint(point, segmentStart, segmentEnd), segmentStart, segmentEnd);
    }

    public static Vector3 ProjectPoint(Vector3 point, Vector3 segmentStart, Vector3 segmentEnd)
    {
        return segmentStart + Vector3.Project(point - segmentStart, segmentEnd - segmentStart);
    }

    private static Vector3 ClampProjection(Vector3 point, Vector3 start, Vector3 end)
    {
        var toStart = (point - start).sqrMagnitude;
        var toEnd = (point - end).sqrMagnitude;
        var segment = (start - end).sqrMagnitude;
        if (toStart > segment || toEnd > segment) return toStart > toEnd ? end : start;
        return point;
    }
    #endregion

    public void Save()
    {
        if (lg_block == null)
            return;
        planeVisual.SetActive(false);
        string fileName = _canvas.TM_inputField.text;
        if (fileName == "")
            fileName = "New";
#if UNITY_EDITOR
        lg_block._name = fileName;
        lg_block.name = fileName;
        lg_block.UpdateLists_Editor();
        SavePrefab();
#endif
        planeVisual.SetActive(true);
    }
#if UNITY_EDITOR
    public void SavePrefab()
    {
        string root;
        string fileName = _canvas.TM_inputField.text;
        if (fileName == "")
            fileName = "New";
        string filePath;

        string[] initialPath = S_root.Split('/');
        root = initialPath[0];
        for (int i = 1; i < initialPath.Length; i++)
        {
            if (!Directory.Exists(root + "/" + initialPath[i]))
            {
                AssetDatabase.CreateFolder(root, initialPath[i]);
                Debug.Log(initialPath[i]);
            }
            root += "/" + initialPath[i];
        }
        if (!Directory.Exists(S_root + "/" + S_themeName))
        {
            AssetDatabase.CreateFolder(S_root, S_themeName);
            Debug.Log(S_root + "/" + S_themeName);
        }

        root = S_root + "/" + S_themeName;

        GameObject _object = lg_block.gameObject;
        filePath = root + "/" + fileName + ".prefab";
        bool prefabSuccess;
        GameObject GO = PrefabUtility.SaveAsPrefabAsset(_object, filePath, out prefabSuccess);
        if (prefabSuccess == true)
            Debug.Log("Prefab was saved successfully");
        else
        {
            Debug.Log("Prefab failed to save" + prefabSuccess);
            return;
        }
        GO.transform.localPosition = Vector3.zero;
        GO.transform.localEulerAngles = Vector3.zero;
        /*
        foreach (Transform t in lg_block.T_architecture)
        {
            SurfaceUpdater[] SU = t.GetComponentsInChildren<SurfaceUpdater>();
            foreach (SurfaceUpdater su in SU)
            {
                AssetDatabase.AddObjectToAsset(su.mf.sharedMesh, GO);
                switch (su._enum)
                {
                    case SurfaceUpdater.enumType.wall:
                        AssetDatabase.AddObjectToAsset(su.skirting.MF.sharedMesh, GO);
                        AssetDatabase.AddObjectToAsset(su.cornice.MF.sharedMesh, GO);
                        break;
                    default:
                        break;
                }
            }
        }
        */

        Mesh m;
        for (int t = 0; t < lg_block.T_architecture.Count; t++)
        {
            SurfaceUpdater[] SU = lg_block.T_architecture[t].GetComponentsInChildren<SurfaceUpdater>();
            SurfaceUpdater[] SU2 = GO.GetComponent<LevelGen_Block>().T_architecture[t].GetComponentsInChildren<SurfaceUpdater>();
            for (int s = 0; s < SU.Length; s++)
            {
                m = SU[s].mf.sharedMesh;
                AssetDatabase.AddObjectToAsset(m, GO);
                SU2[s].mf.sharedMesh = m;
                SU2[s].mc.sharedMesh = m;
                switch (SU[s]._enum)
                {
                    case SurfaceUpdater.enumType.wall:
                        m = SU[s].skirting.MF.sharedMesh;
                        AssetDatabase.AddObjectToAsset(m, GO);
                        SU2[s].skirting.MF.sharedMesh = m;
                        m = SU[s].cornice.MF.sharedMesh;
                        AssetDatabase.AddObjectToAsset(m, GO);
                        SU2[s].cornice.MF.sharedMesh = m;
                        break;
                    default:
                        break;
                }
            }
        }
        PrefabUtility.SavePrefabAsset(GO);
    }
#endif
}
