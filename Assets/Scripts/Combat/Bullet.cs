using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Bullet : DamageSource
{
    public GunManager.bulletClass B_info;
    public LayerMask LM_targets;
    private Transform t_lockOnTarget;

    private List<GameObject> hitObjects = new List<GameObject>();
    public void OnCreate(float _damage, PlayerController _player, GunClass _gun, Ship _ship = null)
    {
        B_info.con_Gun = _gun;
        B_info.B_player = true;
        B_info.con_Player = _player;
        B_info.con_Ship = _ship;
        OnCreate(_damage);
    }
    public void OnCreate(float _damage, AgentController _agent, GunClass _gun, Ship _ship = null)
    {
        B_info.con_Gun = _gun;
        B_info.B_player = false;
        B_info.con_Agent = _agent;
        B_info.con_Ship = _ship;
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
        if (!B_info.con_Gun)
            return;
        if (B_info.F_speed > 0)
        {
            RaycastHit hit;
            float _dist = B_info.F_speed * Time.deltaTime;
            if (Physics.SphereCast(transform.position, B_info.F_radius, transform.forward, out hit, _dist, LM_targets))
            {
                if (DamageObject(hit))
                {
                    B_info.con_Gun.OnHit(this);
                    HitPoint(hit);
                    if (B_info.breakOnImpact)
                        Destroy();
                }
            }
            else
                transform.position += transform.forward * _dist;
            if (t_lockOnTarget != null)
            {
                Quaternion _tarRotation = Quaternion.LookRotation(t_lockOnTarget.position - transform.position);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, _tarRotation, B_info.F_lockOnTurnSpeed * Time.deltaTime);
            }
        }
    }

    public bool DamageObject(Collider collider)
    {
        HitObject hitObject;
        if (collider.TryGetComponent<HitObject>(out hitObject))
        {
            if (DamageObject(hitObject))
            {
                return true;
            }
            return false;
        }
        if (hitObjects.Contains(collider.gameObject))
            return false;
        hitObjects.Add(collider.gameObject);
        return true;
    }

    public bool DamageObject(RaycastHit hit)
    {
        HitObject hitObject;
        if (hit.transform.TryGetComponent<HitObject>(out hitObject))
        {
            if (DamageObject(hitObject))
            {
                if (hitObject.PF_hitParticles != null)
                {
                    GameObject GO = Instantiate(hitObject.PF_hitParticles, hit.point, B_info.PF_impactHit.transform.rotation);
                    GO.transform.forward = hit.normal;
                }
                return true;
            }
            return false;
        }
        if (hitObjects.Contains(hit.transform.gameObject))
            return false;
        hitObjects.Add(hit.transform.gameObject);
        return true;
    }
    public bool DamageObject(HitObject hitObject)
    {
        GameObject _parent = hitObject.GetParent();
        if (B_info.con_Player != null)
            if (_parent == B_info.con_Player.gameObject)
                return false;
        if (B_info.con_Agent != null)
            if (_parent == B_info.con_Agent.gameObject)
                return false;
        if (B_info.con_Ship != null)
            if (_parent == B_info.con_Ship.gameObject)
                return false;

        if (hitObjects.Contains(_parent))
            return false;
        hitObjects.Add(_parent);
        hitObject.OnDamage(B_info, this);
        HitMarker();
        return true;
    }

    void HitMarker()
    {
        if (B_info.B_player)
            B_info.con_Player.reticle().Hit(B_info.D_damageType);
    }

    public void HitPoint(RaycastHit hit)
    {
        GameObject GO = Instantiate(B_info.PF_impactHit, hit.point, B_info.PF_impactHit.transform.rotation);
        Explosion _exp;
        if (GO.TryGetComponent<Explosion>(out _exp))
        {
            if (B_info.B_player)
                _exp.OnCreate(B_info.con_Player, B_info.con_Gun);
            else
                _exp.OnCreate(B_info.con_Agent, B_info.con_Gun);
        }
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

    public virtual void OnTriggerEnter(Collider _collider)
    {
        if (!B_info.con_Gun)
            return;
        if (DamageObject(_collider))
        {
            B_info.con_Gun.OnHit(this);
            if (B_info.breakOnImpact)
                Destroy();
        }
    }
}
