using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PointerBuilder : MonoBehaviour
{

    public enum drawModes { square, point, wall, arrow, place, placeNew, edit }
    public drawModes drawMode;

    private drawModes privateDrawMode = drawModes.square;

    public LayerMask gridMask;
    public LayerMask interactableMask;
    public LayerMask arrowMask;

    [HideInInspector]
    public Transform activeSquare;
    [HideInInspector]
    public Transform activeWall;
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


    // Start is called before the first frame update
    void Start()
    {
        gridSize = 1;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 1000, arrowMask))
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
                switch (hit.transform.gameObject.layer)
                {
                    case 6:
                        drawMode = privateDrawMode;

                        switch (drawMode)
                        {
                            case drawModes.square:
                                if (activeWall != null)
                                    activeWall.GetComponentInParent<RoomUpdater>().HideArrows();
                                firstClickPos = GetPos();
                                GameObject GO = Instantiate(squarePrefab, floorHolder);

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

                                GO.GetComponent<MeshFilter>().mesh = tempMesh;
                                GO.GetComponent<MeshCollider>().sharedMesh = tempMesh;

                                GO.name = Time.time.ToString();
                                //Mesh meshTemp = new Mesh();
                                //Mesh meshTemp2 = GO.GetComponent<MeshFilter>().mesh;
                                //meshTemp.vertices = meshTemp2.vertices;
                                //meshTemp.uv = meshTemp2.uv;
                                //meshTemp.triangles = meshTemp2.triangles;
                                activeSquare = GO.transform;
                                RoomUpdater RM = GO.AddComponent<RoomUpdater>();
                                RM.roomName = GO.name;
                                RM.mf = GO.GetComponent<MeshFilter>();
                                RM.floor = GO.transform;
                                RM.arrow = arrow;

                                RM.height = 3;

                                RM.architraves = architraves;

                                RM.boxCollider = GO.GetComponent<BoxCollider>();
                                RM.meshCollider = GO.GetComponent<MeshCollider>();

                                GameObject GO2 = Instantiate(squarePrefab, activeSquare);
                                RM.mf_Ceiling = GO2.GetComponent<MeshFilter>();
                                RM.meshCollider_Ceiling = GO2.GetComponent<MeshCollider>();

                                GO2.GetComponent<MeshFilter>().mesh = tempMesh;
                                GO2.GetComponent<MeshCollider>().sharedMesh = tempMesh;
                                AddWalls(GO.GetComponent<RoomUpdater>());
                                break;
                            case drawModes.point:
                                Vector3 clickPos = GetPos();
                                if (activeWall != null)
                                    activeWall.GetComponentInParent<RoomUpdater>().HideArrows();
                                if (activeSquare == null)
                                {
                                    GameObject GO1 = Instantiate(squarePrefab, floorHolder);
                                    GO1.name = Time.time.ToString();
                                    Mesh meshTempPoint = new Mesh();
                                    //Mesh meshTempPoint2 = GO1.GetComponent<MeshFilter>().mesh;
                                    //meshTempPoint.vertices = meshTempPoint2.vertices;
                                    //meshTempPoint.uv = meshTempPoint2.uv;
                                    //meshTempPoint.triangles = meshTempPoint2.triangles;
                                    activeSquare = GO1.transform;
                                    GO1.AddComponent<RoomUpdater>();
                                    GO1.GetComponent<MeshFilter>().mesh = meshTempPoint;
                                    GO1.GetComponent<RoomUpdater>().roomName = GO1.name;
                                    GO1.GetComponent<RoomUpdater>().mf = GO1.GetComponent<MeshFilter>();
                                    GO1.GetComponent<RoomUpdater>().floor = GO1.transform;
                                    GO1.GetComponent<RoomUpdater>().arrow = arrow;

                                    GO1.GetComponent<RoomUpdater>().architraves = architraves;

                                    GO1.GetComponent<RoomUpdater>().boxCollider = GO1.GetComponent<BoxCollider>();
                                    GO1.GetComponent<RoomUpdater>().meshCollider = GO1.GetComponent<MeshCollider>();
                                    GO1.GetComponent<RoomUpdater>().vertPos = new Vector3[0];
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

                                    ru.GetComponent<MeshFilter>().mesh.vertices = tempVerts;
                                    ru.GetComponent<MeshFilter>().mesh.triangles = indices;
                                    ru.GetComponent<MeshFilter>().mesh.uv = temp2D;

                                    ru.GetComponent<MeshFilter>().mesh.RecalculateNormals();
                                    ru.GetComponent<MeshFilter>().mesh.RecalculateBounds();

                                    ru.meshCollider.sharedMesh = ru.mf.mesh;
                                }
                                break;
                            default:
                                break;
                        }

                        break;
                    case 14:
                        drawMode = drawModes.wall;
                        Transform activeWallTemp = GetWall();
                        if (activeWallTemp != activeWall && activeWall != null)
                            activeWall.GetComponentInParent<RoomUpdater>().HideArrows();
                        activeWall = activeWallTemp;
                        activeWall.GetComponentInParent<RoomUpdater>().ShowArrows();
                        break;
                    default:
                        break;
                }
            }
        }
        switch (drawMode)
        {
            case drawModes.square:
                if (activeWall != null)
                    activeWall.GetComponentInParent<RoomUpdater>().HideArrows();
                drawModeText.text = "DRAW MODE: Square";
                if (Input.GetMouseButton(0) && activeSquare != null)
                {
                    SetPosScale(firstClickPos, GetPos());
                }
                if (Input.GetMouseButtonUp(0) && activeSquare != null)
                {
                    //if (activeSquare.localScale.x < 0.1f || activeSquare.localScale.y < 0.1f)
                    //    GameObject.Destroy(activeSquare.gameObject);
                    if (firstClickPos == GetPos())
                        GameObject.Destroy(activeSquare.gameObject);
                    activeSquare = null;
                }
                break;
            case drawModes.point:
                {
                    if (activeWall != null)
                        activeWall.GetComponentInParent<RoomUpdater>().HideArrows();
                    drawModeText.text = "DRAW MODE: Point";
                    break;
                }
            case drawModes.wall:
                drawModeText.text = "DRAW MODE: Edit";
                break;
            case drawModes.arrow:
                drawModeText.text = "DRAW MODE: Arrow";
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
                }
                break;
            default:
                break;
        }
        if (Input.GetMouseButtonUp(0))
            UnfocusGrid();
    }

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

    private Transform GetWall()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 1000, interactableMask))
        {
            Vector3Int vecInt = new Vector3Int(Mathf.RoundToInt(hit.point.x / (gridSize / 100)), Mathf.RoundToInt(hit.point.y / (gridSize / 100)), Mathf.RoundToInt(hit.point.z / (gridSize / 100)));
            Vector3 tempVec = vecInt;
            return hit.transform;
        }
        return this.transform;
    }

    private Vector3 GetPos()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit,1000, gridMask))
        {
            //Vector3Int vecInt = new Vector3Int(Mathf.RoundToInt(hit.point.x / gridSize), 0, Mathf.RoundToInt(hit.point.z / gridSize));
            //Vector3 tempVec = vecInt;
            Vector3 tempVec = new Vector3Int(Mathf.RoundToInt(hit.point.x / gridSize), Mathf.RoundToInt(hit.point.y / gridSize), Mathf.RoundToInt(hit.point.z / gridSize));
            tempVec *= gridSize;
            //tempVec = new Vector3(tempVec.x, tempVec.y, tempVec.z);
            return tempVec;
        }
        return new Vector3(0,40400,0);
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
            tempVec = new Vector3(tempVec.x, height/100, tempVec.z);
            return tempVec;
        }
        return new Vector3(0, 40400, 0);
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
            wall.transform = GO.transform;
            wall.mf = GO.GetComponent<MeshFilter>();
            wall.height = 3;
            wall.boxCollider = GO.GetComponent<BoxCollider>();
            wall.meshCollider = GO.GetComponent<MeshCollider>();
            
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
        wall.transform = GO.transform;
        wall.mf = GO.GetComponent<MeshFilter>();
        wall.height = 3;
        wall.boxCollider = GO.GetComponent<BoxCollider>();
        wall.meshCollider = GO.GetComponent<MeshCollider>();

        wall.verts = new Vector2Int(ru.vertPos.Length - 2, ru.vertPos.Length - 1);
        GO.name = "Wall " + (ru.vertPos.Length - 1).ToString() + " a";
        ru.walls.Add(wall);
    }

    public void ChangeDrawMode (int temp)
    {
        if (activeSquare != null)
        {
            activeSquare.GetComponent<RoomUpdater>().HideArrows();
            //GameObject.Destroy(activeSquare.gameObject);
            activeSquare = null;
        }
        switch (temp)
        {
            case 0:
                privateDrawMode = drawModes.square;
                break;
            case 1:
                privateDrawMode = drawModes.point;
                break;
            case 2:
                privateDrawMode = drawModes.placeNew;
                break;
            case 3:
                privateDrawMode = drawModes.wall;
                break;
            default:
                break;
        }
        drawMode = privateDrawMode;
    }
}
