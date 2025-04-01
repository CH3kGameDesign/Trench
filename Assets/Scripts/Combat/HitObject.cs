using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HitObject : MonoBehaviour
{
    public bool B_useHealth = true;
    public float I_maxHealth = 10;
    private float f_health = 10;

    public UnityEvent<GunManager.bulletClass, DamageSource> UE_OnHit;
    public UnityEvent<GunManager.bulletClass, DamageSource> UE_OnDestroy;

    public List<damageTypeEnum> ignoreDamageType = new List<damageTypeEnum>();
    public enum damageTypeEnum { all, bullet, fire, explosive};

    private void Start()
    {
        f_health = I_maxHealth;
    }

    public void OnDamage(GunManager.bulletClass _bullet, DamageSource _source = null)
    {
        if (!ignoreDamageType.Contains(_bullet.D_damageType))
        {
            UE_OnHit.Invoke(_bullet, _source);
            if (B_useHealth)
                f_health -= _bullet.F_damage;
            if (f_health <= 0)
                Destroy(_bullet);
        }
    }
    private void Destroy(GunManager.bulletClass _bullet, DamageSource _source = null)
    {
        UE_OnDestroy.Invoke(_bullet, _source);
    }    
}
