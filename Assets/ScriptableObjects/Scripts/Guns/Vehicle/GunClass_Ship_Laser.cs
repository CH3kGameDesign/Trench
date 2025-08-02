using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Trench/AssetLists/Gun/Ship/Laser", fileName = "New Ship Laser")]
public class GunClass_Ship_Laser : GunClass
{

    public override GunClass Clone()
    {
        GunClass_Ship_Laser _temp = CreateInstance<GunClass_Ship_Laser>();
        base.Clone(_temp);
        return _temp;
    }
    public override void OnFire()
    {
        base.OnFire();
    }
    public override void OnReload()
    {
        base.OnReload();
    }
}
