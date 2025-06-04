using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : DamageSource
{
    public GunManager.bulletClass B_info;
    public LayerMask LM_targets;
    private Transform t_lockOnTarget;
    public void OnCreate(float _damage, PlayerController _player, GunClass _gun)
    {
        B_info.con_Gun = _gun;
        B_info.B_player = true;
        B_info.con_Player = _player;
        OnCreate(_damage);
    }
    public void OnCreate(float _damage, AgentController _agent, GunClass _gun)
    {
        B_info.con_Gun = _gun;
        B_info.B_player = false;
        B_info.con_Agent = _agent;
        OnCreate(_damage);
    }
    public virtual void OnCreate(float _damage)
    {
        B_info.F_damage = _damage;
        Invoke(nameof(Destroy), B_info.F_lifeTime);
    }
    public virtual void LockOn(Transform _target)
    {
        if (_target != null)
            t_lockOnTarget = _target;
    }
    public virtual void Update()
    {
        RaycastHit hit;
        float _dist = B_info.F_speed * Time.deltaTime;
        if (Physics.SphereCast(transform.position, B_info.F_radius, transform.forward, out hit, _dist, LM_targets))
        {
            DamageObject(hit);
            B_info.con_Gun.OnHit(this);
            HitPoint(hit);
            Destroy();
        }
        else
            transform.position += transform.forward * _dist;
        if (t_lockOnTarget != null)
        {
            Quaternion _tarRotation = Quaternion.LookRotation(t_lockOnTarget.position - transform.position);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, _tarRotation, B_info.F_lockOnTurnSpeed * Time.deltaTime);
        }
    }

    public void DamageObject(RaycastHit hit)
    {
        HitObject hitObject;
        if (hit.transform.TryGetComponent<HitObject>(out hitObject))
        {
            hitObject.OnDamage(B_info, this);
            if (hitObject.PF_hitParticles != null)
            {
                GameObject GO = Instantiate(hitObject.PF_hitParticles, hit.point, B_info.PF_impactHit.transform.rotation);
                GO.transform.forward = hit.normal;
            }
        }
    }

    public void HitPoint(RaycastHit hit)
    {
        GameObject GO = Instantiate(B_info.PF_impactHit, hit.point, B_info.PF_impactHit.transform.rotation);
        GO.transform.forward = hit.normal;
        GO.transform.parent = hit.transform;
    }
    public virtual void Destroy()
    {
        Destroy(gameObject);
    }
    public virtual void Detonate()
    {
        
    }
}
