using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class RagdollManager : MonoBehaviour
{
    public bool DisableOnStart = true;
    public Transform[] T_transforms = new Transform[0];
    private Vector3[] v3_pos = new Vector3[0];
    private Quaternion[] q_rot = new Quaternion[0];
    public Rigidbody[] RB_rigidbodies = new Rigidbody[0];
    public Collider[] C_colliders = new Collider[0];
    public Transform[] T_armorPoints = new Transform[0];
    public BaseController BaseController;
    [HideInInspector] public bool agentController = false;

    public SkinnedMeshRenderer MR_skinnedMeshRenderer;
    public Rig R_aimRig;
    public Transform T_secondHand;
    public Transform T_secondElbow;

    [HideInInspector] public bool _rigActive = false;
    private float f_timeTilChange = 0;

    private List<DamageSource> PrevDamageSpurces = new List<DamageSource>();
    // Start is called before the first frame update
    void Start()
    {
        agentController = BaseController is AgentController;
        if (DisableOnStart)
            EnableRigidbodies(false);
        R_aimRig.weight = 0;
        UpdateBaseTransforms();
    }
    void UpdateBaseTransforms()
    {
        v3_pos = new Vector3[T_transforms.Length];
        q_rot = new Quaternion[T_transforms.Length];
        for (int i = 0; i < T_transforms.Length; i++)
        {
            v3_pos[i] = T_transforms[i].localPosition;
            q_rot[i] = T_transforms[i].localRotation;
        }
    }
    public void ApplyBaseTransforms()
    {
        for (int i = 0; i < T_transforms.Length; i++)
        {
            T_transforms[i].localPosition = v3_pos[i];
            T_transforms[i].localRotation = q_rot[i];
        }
    }
    private void Update()
    {
        if (f_timeTilChange > 0)
            f_timeTilChange -= Time.deltaTime;
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
    public void Aiming(bool _aim, bool _force = false)
    {
        if (f_timeTilChange <= 0 || _force)
        {
            if (_aim != _rigActive)
            {
                if (_aim)
                    f_timeTilChange = 2f;
                else
                    f_timeTilChange = 0f;
                _rigActive = _aim;
                StartCoroutine(R_aimRig.Fade(_aim, _aim ? 0.02f : 0.25f));
            }
        }
        else if (_aim && _rigActive)
            f_timeTilChange = Mathf.Max(f_timeTilChange, 2f);
    }
    public bool GetAgentController(out AgentController _controller)
    {
        if (agentController)
        {
            _controller = BaseController as AgentController;
            return true;
        }
        _controller = null;
        return false;
    }
}
