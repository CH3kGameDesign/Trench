using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using PurrNet;

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

    public bool B_lockedRotation = false;
    public bool B_collectable = true;

    private Vector3 v3_spawnPos;

    [HideInInspector] public NetworkTreasure networkTreasure;

    private Coroutine heldCoroutine;

    private void Awake()
    {
        networkTreasure = GetComponent<NetworkTreasure>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (TM_valueText != null)
            TM_valueText.text = I_value.ToString_Currency();
        v3_spawnPos = transform.position;
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
        networkTreasure.OnPickup(_player);
        if (heldCoroutine != null)
            StopCoroutine(heldCoroutine);
        heldCoroutine = StartCoroutine(WhileHeld(_player));
    }
    public void OnDrop(PlayerController _player, bool _isSprinting = false)
    {
        Vector3 _forceDir = _player.C_camera.transform.forward;
        _forceDir += _player.C_camera.transform.up * 0.5f;
        networkTreasure.OnDrop(_player, _forceDir, _isSprinting);
        if (heldCoroutine != null)
            StopCoroutine(heldCoroutine);
        if (B_lockedRotation)
            transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
    }

    IEnumerator WhileHeld(PlayerController _player)
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            MusicHandler.AdjustVolume(MusicHandler.typeEnum.synth, 0.1f);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "DeadSpace")
            Respawn();
    }
    void Respawn()
    {
        if (!RB_rigidbody.isKinematic)
        {
            RB_rigidbody.angularVelocity = Vector3.zero;
            RB_rigidbody.linearVelocity = Vector3.zero;
            transform.position = v3_spawnPos;
        }
    }
}
