using UnityEngine;

public class GunPrefab : MonoBehaviour
{
    public Transform T_barrelHook;
    public ParticleSystem PS_muzzleFire;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (T_barrelHook == null)
            T_barrelHook = transform;
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
