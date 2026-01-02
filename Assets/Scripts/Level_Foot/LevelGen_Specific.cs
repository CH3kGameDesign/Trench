using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using static Consumable;
using static LevelGen;
using static Themes;

public class LevelGen_Specific : LevelGen
{
    public override void Setup(uint _seed, int _id, Vector3 _pos, bool _player = true, BoxCollider _BC = null)
    {
        id = _id;

        Random_Seeded = new Unity.Mathematics.Random(_seed);

        LG_Theme = themeHolder.GetTheme(SaveData.themeCurrent);
        MusicHandler.Instance.SetupPlaylist(LG_Theme.playlist);

        StartCoroutine(SetupLayout(_pos));
    }
    public override void Setup(uint _seed, int _id, Vector3 _pos, Layout_Defined _layoutDefined, bool _player = true, BoxCollider _BC = null)
    {
        Setup(_seed, _id, _pos, _player, _BC);
    }

    public IEnumerator SetupLayout(Vector3 _pos, spawnType _type = spawnType._default)
    {
        LG_Blocks = new List<LevelGen_Block>();
        T_Holder = transform;
        UpdatePosition(T_Holder, _pos);
        Physics.SyncTransforms();

        nm_Surfaces = T_Holder.GetComponents<NavMeshSurface>();
        yield return new WaitForEndOfFrame();

        foreach (var _temp in GetComponentsInChildren<LevelGen_Block>())
        {
            _temp.Setup(id, LG_Blocks.Count);
            LG_Blocks.Add(_temp);
            foreach (var bound in _temp.B_bounds)
                bound.B_Bounds.enabled = true;
        }
        yield return new WaitForEndOfFrame();

        SetDoors_Auto();
        UpdateNavMeshes();
        SetupVehicle(T_Holder, _type);
        SpawnObjects(_type);
        yield return new WaitForEndOfFrame();
        LevelGen_Holder.Instance.IsReady();
    }
}
