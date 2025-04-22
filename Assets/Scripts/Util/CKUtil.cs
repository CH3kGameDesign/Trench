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

    public static string ToString_Input(this string _interactable, string _input)
    {
        string _temp = "Press ";
        _temp += _input;
        _temp += " to interact with ";
        _temp += _interactable;
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
}
