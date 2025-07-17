using PurrNet;
using UnityEngine;

public class GunPrefab : NetworkBehaviour
{
    public Transform T_barrelHook;
    public ParticleSystem PS_muzzleFire;

    public Transform T_secondHandPoint;
    public Transform T_secondElbowPoint;

    public GameObject[] G_activeOnFireReady;
    bool _fireReady = true;
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

    public void FireReady(bool _ready)
    {
        if (G_activeOnFireReady.Length > 0 && _fireReady != _ready)
            FireReady_Send(_ready);
    }

    [ObserversRpc]
    void FireReady_Send(bool _ready)
    {
        _fireReady = _ready;
        foreach (var item in G_activeOnFireReady)
        {
            item.SetActive(_ready);
        }
    }
}
