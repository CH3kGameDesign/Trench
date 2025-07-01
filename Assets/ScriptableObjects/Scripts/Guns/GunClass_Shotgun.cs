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
        if (f_fireTimer <= 0)
        {
            f_fireTimer = meleeVariables.fireRate;
            PM_player.reticle.UpdateRoundCount(this);
            PM_player.reticle.RotateReticle(f_fireTimer);
            A_charModel.Play("Melee_Swing", 1);
            baseController.RM_ragdoll.DisableRig(2.1f);
            baseController.AH_agentAudioHolder.Play(AgentAudioHolder.type.melee);

            if (b_playerGun)
                MusicHandler.AdjustVolume(MusicHandler.typeEnum.guitar, meleeVariables.fireRate / 4);
            else
                MusicHandler.AdjustVolume(MusicHandler.typeEnum.bass, meleeVariables.fireRate / 8);
        }
    }
    public override void OnMelee(bool _isSprinting = false)
    {
        if (_isSprinting)
            OnSprintFire();
        else
            base.OnMelee(false);
    }
    public override void OnReload()
    {
        base.OnReload();
    }
}
