using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

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

    public List<LevelGen_Door> doors = new List<LevelGen_Door>();

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
        bc = GetComponent<BoxCollider>();

        mat = mr.material;
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
        mr.material = _material;
        foreach (MeshRenderer _mr in architraves)
            _mr.material = _material;
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
        foreach (var item in doors)
        {
            holeClass _hole = holeClass.CreateNew(item, low - wallActive.transform.localPosition, wallActive.height);
            _vert.AddRange(_hole._points);
            _uv.AddRange(_hole._uv);
            _tri.AddRange(_hole.GetTriangles(_i, _start, out _i, out _start));
        }
        _tri.AddRange(new int[]
        {
                _start.x, 3, 1,
                _start.x, 1, _start.y,
        });
        mf.mesh.vertices = _vert.ToArray();
        mf.mesh.uv = _uv.ToArray();

        mf.mesh.triangles = _tri.ToArray();
        //Triangulator _temp = new Triangulator(_uv.ToArray());
        //mf.mesh.triangles = _temp.Triangulate();

        mf.mesh.RecalculateNormals();
        mf.mesh.RecalculateBounds();
        if (wallActive.SU.mf.mesh.bounds.IsValid())
            mc.sharedMesh = wallActive.SU.mf.mesh;
        bc.size = new Vector3(Mathf.Abs(vertPos[wallActive.verts.x].x - vertPos[wallActive.verts.y].x), wallActive.height, Mathf.Abs(vertPos[wallActive.verts.x].z - vertPos[wallActive.verts.y].z));
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
            Vector2 _size;
            switch (_door.entryType)
            {
                case LevelGen_Block.entryTypeEnum.singleDoor:
                    _size = new Vector2(3f, 3f);
                    break;
                case LevelGen_Block.entryTypeEnum.wideDoor:
                    _size = new Vector2(8f, 3f);
                    break;
                case LevelGen_Block.entryTypeEnum.vent:
                    _size = new Vector2(1.5f, 2f);
                    break;
                default:
                    return false;
            }
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
