using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Explosion : DamageSource
{
    public GunManager.bulletClass B_info;
    public AnimationCurve AC_dropOffCurve = new AnimationCurve();
    private float f_size = 5;
    public float F_force = 100;

    private List<GameObject> g_affectedObjects = new List<GameObject>();
    public override void Start()
    {
        
    }
    void Awake()
    {
        f_size = transform.localScale.x * GetComponent<SphereCollider>().radius;
        Invoke(nameof(DisableCollider), B_info.F_lifeTime);
    }
    void DisableCollider()
    {
        GetComponent<SphereCollider>().enabled = false;
    }
    public void OnCreate(GunManager.bulletClass _classOverride = null)
    {
        if (_classOverride != null)
            B_info = _classOverride;
    }

    public void OnCreate(PlayerController _player, GunClass _gun)
    {
        B_info.con_Gun = _gun;
        B_info.B_player = true;
        B_info.con_Player = _player;
    }
    public void OnCreate(AgentController _agent, GunClass _gun)
    {
        B_info.con_Gun = _gun;
        B_info.B_player = false;
        B_info.con_Agent = _agent;
    }

    private void OnTriggerEnter(Collider other)
    {
        for (int i = 0; i < g_affectedObjects.Count; i++)
        {
            if (other.gameObject == g_affectedObjects[i])
                return;
        }
        HitObject HO;
        if(other.TryGetComponent<HitObject>(out HO))
        {
            if (HO.RM_ragdollManager != null)
            {
                foreach (var item in HO.RM_ragdollManager.C_colliders)
                    g_affectedObjects.Add(item.gameObject);
            }
        }
        else
            g_affectedObjects.Add(other.gameObject);
        Damage(other);
    }

    void Damage(Collider other)
    {
        Rigidbody _rb;
        HitObject _ho;
        float _dist = Vector3.Distance(transform.position, other.ClosestPoint(transform.position));
        if (other.TryGetComponent<Rigidbody>(out _rb))
        {
            _rb.AddExplosionForce(F_force, transform.position, f_size, 2);
        }
        if (other.TryGetComponent<HitObject>(out _ho))
        {
            float _damage = B_info.F_damage * AC_dropOffCurve.Evaluate(Mathf.Clamp01(_dist / f_size));
            GunManager.bulletClass _bc = B_info.Clone();
            _bc.F_damage = _damage;
            _ho.OnDamage(_bc, this);

            if (B_info.B_player)
            {
                B_info.con_Gun.Damage_Objective(Mathf.FloorToInt(_damage));
                B_info.con_Player.Update_Objectives(Objective_Type.Damage_Explosions, Mathf.FloorToInt(_damage));
            }
        }
    }
}
