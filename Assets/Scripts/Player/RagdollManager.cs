using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollManager : MonoBehaviour
{
    public bool DisableOnStart = true;
    public Transform[] T_transforms = new Transform[0];
    public Rigidbody[] RB_rigidbodies = new Rigidbody[0];
    public Collider[] C_colliders = new Collider[0];
    public Transform[] T_armorPoints = new Transform[0];
    public BaseController BaseController;

    private List<DamageSource> PrevDamageSpurces = new List<DamageSource>();
    // Start is called before the first frame update
    void Start()
    {
        if (DisableOnStart)
            EnableRigidbodies(false);
    }
    public void EnableRigidbodies(bool _enable)
    {
        foreach (var item in RB_rigidbodies)
        {
            item.isKinematic = !_enable;
            item.useGravity = _enable;
        }
    }
    public void EnableColliders(bool _enable)
    {
        foreach (var item in C_colliders)
        {
            item.enabled = _enable;
        }
    }
    public void SetLayer(int _layerNum)
    {
        foreach (var item in RB_rigidbodies)
        {
            item.gameObject.layer = _layerNum;
        }
    }
    public void OnHit(GunManager.bulletClass _bullet, DamageSource _source = null)
    {
        if (!PrevDamageSpurces.Contains(_source) || _source == null)
        {
            if (BaseController != null)
                BaseController.OnHit(_bullet);
            AddSource(_source);
        }
    }
    public void AddSource(DamageSource _source)
    {
        if (_source != null)
        {
            PrevDamageSpurces.Add(_source);
            Invoke(nameof(RemoveFirstSource), 1f);
        }
    }
    void RemoveFirstSource()
    {
        if (PrevDamageSpurces.Count > 1)
            PrevDamageSpurces.RemoveAt(0);
    }
}
