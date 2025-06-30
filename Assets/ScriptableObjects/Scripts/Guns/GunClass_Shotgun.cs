using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Trench/AssetLists/Gun/Shotgun", fileName = "New Shotgun Gun")]
public class GunClass_Shotgun : GunClass
{
    public fireClass fireVariablesOnAim;
    public fireClass fireVariablesOnSprint;

    public override GunClass Clone()
    {
        GunClass_Shotgun _temp = CreateInstance<GunClass_Shotgun>();
        _temp.fireVariablesOnAim = fireVariablesOnAim;
        _temp.fireVariablesOnSprint = fireVariablesOnSprint;
        base.Clone(_temp);
        return _temp;
    }
    public override void OnUpdate()
    {
        base.OnUpdate();
    }
    public override void OnUnEquip()
    {
        base.OnUnEquip();
    }
    public override void OnBullet(Bullet _bullet)
    {
        base.OnBullet(_bullet);
    }
    public override void OnFire()
    {
        if (b_aiming)
        {
            if (f_fireTimer <= 0 && clipAmmo > 0)
                Fire(fireVariablesOnAim);
        }
        else
        {
            base.OnFire();
        }
    }
    public override void OnSprintFire()
    {
        OnMelee();
    }
    public override void OnReload()
    {
        base.OnReload();
    }
}
