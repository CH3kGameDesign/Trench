using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class HitObject : MonoBehaviour
{
    public bool B_useHealth = true;
    [Range(0,5)] public float F_damageMult = 1;
    public float I_maxHealth = 10;
    private float f_health = 10;
    bool b_destroyed = false;

    public GameObject PF_hitParticles;

    public UnityEvent<GunManager.bulletClass, DamageSource, HitObject> UE_OnHit;
    public UnityEvent<GunManager.bulletClass, DamageSource, HitObject> UE_OnDestroy;

    public List<damageTypeEnum> ignoreDamageType = new List<damageTypeEnum>();
    public enum damageTypeEnum { all, bullet, fire, explosive, melee};

    public RagdollManager RM_ragdollManager;
    public Vehicle V_vehicle;


    private void Start()
    {
        f_health = I_maxHealth;
    }

    public void OnDamage(GunManager.bulletClass _bullet, DamageSource _source = null)
    {
        if (b_destroyed)
            return;
        if (!ignoreDamageType.Contains(_bullet.D_damageType))
        {
            UE_OnHit.Invoke(_bullet, _source, this);
            if (B_useHealth)
                f_health -= _bullet.F_damage;
            if (f_health <= 0)
                Destroy(_bullet);
        }
    }
    private void Destroy(GunManager.bulletClass _bullet, DamageSource _source = null)
    {
        b_destroyed = true;
        UE_OnDestroy.Invoke(_bullet, _source, this);
    }    
    public GameObject GetParent()
    {
        if (RM_ragdollManager != null)
            return RM_ragdollManager.gameObject;
        if (V_vehicle != null)
            return V_vehicle.gameObject;
        return gameObject;
    }
}
