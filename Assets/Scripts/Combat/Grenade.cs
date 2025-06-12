using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof (Rigidbody))]
public class Grenade : Bullet
{
    public override void OnCreate(float _damage)
    {
        Vector3 _dir = transform.forward;
        GetComponent<Rigidbody>().AddForce(B_info.F_speed * _dir, ForceMode.Impulse);
        base.OnCreate(_damage);
    }
    public override void Update()
    {
        
    }
    public override void Destroy()
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
        Destroy(GO, 5);
        base.Destroy();
    }
}
