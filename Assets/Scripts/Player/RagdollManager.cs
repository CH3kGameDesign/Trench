using PurrNet;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class RagdollManager : NetworkBehaviour
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

    public SyncVar<bool> colliderEnabled { get; private set; } = new SyncVar<bool>(true);
    public SyncVar<bool> rbEnabled { get; private set; } = new SyncVar<bool>(true);

    public SkinnedMeshRenderer MR_skinnedMeshRenderer;
    [System.Serializable]
    public class RigClass
    {
        public Rig rig;
        public MultiAimConstraint bodyAim;
        public MultiAimConstraint aim;
        public TwoBoneIKConstraint secondHand;

        public IEnumerator Disable(float _timer = 0)
        {
            bodyAim.weight = 0;
            aim.weight = 0;
            secondHand.weight = 0;
            if (_timer > 0)
            {
                yield return new WaitForSeconds(_timer);
                Enable();
            }
        }
        public void Enable()
        {
            bodyAim.weight = 1;
            aim.weight = 1;
            secondHand.weight = 1;
        }
    }
    public void DisableRig(float _timer = 0)
    {
        StartCoroutine(R_Rig.Disable(_timer));
    }
    public RigClass R_Rig;
    public Transform T_secondHand;
    public Transform T_secondElbow;

    public Rigidbody RB_backJoint;
    private FixedJoint FJ_backJoint;

    public LayerMask LM_ignoreLayersThrown;

    [HideInInspector] public bool _rigActive = false;
    private float f_timeTilChange = 0;

    private List<DamageSource> PrevDamageSpurces = new List<DamageSource>();
    private void Awake()
    {
        rbEnabled.onChanged += EnableRigidbodies;
        colliderEnabled.onChanged += EnableColliders;
    }

    // Start is called before the first frame update
    protected override void OnSpawned()
    {
        base.OnSpawned();
        agentController = BaseController is AgentController;
        R_Rig.rig.weight = 0;
        UpdateBaseTransforms();
        if (!isController)
            return;
        if (DisableOnStart)
            SetRigidBodies(false);
    }
    protected override void OnDestroy()
    {
        rbEnabled.onChanged -= EnableRigidbodies;
        colliderEnabled.onChanged -= EnableColliders;
        base.OnDestroy();
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

    [ServerRpc]
    public void SetColliders(bool _enable) { colliderEnabled.value = _enable; }
    [ServerRpc]
    public void SetRigidBodies(bool _enable) { rbEnabled.value = _enable; }
    public void Attach(Rigidbody _target)
    {
        SetColliders(false);
        SetRigidBodies(true);
        if (FJ_backJoint != null)
        {
            Destroy(FJ_backJoint);
            FJ_backJoint = null;
        }
        RB_backJoint.transform.position = _target.position + (-_target.transform.forward * 0.25f);
        RB_backJoint.transform.rotation = _target.transform.rotation;
        RB_backJoint.transform.eulerAngles += new Vector3(0, 180, 0);
        FJ_backJoint = RB_backJoint.AddComponent<FixedJoint>();
        FJ_backJoint.connectedBody = _target;
    }
    public void Detach()
    {
        if (FJ_backJoint != null)
        {
            Destroy(FJ_backJoint);
            FJ_backJoint = null;
        }

        foreach (var item in C_colliders)
            item.excludeLayers = LM_ignoreLayersThrown;

        SetColliders(true);
        SetRigidBodies(true);

        StartCoroutine(Detach_Delay());
    }

    IEnumerator Detach_Delay()
    {
        yield return new WaitForSeconds(1f);
        foreach (var item in C_colliders)
            item.excludeLayers = new LayerMask();
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
                StartCoroutine(R_Rig.rig.Fade(_aim, _aim ? 0.02f : 0.25f));
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
