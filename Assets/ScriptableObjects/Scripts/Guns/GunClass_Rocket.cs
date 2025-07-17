using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[CreateAssetMenu(menuName = "Trench/AssetLists/Gun/Rocket", fileName = "New Rocket Gun")]
public class GunClass_Rocket : GunClass
{
    public Explosion PF_meleeExplosion;
    public float F_lockOnTime = 3f;
    public float F_lockOnDistance = 100f;
    private float f_lockOnTimer = 0f;
    private bool b_lockedOn = false;
    private Transform t_lockOnTarget;
    public string[] S_lockOnTags;
    
    public override GunClass Clone()
    {
        GunClass_Rocket _temp = CreateInstance<GunClass_Rocket>();
        _temp.PF_meleeExplosion = PF_meleeExplosion;
        _temp.F_lockOnTime = F_lockOnTime;
        _temp.F_lockOnDistance = F_lockOnDistance;
        _temp.S_lockOnTags = S_lockOnTags;
        base.Clone(_temp);
        return _temp;
    }
    public override void OnMelee(bool _isSprinting = false)
    {
        if (clipAmmo > 0 && f_fireTimer <= 0)
        {
            Explosion GO = Instantiate(PF_meleeExplosion, PM_player.T_barrelHook.position, PF_meleeExplosion.transform.rotation);
            GO.OnCreate(null);
            Destroy(GO, 5);
            if (b_playerGun)
            {
                PM_player.RM_ragdoll.AddSource(GO);
                PM_player.Jump_Force(2f);
                if (_isSprinting) PM_player.Apply_Force(PM_player.RB.transform.forward);
            }
            else
            {
                AC_agent.RM_ragdoll.AddSource(GO);
                //AC_agent.Jump_Force(2f);
                //if (_isSprinting) PM_player.Apply_Force(PM_player.RB.transform.forward);
            }
                clipAmmo = 0;
        }
        base.OnMelee();
    }
    public override void OnUpdate()
    {
        if(b_aiming)
        {
            bool _foundObject = false;
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, F_lockOnDistance, LM_GunRay))
            {
                foreach (var item in S_lockOnTags)
                {
                    if (hit.transform.tag == item)
                    {
                        _foundObject = true;
                        if (t_lockOnTarget != hit.transform)
                        {
                            f_lockOnTimer = 0;
                            DisengageLockOn();
                            t_lockOnTarget = hit.transform;
                        }
                        f_lockOnTimer += Time.deltaTime;
                        if (f_lockOnTimer > F_lockOnTime)
                        {
                            EngageLockOn();
                            f_lockOnTimer = F_lockOnTime;
                        }
                        break;
                    }
                }
            }
            if (!_foundObject)
            {
                f_lockOnTimer -= Time.deltaTime;
                if (f_lockOnTimer < 0)
                {
                    DisengageLockOn();
                    f_lockOnTimer = 0;
                    t_lockOnTarget = null;
                }
            }
        }
        else
        {
            f_lockOnTimer = 0;
            DisengageLockOn();
            t_lockOnTarget = null;
        }
        UpdateLockOnGraphic();
        base.OnUpdate();
    }

    void UpdateLockOnGraphic()
    {
        if (b_playerGun)
        {
            if (t_lockOnTarget != null)
            {
                Vector3 _tarPos = Camera.main.WorldToScreenPoint(t_lockOnTarget.position);
                _tarPos /= PlayerManager.conversation.C_canvas.scaleFactor;

                PM_player.RT_lockOnPoint.anchoredPosition = _tarPos;
                PM_player.RT_lockOnPoint.gameObject.SetActive(true);

                float _rotation = 90 * (f_lockOnTimer / F_lockOnTime);
                PM_player.RT_lockOnPoint.localEulerAngles = new Vector3(0, 0, _rotation);
            }
            else
                PM_player.RT_lockOnPoint.gameObject.SetActive(false);
        }
    }

    void EngageLockOn()
    {
        if (b_lockedOn == false)
        {
            if (b_playerGun)
                PM_player.RT_lockOnPoint.GetComponent<Image>().color = Color.green;
            b_lockedOn = true;
        }
    }
    void DisengageLockOn()
    {
        if (b_lockedOn == true)
        {
            if (b_playerGun)
                PM_player.RT_lockOnPoint.GetComponent<Image>().color = Color.grey;
            b_lockedOn = false;
        }
    }

    public override void OnBullet(Bullet _bullet)
    {
        if (b_lockedOn)
            _bullet.LockOn(t_lockOnTarget);
        base.OnBullet(_bullet);
    }
}
