using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public GunManager.bulletClass B_info;
    public AnimationCurve AC_dropOffCurve = new AnimationCurve();
    private float f_size = 5;
    private Rigidbody rb_ignore = null;
    public float F_force = 100;

    private List<GameObject> g_affectedObjects = new List<GameObject>();
    private void Start()
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
    public void OnCreate(GunManager.bulletClass _classOverride = null, Rigidbody _ignoreRB = null)
    {
        if (_classOverride != null)
            B_info = _classOverride;
        rb_ignore = _ignoreRB;
    }

    private void OnTriggerEnter(Collider other)
    {
        for (int i = 0; i < g_affectedObjects.Count; i++)
        {
            if (other.gameObject == g_affectedObjects[i])
                return;
        }
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
            if (_rb == rb_ignore)
                return;
            _rb.AddExplosionForce(F_force, transform.position, f_size, 2);
        }
        if (other.TryGetComponent<HitObject>(out _ho))
        {
            float _damage = B_info.F_damage * AC_dropOffCurve.Evaluate(Mathf.Clamp01(_dist / f_size));
            GunManager.bulletClass _bc = B_info.Clone();
            _bc.F_damage = _damage;
            _ho.OnDamage(_bc);
        }
    }
}
