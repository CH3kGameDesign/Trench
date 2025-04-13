using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Trench/AssetLists/Gun/Rod", fileName = "New Rod Gun")]
public class GunClass_Rod : GunClass
{
    public float F_floatDrag = 8f;
    private List<Bullet> firedRods = new List<Bullet>();

    public override GunClass Clone()
    {
        GunClass_Rod _temp = CreateInstance<GunClass_Rod>();
        base.Clone(_temp);
        _temp.F_floatDrag = F_floatDrag;
        return _temp;
    }
    public override void OnUpdate()
    {
        if (b_aiming)
            PM_player.RB_player.linearDamping = F_floatDrag;
        else
            PM_player.RB_player.linearDamping = 0;

        base.OnUpdate();
    }
    public override void OnUnEquip()
    {
        Detonate();
        PM_player.RB_player.linearDamping = 0;
        base.OnUnEquip();
    }
    public override void OnBullet(Bullet _bullet)
    {
        firedRods.Add(_bullet);
        base.OnBullet(_bullet);
    }
    public override void OnReload()
    {
        Detonate();
        base.OnReload();
    }
    void Detonate()
    {
        foreach (var item in firedRods)
        {
            if (item != null)
                item.Detonate();
        }
        firedRods.Clear();
    }
}
