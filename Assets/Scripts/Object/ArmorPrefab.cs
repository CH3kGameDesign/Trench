using PurrNet;
using UnityEngine;

public class ArmorPrefab : NetworkBehaviour
{
    protected override void OnSpawned()
    {
        base.OnSpawned();
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(Vector3.zero);
    }
}
