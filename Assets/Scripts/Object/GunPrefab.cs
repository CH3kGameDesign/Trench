using PurrNet;
using UnityEngine;

public class GunPrefab : NetworkBehaviour
{
    public Transform T_barrelHook;
    public ParticleSystem PS_muzzleFire;

    public Transform T_secondHandPoint;
    public Transform T_secondElbowPoint;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (T_barrelHook == null)
            T_barrelHook = transform;
    }

    protected override void OnSpawned()
    {
        base.OnSpawned();
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(Vector3.zero);
    }

    public void Shoot()
    {
        if (PS_muzzleFire != null)
            PS_muzzleFire.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
