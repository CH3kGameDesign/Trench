using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEditor.UIElements;

public class Treasure : Interactable
{
    public int I_value;
    public float F_throwForce = 100;
    public LayerMask LM_thrownIgnoreLayers = new LayerMask();
    public Transform T_backPivot;
    public TextMeshProUGUI TM_valueText;
    public Rigidbody RB_rigidbody;
    public Collider C_collider;

    private int i_droppedLayer = 7;
    private int i_heldLayer = 2;


    // Start is called before the first frame update
    void Start()
    {
        TM_valueText.text = I_value.ToString_Currency();
    }

    // Update is called once per frame
    public override void Update()
    {
        
    }

    public override void OnInteract(BaseController _player)
    {
        _player.Pickup_Treasure(this);
    }

    public void OnPickUp(PlayerController _player)
    {
        Vector3 _pos = -T_backPivot.localPosition;
        Quaternion _rot = Quaternion.Inverse(T_backPivot.localRotation);
        transform.parent = _player.Ref.T_backPivot;
        transform.localPosition = _pos;
        transform.localRotation = _rot;

        RB_rigidbody.isKinematic = true;
        //gameObject.layer = i_heldLayer;
        C_collider.enabled = false;
    }
    public void OnDrop(PlayerController _player, bool _isSprinting = false)
    {
        transform.parent = null;
        RB_rigidbody.isKinematic = false;
        //gameObject.layer = i_droppedLayer;
        C_collider.enabled = true;

        StartCoroutine(OnThrown(_player, _isSprinting ? 1f : 2f));
    }

    IEnumerator OnThrown(PlayerController _player, float _mult)
    {
        Vector3 _forceDir = _player.C_camera.transform.forward;
        _forceDir += _player.C_camera.transform.up * 0.5f;
        RB_rigidbody.AddForce(_forceDir * F_throwForce * _mult, ForceMode.Impulse);
        C_collider.excludeLayers = LM_thrownIgnoreLayers;
        yield return new WaitForSeconds(0.5f);
        C_collider.excludeLayers = new LayerMask();
    }
}
