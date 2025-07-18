using PurrNet;
using System.Collections;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class NetworkTreasure : NetworkIdentity
{
    [HideInInspector] public Treasure treasure;
    protected void Awake()
    {
        treasure = GetComponent<Treasure>();
    }
    [ServerRpc]
    public void OnPickup(PlayerController _player)
    {
        Vector3 _pos = -treasure.T_backPivot.localPosition;
        Quaternion _rot = Quaternion.Inverse(treasure.T_backPivot.localRotation);
        transform.parent = _player.Ref.T_backPivot;
        transform.localPosition = _pos;
        transform.localRotation = _rot;

        treasure.RB_rigidbody.isKinematic = true;
        //gameObject.layer = i_heldLayer;
        treasure.C_collider.enabled = false;
    }
    [ServerRpc]
    public void OnDrop(PlayerController _player, Vector3 _dir, bool _isSprinting = false)
    {
        transform.parent = null;
        treasure.RB_rigidbody.isKinematic = false;
        //gameObject.layer = i_droppedLayer;
        treasure.C_collider.enabled = true;

        StartCoroutine(OnThrown(_player, _dir, _isSprinting ? 2f : 1f));
    }

    IEnumerator OnThrown(PlayerController _player, Vector3 _dir, float _mult)
    {
        
        treasure.RB_rigidbody.AddForce(_dir * treasure.F_throwForce * _mult, ForceMode.Impulse);
        treasure.C_collider.excludeLayers = treasure.LM_thrownIgnoreLayers;
        yield return new WaitForSeconds(1f);
        treasure.C_collider.excludeLayers = new LayerMask();
    }
}
