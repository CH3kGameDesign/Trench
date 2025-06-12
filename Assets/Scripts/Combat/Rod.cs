using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rod : Bullet
{
    private bool b_hitTarget = false;
    public override void Update()
    {
        if (!b_hitTarget)
        {
            RaycastHit hit;
            float _dist = B_info.F_speed * Time.deltaTime;
            if (Physics.SphereCast(transform.position, B_info.F_radius, transform.forward, out hit, _dist, LM_targets))
            {
                DamageObject(hit);
                B_info.con_Gun.OnHit(this);
                Stick(hit);
            }
            else
                transform.position += transform.forward * _dist;
        }
    }

    void Stick(RaycastHit hit)
    {
        transform.position = hit.point;
        transform.parent = hit.transform;
        b_hitTarget = true;
    }

    public override void Destroy()
    {
        if (!b_hitTarget)
            Destroy(gameObject);
    }
    
    public override void Detonate()
    {
        GameObject GO = Instantiate(B_info.PF_impactHit, transform.position, B_info.PF_impactHit.transform.rotation);
        Explosion _exp;
        if (GO.TryGetComponent<Explosion>(out _exp))
        {
            if (B_info.B_player)
                _exp.OnCreate(B_info.con_Player, B_info.con_Gun);
            else
                _exp.OnCreate(B_info.con_Agent, B_info.con_Gun);
        }
        Destroy(GO, 5f);
        base.Destroy();
    }
}
