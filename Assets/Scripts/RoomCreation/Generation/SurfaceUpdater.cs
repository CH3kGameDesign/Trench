using UnityEngine;
using System.Collections.Generic;

public class SurfaceUpdater : MonoBehaviour
{
    public enumType _enum;
    public enum enumType { wall, floor, ceiling};

    public MeshFilter mf;
    public MeshRenderer mr;
    public MeshCollider mc;
    public BoxCollider bc;

    public RoomUpdater RU;
    [HideInInspector] public List<MeshRenderer> architraves = new List<MeshRenderer>();

    private Material mat;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        
    }

    public void Setup(RoomUpdater _RU, enumType _type)
    {
        RU = _RU;
        _enum = _type;

        mf = GetComponent<MeshFilter>();
        mr = GetComponent<MeshRenderer>();
        mc = GetComponent<MeshCollider>();
        bc = GetComponent<BoxCollider>();

        mat = mr.material;
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void OffHover()
    {
        OffHover_Paint();
    }

    public void OnHover_Paint(Material _material)
    {
        UpdateMaterial(_material);
    }
    public void OffHover_Paint()
    {
        UpdateMaterial(mat);
    }
    public void OnClick_Paint(Material _material)
    {
        mat = _material;
    }

    void UpdateMaterial(Material _material)
    {
        mr.material = _material;
        foreach (MeshRenderer _mr in architraves)
            _mr.material = _material;
    }

    public void UpdateWall(Vector3[] vertPos, RoomUpdater.wall wallActive)
    {
        Vector3 low = vertPos[wallActive.verts.x];
        Vector3 high = vertPos[wallActive.verts.y];

        wallActive.transform.localPosition = ((low + high) / 2) + (new Vector3(0, wallActive.height / 2, 0));
        mf.mesh.vertices = new Vector3[]
        {
            low - wallActive.transform.localPosition,
            high - wallActive.transform.localPosition,
            new Vector3(low.x, low.y+ wallActive.height, low.z) -wallActive.transform.localPosition,
            high + new Vector3(0, wallActive.height, 0) - wallActive.transform.localPosition
        };
        float dist = Vector3.Distance(low, high);
        mf.mesh.uv = new Vector2[]
        {
            Vector2.zero,
            new Vector2(dist, 0),
            new Vector2(0, wallActive.height),
            new Vector2(dist, wallActive.height)
        };

        mf.mesh.RecalculateBounds();
        mc.sharedMesh = wallActive.SU.mf.mesh;
        bc.size = new Vector3(Mathf.Abs(vertPos[wallActive.verts.x].x - vertPos[wallActive.verts.y].x), wallActive.height, Mathf.Abs(vertPos[wallActive.verts.x].z - vertPos[wallActive.verts.y].z));
    }
}
