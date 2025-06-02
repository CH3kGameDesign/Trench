using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public static class CKUtil
{
    public static string ToString_Currency(this int _value)
    {
        string _temp = "$" + _value.ToString();
        return _temp;
    }

    public static string ToString_Distance(this float _value)
    {
        string _temp;
        if (_value > 600)
        {
            _temp = (Mathf.Floor(_value / 10) / 100).ToString();
            _temp += "km";
        }
        else if (_value > 60)
        {
            _temp = (Mathf.Floor(_value / 10) * 10).ToString();
            _temp += "m";
        }
        else
        {
            _temp = (Mathf.Floor(_value)).ToString();
            _temp += "m";
        }
        return _temp;
    }

    public static string ToString_Input(this string _interactable, string _input, Interactable.enumType _type = Interactable.enumType.interact)
    {

        string _temp = Interactable.interactText[(int)_type];
        _temp = _temp.Replace("[0]", _input);
        _temp = _temp.Replace("[1]", _interactable);
        return _temp;
    }

    public static bool CheckEntry(this LevelGen_Block.entryTypeEnum _entry, LevelGen_Block.entryTypeEnum _exit)
    {
        switch (_entry)
        {
            case LevelGen_Block.entryTypeEnum.shipDoor:
                return _exit == LevelGen_Block.entryTypeEnum.shipPark;
            case LevelGen_Block.entryTypeEnum.shipPark:
                return _exit == LevelGen_Block.entryTypeEnum.shipDoor;
            case LevelGen_Block.entryTypeEnum.any:
                return true;
            default:
                return _entry == _exit;
        }
    }

    public static bool isSelfHittable(this HitObject.damageTypeEnum _damageType)
    {
        switch (_damageType)
        {
            case HitObject.damageTypeEnum.all:
                return true;
            case HitObject.damageTypeEnum.bullet:
                return false;
            case HitObject.damageTypeEnum.fire:
                return true;
            case HitObject.damageTypeEnum.explosive:
                return true;
            default:
                return true;
        }
    }
    public static LevelGen_Block.blockTypeEnum ConvertToLevelGen(this Layout_Basic.roomEnum _roomType)
    {
        switch (_roomType)
        {
            case Layout_Basic.roomEnum.bridge:
                return LevelGen_Block.blockTypeEnum.bridge;
            case Layout_Basic.roomEnum.hangar:
                return LevelGen_Block.blockTypeEnum.hangar;
            case Layout_Basic.roomEnum.corridor:
                return LevelGen_Block.blockTypeEnum.corridor;


            case Layout_Basic.roomEnum.deadEnd:
                return LevelGen_Block.blockTypeEnum.deadend;
            case Layout_Basic.roomEnum.engine:
                return LevelGen_Block.blockTypeEnum.engine;
            case Layout_Basic.roomEnum.foodHall:
                return LevelGen_Block.blockTypeEnum.foodhall;
            case Layout_Basic.roomEnum.crewQuarters:
                return LevelGen_Block.blockTypeEnum.crewQuarters;
            case Layout_Basic.roomEnum.captain:
                return LevelGen_Block.blockTypeEnum.captain;
            case Layout_Basic.roomEnum.vault:
                return LevelGen_Block.blockTypeEnum.vault;

            default:
                return LevelGen_Block.blockTypeEnum.corridor;
        }
    }
    public static LevelGen_Block.entryTypeEnum ConvertToLevelGen(this Layout_Basic.entryTypeEnum _entryType)
    {
        switch (_entryType)
        {
            case Layout_Basic.entryTypeEnum.any:
                return LevelGen_Block.entryTypeEnum.any;
            case Layout_Basic.entryTypeEnum.singleDoor:
                return LevelGen_Block.entryTypeEnum.singleDoor;
            case Layout_Basic.entryTypeEnum.wideDoor:
                return LevelGen_Block.entryTypeEnum.wideDoor;
            case Layout_Basic.entryTypeEnum.vent:
                return LevelGen_Block.entryTypeEnum.vent;
            case Layout_Basic.entryTypeEnum.shipDoor:
                return LevelGen_Block.entryTypeEnum.shipDoor;
            case Layout_Basic.entryTypeEnum.shipPark:
                return LevelGen_Block.entryTypeEnum.shipPark;
            default:
                return LevelGen_Block.entryTypeEnum.any;
        }
    }

    public static bool CompareEntries(this LevelGen_Block.entryTypeEnum _entry1, LevelGen_Block.entryTypeEnum _entry2)
    {
        switch (_entry1)
        {
            case LevelGen_Block.entryTypeEnum.any:
                return true;
            default:
                if (_entry2 == LevelGen_Block.entryTypeEnum.any)
                    return true;
                else
                    return _entry1 == _entry2;
        }
    }

    public static void Shuffle<T>(this IList<T> ts)
    {
        ts.Shuffle(new Unity.Mathematics.Random());
    }
    public static void Shuffle<T>(this IList<T> ts, Unity.Mathematics.Random Random_Seeded)
    {
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i)
        {
            var r = Random_Seeded.NextInt(count);
            var tmp = ts[i];
            ts[i] = ts[r];
            ts[r] = tmp;
        }
    }
    public static T GetRandom<T>(this IList<T> ts)
    {
        if (ts.Count == 0)
        {
            Debug.LogWarning("List Empty");
            return default(T);
        }
        return ts.GetRandom(new Unity.Mathematics.Random(0x6E624EB7u));
    }
    public static T GetRandom<T>(this IList<T> ts, Unity.Mathematics.Random Random_Seeded)
    {
        int _ran = Random_Seeded.NextInt(0, ts.Count);
        return ts[_ran];
    }

    public static T[] Reverse<T>(this T[] ts)
    {
        T[] _temp = new T[ts.Length];
        int j = ts.Length - 1;
        for (int i = 0; i < ts.Length; i++)
        {
            _temp[i] = ts[j];
            j--;
        }
        return _temp;
    }

    public static void PlayClip(this Animator _anim, string _name)
    {
        _anim.Play(_name);
    }

    public static void FollowObject(this RectTransform _holder, Canvas C_canvas, Transform _target)
    {
        Vector3 _tarPos = Camera.main.WorldToScreenPoint(_target.position);

        _tarPos /= C_canvas.scaleFactor;

        Vector2 _size = _holder.sizeDelta / 2;
        Vector2 _xBounds = new Vector2(_size.x, (Screen.width / C_canvas.scaleFactor) - _size.x);
        Vector2 _yBounds = new Vector2(_size.y, (Screen.height / C_canvas.scaleFactor) - _size.y);

        _tarPos.x = Mathf.Clamp(_tarPos.x, _xBounds.x, _xBounds.y);
        _tarPos.y = Mathf.Clamp(_tarPos.y, _yBounds.x, _yBounds.y);


        if(_tarPos.z < 0)
        {
            Vector2 _checker = _tarPos;
            _checker.x = 1 - Mathf.Abs(_checker.x / (Screen.width / 2) - 1);
            _checker.y = 1 - Mathf.Abs(_checker.y / (Screen.height / 2) - 1);
            if (_checker.x >= _checker.y)
            {
                if (_tarPos.x > Screen.width / 2) _tarPos.x = _xBounds.y;
                else _tarPos.x = _xBounds.x;
            }
            else
            {
                if (_tarPos.y > Screen.height / 2) _tarPos.y = _yBounds.y;
                else _tarPos.y = _yBounds.x;
            }
        }

        _holder.anchoredPosition = _tarPos;
    }
    public static void DeleteChildren (this Transform _transform, bool _immediate = false)
    {
        for (int i = _transform.childCount - 1; i >= 0; i--)
        {
            if (_immediate) GameObject.DestroyImmediate(_transform.GetChild(i).gameObject);
            else GameObject.Destroy(_transform.GetChild(i).gameObject);
        }
    }
}
