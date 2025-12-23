using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Layout Defined", menuName = "Trench/Layout/Defined")]
public class Layout_Defined : ScriptableObject
{
    public LevelGen_Theme _theme;
    public Layout_Bounds _bounds;
    public List<objectClass> _objects = new List<objectClass>();
    [HideInInspector] public effectClass _effect;
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
        _effect = new effectClass(this);
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

    [System.Serializable]
    public class effectClass
    {
        public int _respawnTotal = 0;
        public int _respawnAmt = 0;

        public effectClass(Layout_Defined _layout)
        {
            if (_layout.GetEffect(out float f, LevelGen_Block.effectTypeEnum.respawn))
            {
                _respawnTotal = Mathf.RoundToInt(f);
                _respawnAmt = _respawnTotal;
            }
            else
            {
                _respawnTotal = 0;
                _respawnAmt = 0;
            }
        }
        public bool Respawn()
        {
            if (_respawnAmt <= 0)
                return false;

            _respawnAmt--;
            PlayerManager.main.Ref.HUI_health.UpdateRespawns();

            return true;
        }
    }
    public bool GetEffect(out float f, LevelGen_Block.effectTypeEnum _effect)
    {
        f = 0;
        bool _valid = false;
        foreach (var _obj in _objects)
        {
            if (_obj._block.GetEffectAmt(out float _temp, _effect))
            {
                f += _temp;
                _valid = true;
            }
        }
        return _valid;
    }
}
