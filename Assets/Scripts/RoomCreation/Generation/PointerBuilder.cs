using NUnit.Framework;
using NUnit.Framework.Internal;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PointerBuilder : MonoBehaviour
{
    public LevelGen_Materials _Materials;
    public LevelGen_Placeables _Placeables;
    public enum drawModes { square, point, wall, arrow }
    public enum beltModes { build, paint, place}
    [HideInInspector] public drawModes drawMode;
    [HideInInspector] public beltModes beltMode;


    [HideInInspector]
    public Transform activeSquare;
    [HideInInspector]
    public SurfaceUpdater activeWall;
    [HideInInspector]
    public Transform activeArrow;
    public GameObject squarePrefab;

    public Transform floorHolder;

    [HideInInspector]
    public float gridSize = 1;

    [HideInInspector]
    public float height = 0;

    public Transform grid;

    private Vector3 firstClickPos;

    public GameObject arrow;


    public TextMeshProUGUI drawModeText;
    
    public Architraves architraves;
    public layerClass _layer;
    [System.Serializable]
    public class layerClass
    {
        public LayerMask gridMask;
        public LayerMask interactableMask;
        public LayerMask arrowMask;
    }

    public CanvasClass _canvas;
    [System.Serializable]
    public class CanvasClass
    {
        public Button[] toolBelt_Buttons;
        [HideInInspector] public ToolBeltButton[] toolBeltSub_Buttons;
        [HideInInspector] public ToolBeltButton[] toolBeltFull_Buttons;

        public ToolBeltButton PF_toolBeltSub;
        public ToolBeltButton PF_toolBeltFull;

        public RectTransform RT_toolBeltSub;
        public RectTransform RT_toolBeltFull;
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
        public GameObject activePrefab;
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
            SC.items[SC.i_lastSel].OnSelect(PB);
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

        }
    }
    public class BuildItem : SubItem
    {
        public override void OnSelect(PointerBuilder PB)
        {

        }
    }
    public class PaintItem : SubItem
    {
        public Material _mat;
        public override void OnSelect(PointerBuilder PB)
        {
            PB._Paint.activeMat = _mat;
        }
    }
    public class PlaceItem : SubItem
    {
        public GameObject _prefab;
        public override void OnSelect(PointerBuilder PB)
        {
            PB._Place.activePrefab = _prefab;
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

    void Update_Build ()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, 1000, _layer.arrowMask))
                {
                    activeArrow = hit.transform;
                    activeArrow.localScale = Vector3.one * 1.1f;
                    firstClickPos = GetPos();
                    drawMode = drawModes.arrow;
                    FocusGrid(activeArrow);
                    return;
                }
                if (Physics.Raycast(ray, out hit, 1000))
                {
                    int l = hit.transform.gameObject.layer;
                    if (_layer.gridMask.Check(l))
                    {
                        GameObject GO;
                        RoomUpdater RU;
                        SurfaceUpdater SU;
                        switch (drawMode)
                        {
                            case drawModes.square:
                                if (activeWall != null)
                                    activeWall.GetComponentInParent<RoomUpdater>().HideArrows();
                                firstClickPos = GetPos();
                                GO = Instantiate(squarePrefab, floorHolder);

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
                                SU.Setup(RU);
                                RU.roomName = GO.name;
                                RU.SU_Floor = SU;

                                SU.mf.mesh = tempMesh;
                                SU.mc.sharedMesh = tempMesh;
                                RU.floor = GO.transform;
                                RU.arrow = arrow;

                                RU.height = 3;

                                RU.architraves = architraves;

                                //CEILING//////////
                                GO = Instantiate(squarePrefab, activeSquare);
                                SU = GO.AddComponent<SurfaceUpdater>();
                                SU.Setup(RU);
                                RU.SU_Ceiling = SU;

                                SU.mf.mesh = tempMesh;
                                SU.mc.sharedMesh = tempMesh;
                                AddWalls(RU);
                                break;
                            case drawModes.point:
                                Vector3 clickPos = GetPos();
                                if (activeWall != null)
                                    activeWall.GetComponentInParent<RoomUpdater>().HideArrows();
                                if (activeSquare == null)
                                {
                                    GO = Instantiate(squarePrefab, floorHolder);
                                    GO.name = Time.time.ToString();
                                    Mesh meshTempPoint = new Mesh();
                                    //Mesh meshTempPoint2 = GO1.GetComponent<MeshFilter>().mesh;
                                    //meshTempPoint.vertices = meshTempPoint2.vertices;
                                    //meshTempPoint.uv = meshTempPoint2.uv;
                                    //meshTempPoint.triangles = meshTempPoint2.triangles;
                                    activeSquare = GO.transform;
                                    RU = GO.AddComponent<RoomUpdater>();
                                    SU = GO.AddComponent<SurfaceUpdater>();
                                    SU.Setup(RU);
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
                                break;
                            default:
                                break;
                        }
                    }
                    else if (_layer.interactableMask.Check(l))
                    {
                        drawMode = drawModes.wall;
                        SurfaceUpdater activeWallTemp = GetWall();
                        if (activeWallTemp != activeWall && activeWall != null)
                            activeWall.GetComponentInParent<RoomUpdater>().HideArrows();
                        activeWall = activeWallTemp;
                        RoomUpdater RU = activeWall.GetComponentInParent<RoomUpdater>();
                        activeSquare = RU.transform;
                        RU.ShowArrows();
                    }
                }
            }
        }
        switch (drawMode)
        {
            case drawModes.square:
                if (activeWall != null)
                    activeWall.GetComponentInParent<RoomUpdater>().HideArrows();
                drawModeText.text = "Draw Mode";
                if (Input.GetMouseButton(0) && activeSquare != null)
                {
                    SetPosScale(firstClickPos, GetPos());
                }
                if (Input.GetMouseButtonUp(0) && activeSquare != null)
                {
                    if (firstClickPos == GetPos())
                        GameObject.Destroy(activeSquare.gameObject);
                    activeSquare = null;
                }
                break;
            case drawModes.point:
                {
                    if (activeWall != null)
                        activeWall.GetComponentInParent<RoomUpdater>().HideArrows();
                    drawModeText.text = "Point Mode";
                    break;
                }
            case drawModes.wall:
                drawModeText.text = "Edit Mode";
                if (Input.GetKeyDown(KeyCode.Delete))
                    DeleteSquare();
                break;
            case drawModes.arrow:
                drawModeText.text = "Edit Mode";
                if (activeArrow != null)
                {
                    if (Input.GetMouseButton(0))
                    {
                        RoomUpdater RM = activeWall.GetComponentInParent<RoomUpdater>();
                        Vector3 temp = GetPos();
                        //temp = new Vector3(temp.x, firstClickPos.y, temp.z);
                        Vector3 changeFinal = temp - firstClickPos;
                        changeFinal = ClampPoint(changeFinal, activeArrow.up * -1, activeArrow.up * 1);
                        ArrowMove(RM, changeFinal);

                        RM.UpdateMeshes();
                        firstClickPos = temp;
                    }
                    if (Input.GetMouseButtonUp(0))
                    {
                        activeWall.GetComponentInParent<RoomUpdater>().ShowArrows();
                        activeArrow.localScale = Vector3.one;
                    }
                    if (Input.GetKeyDown(KeyCode.Delete))
                        DeleteSquare();
                }
                break;
            default:
                break;
        }
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
                        if (Input.GetMouseButtonDown(0))
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

    }
    #region Player Interactions
    void ArrowMove(RoomUpdater RM, Vector3 changeFinal)
    {
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
        for (int i = 0; i < RM.moveArrows.Length; i++)
        {
            if (RM.moveArrows[i] == activeArrow.parent.gameObject)
            {
                RM.transform.position += changeFinal;
                return;
            }
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
    void DeleteSquare()
    {
        if (activeSquare != null)
        {
            GameObject.Destroy(activeSquare.gameObject);
        }
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
            activeWall.OffHover();
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
    private void DoorPlaceWallFix(Vector3[] points, MeshFilter mf, float heightFix)
    {
        Vector3 min = points[0];
        Vector3 max = points[0];

        Vector2 height = new Vector2(min.y, min.y);

        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].x != min.x || points[i].z != min.z)
            {
                max = points[i];
            }
            if (points[i].y > height.y)
                height.y = points[i].y;
        }

        min = new Vector3(min.x, height.x, min.z);
        max = new Vector3(max.x, height.y, max.z);

        Vector3[] tempVerts = new Vector3[]
        {
            min,
            new Vector3(min.x, min.y + heightFix, min.z),
            new Vector3(max.x, min.y + heightFix, max.z),
            new Vector3(max.x, min.y, max.z)

        };


        Vector2[] temp2D = new Vector2[4];

        for (int i = 0; i < 4; i++)
        {
            temp2D[i] = new Vector2(tempVerts[i].x + tempVerts[i].z, tempVerts[i].y);
        }



        Triangulator tr = new Triangulator(temp2D);
        int[] indices = tr.Triangulate();


        int[] finalIndices = new int[indices.Length];

        for (int i = 0; i < finalIndices.Length; i++)
        {
            finalIndices[i] = indices[indices.Length - 1 - i];
        }

        mf.mesh.vertices = tempVerts;
        mf.mesh.triangles = indices;
        mf.mesh.uv = temp2D;

        mf.mesh.RecalculateNormals();
        mf.mesh.RecalculateBounds();

        if (tempVerts[0].x + tempVerts[0].z > tempVerts[2].x + tempVerts[2].z)
            mf.mesh.triangles = finalIndices;

        mf.GetComponent<MeshCollider>().sharedMesh = mf.mesh;

    }
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
    public void AddWalls(RoomUpdater ru)
    {
        for (int i = 0; i < 4; i++)
        {
            GameObject GO = Instantiate(squarePrefab, activeSquare);
            GO.tag = "Wall";
            RoomUpdater.wall wall = new RoomUpdater.wall();
            SurfaceUpdater SU = GO.AddComponent<SurfaceUpdater>();
            SU.Setup(ru);
            wall.transform = GO.transform;
            wall.SU = SU;
            wall.height = 3;
            
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

    public void AddWallSingle(RoomUpdater ru)
    {
        GameObject GO = Instantiate(squarePrefab, activeSquare);
        GO.tag = "Wall";
        RoomUpdater.wall wall = new RoomUpdater.wall();
        SurfaceUpdater SU = GO.AddComponent<SurfaceUpdater>();
        SU.Setup(ru);
        wall.SU = SU;
        wall.transform = GO.transform;
        wall.height = 3;

        wall.verts = new Vector2Int(ru.vertPos.Length - 2, ru.vertPos.Length - 1);
        GO.name = "Wall " + (ru.vertPos.Length - 1).ToString() + " a";
        ru.walls.Add(wall);
    }
    #endregion

    #region Tool Belt
    void Setup()
    {
        _Build.Setup(this);
        _Paint.Setup(this);
        _Place.Setup(this);
    }
    void UpdateItemsList()
    {
        _Build.UpdateItemsList();
        _Paint.UpdateItemsList();
        _Place.UpdateItemsList();
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
}
