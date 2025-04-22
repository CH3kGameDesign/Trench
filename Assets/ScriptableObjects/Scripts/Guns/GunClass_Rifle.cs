using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Trench/AssetLists/Gun/Rifle", fileName = "New Rifle Gun")]
public class GunClass_Rifle : GunClass
{
    public fireClass fireVariablesOnAim;
    public float F_grenadeDamagePerBullet = 3;
    public Grenade reloadGrenade;

    public override GunClass Clone()
    {
        GunClass_Rifle _temp = CreateInstance<GunClass_Rifle>();
        base.Clone(_temp);
        _temp.fireVariablesOnAim = fireVariablesOnAim;
        _temp.F_grenadeDamagePerBullet = F_grenadeDamagePerBullet;
        _temp.reloadGrenade = reloadGrenade;
        return _temp;
    }
    public override void OnFire()
    {
        if (b_aiming)
        {
            if (f_fireTimer <= 0 && clipAmmo > 0)
                Fire(fireVariablesOnAim);
        }
        else
            base.OnFire();
    }
    public override void OnReload()
    {
        if (f_fireTimer <= 0 && clipAmmo < clipVariables.clipSize)
        {
            PM_player.NMA.transform.eulerAngles = new Vector3(0, PM_player.T_camHolder.eulerAngles.y, 0);
            Grenade GO = Instantiate(reloadGrenade, PM_player.T_barrelHook.position, PM_player.T_camHolder.rotation);
            float _damage = F_grenadeDamagePerBullet * (clipVariables.clipSize - clipAmmo);
            GO.OnCreate(_damage, PM_player, this);
            base.OnReload();
        }
    }
}
