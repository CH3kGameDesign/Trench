using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Layout Defined", menuName = "Trench/Layout/Defined")]
public class Layout_Defined : ScriptableObject
{
    public LevelGen_Theme _theme;
    public Layout_Bounds _bounds;
    public List<objectClass> _objects = new List<objectClass>();

    public Layout_Defined()
    {
        _theme = null;
        _bounds = null;
        _objects = new List<objectClass>();
    }
    public Layout_Defined(Layout_Defined _temp)
    {
        _theme = _temp._theme;
        _bounds = _temp._bounds;
        _objects = new List<objectClass>();
        foreach (var item in _temp._objects)
            _objects.Add(new objectClass(item));
    }
    public Layout_Defined(LevelGen_Theme theme, Layout_Bounds bounds, List<objectClass> objects)
    {
        _theme = theme;
        _bounds = bounds;
        _objects = new List<objectClass>();
        foreach (var item in objects)
            _objects.Add(new objectClass(item));
    }
    [System.Serializable]
    public class objectClass
    {
        public LevelGen_Block _block;
        public Vector2Int _pos;
        public int _rot = 0;
        public bool _locked = false;
        public objectClass()
        {
            _block = null;
            _pos = Vector2Int.zero;
            _rot = 0;
            _locked = false;
        }
        public objectClass(objectClass _temp)
        {
            _block = _temp._block;
            _pos = _temp._pos;
            _rot = _temp._rot;
            _locked = _temp._locked;
        }
        public objectClass(LevelGen_Block block, Vector2Int pos, int rot, bool locked)
        {
            _block = block;
            _pos = pos;
            _rot = rot;
            _locked = locked;
        }
    }
}
