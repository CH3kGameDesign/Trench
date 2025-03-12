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
        Destroy(GO, 5);
        base.Destroy();
    }
}
