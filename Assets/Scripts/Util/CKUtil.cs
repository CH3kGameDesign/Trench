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
}
