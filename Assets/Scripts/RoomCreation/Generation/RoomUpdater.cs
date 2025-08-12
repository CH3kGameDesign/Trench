using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class RoomUpdater : MonoBehaviour
{
    public string roomName;
    public Transform floor;
    public SurfaceUpdater SU_Floor;
    public SurfaceUpdater SU_Ceiling;

    public GameObject arrow;

    public Architraves architraves;

    private List<GameObject> measurements = new List<GameObject>();

    public Vector3[] vertPos = new Vector3[]
        {
            Vector3.zero,
            new Vector3(1,0,0),
            new Vector3(0,0,1),
            new Vector3(1,0,1)
        };
    public List<wall> walls = new List<wall>();
    public class wall
    {
        public Transform transform;
        public SurfaceUpdater SU;
        public float height;
        public Vector2Int verts;
        public GameObject arrow;
    }

    public GameObject upArrow;
    public GameObject downArrow;
    public GameObject[] moveArrows = new GameObject[0];

    public float height = 1;

    public List<GameObject> cornices = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void MoveFurniture(Vector3 _dif)
    {
        SU_Floor.MoveFurniture(_dif);
        SU_Ceiling.MoveFurniture(_dif);
        for (int i = 0; i < walls.Count; i++)
            walls[i].SU.MoveFurniture(_dif);
    }

    public void UpdateMeshes()
    {
        UpdateFloor();
        UpdateCeiling();

        for (int i = 0; i < walls.Count; i++)
            UpdateWall(walls[i]);
        //UpdateArchitraves();
    }
    
    public void UpdateFloor()
    {
        SU_Floor.mf.mesh.vertices = vertPos;
        Vector2[] temp = new Vector2[vertPos.Length];
        for (int i = 0; i < temp.Length; i++)
            temp[i] = new Vector2(vertPos[i].x, vertPos[i].z);
        SU_Floor.mf.mesh.uv = temp;
        SU_Floor.mf.mesh.RecalculateBounds();
        if (SU_Floor.mf.mesh.bounds.IsValid())
            SU_Floor.mc.sharedMesh = SU_Floor.mf.mesh;
        //boxCollider.size = new Vector3(Mathf.Abs(vertPos[0].x - vertPos[3].x), 0.01f, Mathf.Abs(vertPos[0].z - vertPos[3].z));
    }

    public void UpdateCeiling()
    {
        SU_Ceiling.mf.transform.localPosition = Vector3.up * height;
        SU_Ceiling.mf.mesh.vertices = vertPos.Reverse();

        Vector2[] temp = new Vector2[SU_Ceiling.mf.mesh.vertices.Length];
        for (int i = 0; i < temp.Length; i++)
            temp[i] = new Vector2(SU_Ceiling.mf.mesh.vertices[i].x, SU_Ceiling.mf.mesh.vertices[i].z);
        SU_Ceiling.mf.mesh.uv = temp;
        SU_Ceiling.mf.mesh.RecalculateNormals();
        SU_Ceiling.mf.mesh.RecalculateBounds();
        if (SU_Ceiling.mf.mesh.bounds.IsValid())
            SU_Ceiling.mc.sharedMesh = SU_Ceiling.mf.mesh;
        //boxCollider.size = new Vector3(Mathf.Abs(vertPos[0].x - vertPos[3].x), 0.01f, Mathf.Abs(vertPos[0].z - vertPos[3].z));
    }

    public void ShowArrows(PointerBuilder.drawModes _mode)
    {
        HideArrows();
        switch (_mode)
        {
            case PointerBuilder.drawModes.stretch:
                StretchArrows();
                break;
            case PointerBuilder.drawModes.move:
                MoveArrows();
                break;
            case PointerBuilder.drawModes.extend:
                StretchArrows();
                break;
            default:
                break;
        }
    }

    void StretchArrows()
    {
        GameObject GO;
        //Wall Arrow
        foreach (var item in walls)
        {
            GO = Instantiate(arrow, item.transform.position, item.transform.rotation, item.transform);
            GO.transform.LookAt(item.transform.position + new Vector3(item.SU.mf.mesh.vertices[0].x, 0, item.SU.mf.mesh.vertices[0].z));
            GO.transform.localEulerAngles += new Vector3(0, -90, 0);
            item.arrow = GO;
        }
        //Up Arrow
        Vector3 _height = new Vector3(0, height, 0);
        GO = Instantiate(arrow, SU_Floor.mf.transform.position + _height, SU_Floor.mf.transform.rotation, SU_Floor.mf.transform);
        GO.name = "UpArrow";
        GO.transform.localEulerAngles = new Vector3(90, 0, 0);
        upArrow = GO;
        //Down Arrow
        GO = Instantiate(arrow, SU_Floor.mf.transform.position, SU_Floor.mf.transform.rotation, SU_Floor.mf.transform);
        GO.name = "DownArrow";
        GO.transform.localEulerAngles = new Vector3(-90, 0, 0);
        downArrow = GO;
    }
    void MoveArrows()
    {
        GameObject GO;
        Vector3 _height = new Vector3(0, height, 0);
        moveArrows = new GameObject[3];
        string[] names = new string[] { "MoveUpArrow", "MoveForwardArrow", "MoveRightArrow" };
        Vector3[] dir = new Vector3[] { -Vector3.up, -Vector3.forward, -Vector3.right };
        Color[] color = new Color[] { Color.green, Color.blue, Color.red };
        for (int i = 0; i < 3; i++)
        {
            GO = Instantiate(arrow, SU_Floor.mf.transform.position + (_height) / 2, SU_Floor.mf.transform.rotation, SU_Floor.mf.transform);
            GO.transform.localScale = Vector3.one * 0.5f;
            GO.name = names[i];
            GO.transform.forward = dir[i];
            GO.transform.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_LitColor", color[i]);

            moveArrows[i] = GO;
        }
    }

    public void UpdateArchitraves()
    {
        foreach (var item in cornices)
            GameObject.Destroy(item);
        cornices = new List<GameObject>();


        UpdateSkirting();
        UpdateCornices();
    }

    

    public void UpdateSkirting()
    {
        List<MeshFilter> walls = new List<MeshFilter>();
        foreach (var item in GetComponentsInChildren<MeshFilter>())
        {
            if (item.gameObject.name[item.gameObject.name.Length - 1] == 'a')
                walls.Add(item);
        }
        for (int j = 0; j < walls.Count; j++)
        {
            SurfaceUpdater SU = walls[j].GetComponent<SurfaceUpdater>();
            SU.skirting = null;
            int k = j + 1;
            if (k >= GetComponent<MeshFilter>().mesh.vertices.Length)
                k = 0;

            GameObject wallActive = new GameObject("Skirting:" + j.ToString());
            wallActive.transform.position = walls[j].mesh.vertices[0] + walls[j].transform.position;
            wallActive.transform.parent = transform;
            wallActive.transform.LookAt(walls[j].mesh.vertices[1] + walls[j].transform.position);
            wallActive.transform.localEulerAngles -= new Vector3(0, 90, 0);
            MeshFilter MF = wallActive.AddComponent<MeshFilter>();
            MeshRenderer MR = wallActive.AddComponent<MeshRenderer>();
            cornices.Add(wallActive);

            Vector3[] tempVerts = new Vector3[architraves.skirting[0].positionCount * 2];
            for (int i = 0; i < architraves.skirting[0].positionCount; i++)
            {
                tempVerts[i] = architraves.skirting[0].GetPosition(i);
            }

            float dist = Vector2.Distance(new Vector2(walls[j].mesh.vertices[0].x, walls[j].mesh.vertices[0].z), new Vector2(walls[j].mesh.vertices[3].x, walls[j].mesh.vertices[3].z));
            for (int i = 0; i < architraves.skirting[0].positionCount; i++)
            {
                tempVerts[i + architraves.skirting[0].positionCount] = architraves.skirting[0].GetPosition(i) + (Vector3.right * dist);
            }
            Vector2[] tempUV = new Vector2[architraves.skirting[0].positionCount * 2];
            for (int i = 0; i < tempUV.Length; i++)
            {
                Vector3 temp = tempVerts[i];
                tempUV[i] = new Vector2(temp.x + temp.z, temp.y);
            }

            int[] tempTris = new int[(architraves.skirting[0].positionCount - 1) * 6];

            for (int i = 0; i < architraves.skirting[0].positionCount - 1; i++)
            {
                tempTris[i * 6] = i;
                tempTris[(i * 6) + 1] = i + 1;
                tempTris[(i * 6) + 2] = architraves.skirting[0].positionCount + i;

                //Debug.Log(i + "," + (i + 1) + "," + (cornice.GetComponent<LineRenderer>().positionCount + i));

                tempTris[(i * 6) + 3] = i + 1;
                tempTris[(i * 6) + 4] = architraves.skirting[0].positionCount + i + 1;
                tempTris[(i * 6) + 5] = architraves.skirting[0].positionCount + i;
                //Debug.Log((i + 1) + "," + (cornice.GetComponent<LineRenderer>().positionCount + i + 1) + "," + (cornice.GetComponent<LineRenderer>().positionCount + i));
            }
            MF.mesh.vertices = tempVerts;
            MF.mesh.uv = tempUV;
            MF.mesh.triangles = tempTris;

            MR.material = walls[j].GetComponent<MeshRenderer>().material;
            //SU.skirting = MR;
            MF.mesh.RecalculateNormals();
            MF.mesh.RecalculateBounds();

            //wallActive.meshCollider.sharedMesh = wallActive.mf.mesh;
            //wallActive.boxCollider.size = new Vector3(Mathf.Abs(vertPos[wallActive.verts.x].x - vertPos[wallActive.verts.y].x), wallActive.height, Mathf.Abs(vertPos[wallActive.verts.x].z - vertPos[wallActive.verts.y].z));
        }
    }

    public void UpdateCornices()
    {
        //SU_Ceiling.architraves = new List<MeshRenderer>();
        for (int j = 0; j < GetComponent<MeshFilter>().mesh.vertices.Length; j++)
        {
            int k = j + 1;
            if (k >= GetComponent<MeshFilter>().mesh.vertices.Length)
                k = 0;

            GameObject wallActive = new GameObject("Cornice:" + j.ToString());
            wallActive.transform.position = GetComponent<MeshFilter>().mesh.vertices[j] + transform.position;
            wallActive.transform.parent = transform;
            wallActive.transform.LookAt(GetComponent<MeshFilter>().mesh.vertices[k] + transform.position);
            wallActive.transform.localEulerAngles -= new Vector3(0, 90, 0);
            MeshFilter MF = wallActive.AddComponent<MeshFilter>();
            MeshRenderer MR = wallActive.AddComponent<MeshRenderer>();

            cornices.Add(wallActive);
            //SU_Ceiling.architraves.Add(MR);
            Vector3[] tempVerts = new Vector3[architraves.cornices[0].positionCount * 2];
            for (int i = 0; i < architraves.cornices[0].positionCount; i++)
            {
                tempVerts[i] = architraves.cornices[0].GetPosition(i) + new Vector3(0, walls[j].height,0);
            }

            float dist = Vector2.Distance(new Vector2(GetComponent<MeshFilter>().mesh.vertices[j].x, GetComponent<MeshFilter>().mesh.vertices[j].z), new Vector2(GetComponent<MeshFilter>().mesh.vertices[k].x, GetComponent<MeshFilter>().mesh.vertices[k].z));
            for (int i = 0; i < architraves.cornices[0].positionCount; i++)
            {
                tempVerts[i + architraves.cornices[0].positionCount] = architraves.cornices[0].GetPosition(i) + (Vector3.right * dist) + new Vector3(0, walls[j].height, 0);
            }
            Vector2[] tempUV = new Vector2[architraves.cornices[0].positionCount * 2];
            for (int i = 0; i < tempUV.Length; i++)
            {
                Vector3 temp = tempVerts[i];
                tempUV[i] = new Vector2(temp.x + temp.z, temp.y);
            }

            int[] tempTris = new int[(architraves.cornices[0].positionCount - 1) * 6];

            for (int i = 0; i < architraves.cornices[0].positionCount - 1; i++)
            {
                tempTris[i * 6] = i;
                tempTris[(i * 6) + 1] = i + 1;
                tempTris[(i * 6) + 2] = architraves.cornices[0].positionCount + i;

                //Debug.Log(i + "," + (i + 1) + "," + (cornice.GetComponent<LineRenderer>().positionCount + i));

                tempTris[(i * 6) + 3] = i + 1;
                tempTris[(i * 6) + 4] = architraves.cornices[0].positionCount + i + 1;
                tempTris[(i * 6) + 5] = architraves.cornices[0].positionCount + i;
                //Debug.Log((i + 1) + "," + (cornice.GetComponent<LineRenderer>().positionCount + i + 1) + "," + (cornice.GetComponent<LineRenderer>().positionCount + i));
            }
            MF.mesh.vertices = tempVerts;
            MF.mesh.uv = tempUV;
            MF.mesh.triangles = tempTris;

            MR.material = SU_Ceiling.mr.material;

            MF.mesh.RecalculateNormals();
            MF.mesh.RecalculateBounds();

            //wallActive.meshCollider.sharedMesh = wallActive.mf.mesh;
            //wallActive.boxCollider.size = new Vector3(Mathf.Abs(vertPos[wallActive.verts.x].x - vertPos[wallActive.verts.y].x), wallActive.height, Mathf.Abs(vertPos[wallActive.verts.x].z - vertPos[wallActive.verts.y].z));
        }
    }

    public void HideArrows()
    {
        //boxCollider.enabled = true;
        foreach (var item in walls)
        {
            //item.boxCollider.enabled = true;
            if (item.arrow != null)
            {
                GameObject.Destroy(item.arrow);
                item.arrow = null;
            }
        }
        if (upArrow != null)
        {
            GameObject.Destroy(upArrow);
            upArrow = null;
        }
        if (downArrow != null)
        {
            GameObject.Destroy(downArrow);
            downArrow = null;
        }
        foreach (var item in moveArrows)
            GameObject.Destroy(item);
        moveArrows = new GameObject[0];
    }

    public void UpdateWall(wall wallActive)
    {
        wallActive.SU.UpdateWall(vertPos, wallActive);
    }

    public void UpdateSurface(SurfaceUpdater SU)
    {
        foreach (var item in walls)
        {
            if (SU == item.SU)
            {
                UpdateWall(item);
                return;
            }
        }
        if (SU_Floor == SU)
        {
            UpdateFloor();
            return;
        }
        if (SU_Ceiling == SU)
        {
            UpdateCeiling();
            return;
        }
    }
}
