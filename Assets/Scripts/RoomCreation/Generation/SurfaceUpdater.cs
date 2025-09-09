using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;

public class SurfaceUpdater : MonoBehaviour
{
    public enumType _enum;
    public enum enumType { wall, floor, ceiling};

    public MeshFilter mf;
    public MeshRenderer mr;
    public MeshCollider mc;
    public List<BoxCollider> bc = new List<BoxCollider>();

    public RoomUpdater RU;
    [HideInInspector] public architrave skirting = null;
    [HideInInspector] public architrave cornice = null;

    [System.Serializable]
    public class architrave
    {
        public Transform T;
        public MeshRenderer MR;
        public MeshFilter MF;

        public architrave(Transform transform, MeshRenderer mr, MeshFilter mf)
        {
            this.T = transform;
            this.MR = mr;
            this.MF = mf;
        }
    }

    public List<LevelGen_Door> doors = new List<LevelGen_Door>();

    public List<Prefab_Environment> PE_furniture = new List<Prefab_Environment>();

    private Material mat;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        
    }

    public void Setup(RoomUpdater _RU, enumType _type)
    {
        RU = _RU;
        _enum = _type;
        SetLayer(_type);

        mf = GetComponent<MeshFilter>();
        mr = GetComponent<MeshRenderer>();
        mc = GetComponent<MeshCollider>();
        bc = GetComponents<BoxCollider>().ToList();

        mat = mr.sharedMaterial;
    }

    void SetLayer(enumType _type)
    {
        switch (_type)
        {
            case enumType.wall:
                gameObject.layer = 19;
                break;
            case enumType.floor:
                gameObject.layer = 14;
                break;
            case enumType.ceiling:
                gameObject.layer = 20;
                break;
            default:
                break;
        }
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
        mr.sharedMaterial = _material;
        switch (_enum)
        {
            case enumType.wall:
                if (skirting != null)
                    skirting.MR.sharedMaterial = _material;
                break;
            case enumType.ceiling:
                foreach (RoomUpdater.wall w in RU.walls)
                    w.SU.UpdateMaterial_Cornice(_material);
                break;
            default:
                break;
        }
    }
    public void UpdateMaterial_Cornice(Material _material)
    {
        if (cornice != null)
            cornice.MR.sharedMaterial = _material;
    }

    public void UpdateSurface()
    {
        RU.UpdateSurface(this);
    }

    public void UpdateWall(Vector3[] vertPos, RoomUpdater.wall wallActive)
    {
        Vector3 low = vertPos[wallActive.verts.x];
        Vector3 high = vertPos[wallActive.verts.y];

        wallActive.transform.localPosition = ((low + high) / 2) + (new Vector3(0, wallActive.height / 2, 0));

        List<Vector3> _vert = new List<Vector3>()
        {
            low - wallActive.transform.localPosition,
            high - wallActive.transform.localPosition,
            low + new Vector3(0, wallActive.height, 0) - wallActive.transform.localPosition,
            high + new Vector3(0, wallActive.height, 0) - wallActive.transform.localPosition
        };
        float dist = Vector3.Distance(low, high);
        List<Vector2> _uv = new List<Vector2>()
        {
            Vector2.zero,
            new Vector2(dist, 0),
            new Vector2(0, wallActive.height),
            new Vector2(dist, wallActive.height)
        };

        List<int> _tri = new List<int>();
        int _i = 4;
        Vector2Int _start = new Vector2Int(2, 0);
        List<Vector3> BC_points = new List<Vector3>() { _vert[0] };
        foreach (var item in doors)
        {
            holeClass _hole = holeClass.CreateNew(item, low - wallActive.transform.localPosition, wallActive.height);
            _vert.AddRange(_hole._points);
            _uv.AddRange(_hole._uv);
            _tri.AddRange(_hole.GetTriangles(_i, _start, out _i, out _start));
            BC_points.Add(_hole._points[4]);
            BC_points.Add(_hole._points[2]);
            BC_points.Add(_hole._points[5]);
            BC_points.Add(_hole._points[1]);
        }
        _tri.AddRange(new int[]
        {
                _start.x, 3, 1,
                _start.x, 1, _start.y,
        });
        BC_points.Add(_vert[3]);


        mf.mesh.triangles = new int[0];
        mf.mesh.vertices = _vert.ToArray();
        mf.mesh.uv = _uv.ToArray();

        mf.mesh.triangles = _tri.ToArray();
        //Triangulator _temp = new Triangulator(_uv.ToArray());
        //mf.mesh.triangles = _temp.Triangulate();

        mf.mesh.RecalculateNormals();
        mf.mesh.RecalculateBounds();
        if (wallActive.SU.mf.mesh.bounds.IsValid())
            mc.sharedMesh = wallActive.SU.mf.mesh;
        UpdateBoxColliders(BC_points);

        UpdateSkirting();
        UpdateCornice();
    }

    void UpdateBoxColliders(List<Vector3> BC_points)
    {
        int tarAmt = BC_points.Count / 2;
        for (int i = bc.Count - 1; i >= tarAmt; i--)
        {
            Destroy(bc[i]);
            bc.RemoveAt(i);
        }

        for (int i = 0; i < tarAmt; i ++)
        {
            int j = i * 2;
            BoxCollider _bc;
            if (i >= bc.Count)
            {
                _bc = this.AddComponent<BoxCollider>();
                bc.Add(_bc);
            }
            else
                _bc = bc[i];
            Vector3 _dif = BC_points[j + 1] - BC_points[j];
            _bc.size = new Vector3(Mathf.Abs(_dif.x), Mathf.Abs(_dif.y), Mathf.Abs(_dif.z));
            _bc.center = BC_points[j] + (_dif / 2);
        }
    }

    void UpdateSkirting()
    {
        if (skirting == null)
        {
            GameObject GO = new GameObject("Skirting: " + name);
            GO.transform.parent = transform.parent;
            MeshFilter _MF = GO.AddComponent<MeshFilter>();
            MeshRenderer _MR = GO.AddComponent<MeshRenderer>();
            skirting = new architrave(GO.transform, _MR, _MF);
        }
        skirting.T.position = mf.mesh.vertices[0] + transform.position;
        skirting.T.LookAt(mf.mesh.vertices[1] + transform.position);
        skirting.T.localEulerAngles -= new Vector3(0, 90, 0);

        LineRenderer LR = RU.architraves.skirting[0];
        int pCount = LR.positionCount;
        Vector3[] tempVerts = new Vector3[pCount * 2 * (1 + doors.Count)];
        int ii = 0;
        for (int i = 0; i < pCount; i++)
        {
            tempVerts[ii] = LR.GetPosition(i);
            ii++;
        }
        float _dist;
        for (int d = 0; d < doors.Count; d++)
        {
            _dist = Vector3.Distance(doors[d].transform.position, mf.mesh.vertices[0] + transform.position) - (doors[d].GetSize().x / 2);

            for (int i = 0; i < pCount; i++)
            {
                tempVerts[ii] = LR.GetPosition(i) + Vector3.right * _dist;
                ii++;
            }
            _dist += doors[d].GetSize().x;
            for (int i = 0; i < pCount; i++)
            {
                tempVerts[ii] = LR.GetPosition(i) + Vector3.right * _dist;
                ii++;
            }
        }
        _dist = Vector3.Distance(mf.mesh.vertices[0], mf.mesh.vertices[1]);
        for (int i = 0; i < pCount; i++)
        {
            tempVerts[ii] = LR.GetPosition(i) + Vector3.right * _dist;
            ii++;
        }
        Vector2[] tempUV = new Vector2[tempVerts.Length];
        for (int i = 0; i < tempUV.Length; i++)
        {
            Vector3 temp = tempVerts[i];
            tempUV[i] = new Vector2(temp.x + temp.z, temp.y);
        }

        int[] tempTris = new int[tempVerts.Length * 6];

        for (int j = 0; j <= doors.Count; j++)
        {
            for (int i = 0; i < pCount - 1; i++)
            {
                int k = i + (j * 2 * pCount);
                tempTris[k * 6] = k;
                tempTris[(k * 6) + 1] = k + 1;
                tempTris[(k * 6) + 2] = pCount + k;

                tempTris[(k * 6) + 3] = k + 1;
                tempTris[(k * 6) + 4] = pCount + k + 1;
                tempTris[(k * 6) + 5] = pCount + k;
            }
        }

        skirting.MF.mesh.triangles = new int[0];
        skirting.MF.mesh.vertices = tempVerts;
        skirting.MF.mesh.uv = tempUV;
        skirting.MF.mesh.triangles = tempTris;

        skirting.MR.sharedMaterial = mr.sharedMaterial;
        skirting.MF.mesh.RecalculateNormals();
        skirting.MF.mesh.RecalculateBounds();
    }
    void UpdateCornice()
    {
        if (cornice == null)
        {
            GameObject GO = new GameObject("Cornice: " + name);
            GO.transform.parent = transform.parent;
            MeshFilter _MF = GO.AddComponent<MeshFilter>();
            MeshRenderer _MR = GO.AddComponent<MeshRenderer>();
            cornice = new architrave(GO.transform, _MR, _MF);
        }
        cornice.T.position = mf.mesh.vertices[2] + transform.position;
        cornice.T.LookAt(mf.mesh.vertices[3] + transform.position);
        cornice.T.localEulerAngles -= new Vector3(0, 90, 0);

        LineRenderer LR = RU.architraves.cornices[0];
        int pCount = LR.positionCount;

        Vector3[] tempVerts = new Vector3[pCount * 2 * (1 + doors.Count)];
        int ii = 0;
        for (int i = 0; i < pCount; i++)
        {
            tempVerts[ii] = LR.GetPosition(i);
            ii++;
        }
        float _dist = Vector3.Distance(mf.mesh.vertices[0], mf.mesh.vertices[1]);
        for (int i = 0; i < pCount; i++)
        {
            tempVerts[ii] = LR.GetPosition(i) + Vector3.right * _dist;
            ii++;
        }
        Vector2[] tempUV = new Vector2[tempVerts.Length];
        for (int i = 0; i < tempUV.Length; i++)
        {
            Vector3 temp = tempVerts[i];
            tempUV[i] = new Vector2(temp.x + temp.z, temp.y);
        }

        int[] tempTris = new int[tempVerts.Length * 6];

        for (int i = 0; i < pCount - 1; i++)
        {
            tempTris[i * 6] = i;
            tempTris[(i * 6) + 1] = i + 1;
            tempTris[(i * 6) + 2] = pCount + i;

            tempTris[(i * 6) + 3] = i + 1;
            tempTris[(i * 6) + 4] = pCount + i + 1;
            tempTris[(i * 6) + 5] = pCount + i;
        }

        cornice.MF.mesh.triangles = new int[0];
        cornice.MF.mesh.vertices = tempVerts;
        cornice.MF.mesh.uv = tempUV;
        cornice.MF.mesh.triangles = tempTris;

        cornice.MR.sharedMaterial = RU.SU_Ceiling.mr.sharedMaterial;
        cornice.MF.mesh.RecalculateNormals();
        cornice.MF.mesh.RecalculateBounds();
    }

    public void MoveFurniture(Vector3 _dif)
    {
        if (PE_furniture.Count == 0)
            return;
        switch (_enum)
        {
            case enumType.wall:
                _dif = PE_furniture[0].transform.rotation * _dif;
                _dif.z = 0;
                _dif = Quaternion.Inverse(PE_furniture[0].transform.rotation) * _dif / 2;
                break;
            default:
                _dif /= 2;
                _dif.y = 0;
                break;
        }
        foreach (var p in PE_furniture)
        {
            if (p == null) continue;
            p.transform.localPosition -= _dif;
            switch (p._type)
            {
                case Prefab_Environment.TypeEnum.door:
                    Vector3 _h = p.transform.localPosition;
                    _h.y = mf.mesh.vertices[0].y;
                    p.transform.localPosition = _h;
                    break;
                default:
                    break;
            }
        }
    }

    public void AddFurniture(Prefab_Environment _prefab)
    {
        if (PE_furniture.Contains(_prefab))
            return;
        _prefab.SU_surface = this;
        PE_furniture.Add(_prefab);
        if (_prefab._type == Prefab_Environment.TypeEnum.door)
        {
            LevelGen_Door _door;
            if (_prefab.TryGetComponent<LevelGen_Door>(out _door))
            {
                _door.openDoor(true);
                AddHole(_door);
            }
        }
    }
    public void RemoveFurniture(Prefab_Environment _prefab)
    {
        if (!PE_furniture.Contains(_prefab))
            return;
        PE_furniture.Remove(_prefab);
        if (_prefab._type == Prefab_Environment.TypeEnum.door)
        {
            LevelGen_Door _door;
            if (_prefab.TryGetComponent<LevelGen_Door>(out _door))
            {
                RemoveHole(_door);
            }
        }
    }

    public void AddHole(LevelGen_Door _door)
    {
        if (!doors.Contains(_door))
        {
            doors.Add(_door);
            SortHoles();
        }
    }
    public void SortHoles()
    {
        if (doors.Count <= 1)
            return;
        doors = doors.OrderBy(_d =>
        Vector3.Distance(_d.transform.position, mf.mesh.vertices[0] + transform.position)).ToList();
    }
    public void RemoveHole(LevelGen_Door _door)
    {
        if (doors.Contains(_door))
        {
            doors.Remove(_door);
            UpdateSurface();
        }
    }

    public void ShowHide(bool _show, bool _includeArchitrave = true)
    {
        mr.enabled = _show;
        mc.enabled = _show;
        foreach (var item in bc)
            item.enabled = _show;

        if (_includeArchitrave)
        {
            switch (_enum)
            {
                case enumType.wall:
                    if (cornice != null)
                        cornice.MR.enabled = _show;
                    if (skirting != null)
                        skirting.MR.enabled = _show;
                    break;
                case enumType.floor:
                    foreach (RoomUpdater.wall w in RU.walls)
                        if (w.SU.skirting != null) w.SU.skirting.MR.enabled = _show;
                    break;
                case enumType.ceiling:
                    foreach (RoomUpdater.wall w in RU.walls)
                        if (w.SU.cornice != null) w.SU.cornice.MR.enabled = _show;
                    break;
                default:
                    break;
            }
        }
    }

    public class holeClass
    {
        public enum typeEnum { door}
        public typeEnum type;
        public Vector3[] _points = new Vector3[0];
        public Vector2[] _uv = new Vector2[0];

        public static holeClass CreateNew(LevelGen_Door _door, Vector3 _wallMin, float _wallHeight)
        {
            holeClass _temp = new holeClass();
            _temp.type = typeEnum.door;
            _temp.GeneratePoints(_door, _wallMin, _wallHeight);
            return _temp;
        }

        bool GeneratePoints(LevelGen_Door _door, Vector3 _wallMin, float _wallHeight)
        {
            Vector3 _point = _door.transform.position;
            Vector2 _size = _door.GetSize();
            if (_size == Vector2.zero)
                return false;
            Vector3 _0 = Vector3.MoveTowards(_door.transform.localPosition, _wallMin, _size.x/2);
            Vector3 _1 = Vector3.MoveTowards(_door.transform.localPosition, _wallMin, -_size.x/2);
            _points = new Vector3[]
            {
                _0, _1,
                _0 + Vector3.up * _size.y, _1 + Vector3.up * _size.y,
                _0 + Vector3.up * _wallHeight, _1 + Vector3.up * _wallHeight
            };
            float _dist = Vector3.Distance(_door.transform.localPosition, _wallMin);
            _uv = new Vector2[]
            {
                new Vector2(_dist - _size.x, 0), new Vector2(_dist + _size.x, 0),
                new Vector2(_dist - _size.x, _size.y), new Vector2(_dist + _size.x, _size.y),
                new Vector2(_dist - _size.x, _wallHeight), new Vector2(_dist + _size.x, _wallHeight)
            };

            return true;
        }

        public List<int> GetTriangles(int _i, Vector2Int _start, out int _endI, out Vector2Int _end)
        {
            List<int> tri = new List<int>();
            if (_start != Vector2Int.zero)
                switch (type)
                {
                    case typeEnum.door:
                        tri.AddRange(new int[]
                        {
                        _start.x, _i + 4, _i,
                        _start.x, _i, _start.y,
                        });
                        break;
                    default:
                        break;
                }
            switch (type)
            {
                case typeEnum.door:
                    tri.AddRange(new int[]
                    {
                        _i + 4, _i + 5, _i + 3,
                        _i + 4, _i + 3, _i + 2,
                    });
                    _end = new Vector2Int(_i + 5, _i + 1);
                    _endI = _i + 6;
                    break;
                default:
                    _end = Vector2Int.zero;
                    _endI = _i;
                    break;
            }
            return tri;
        }
    }
}
