using PurrNet;
using PurrNet.Packing;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(BaseInfo))]
public class BaseController : NetworkBehaviour
{
    [HideInInspector] public BaseInfo info;
    public NavMeshAgent NMA;
    public Transform T_model;
    public Transform T_gunHook;
    public Transform T_barrelHook;
    public Transform T_aimPoint;
    public Rigidbody RB;
    public RagdollManager RM_ragdoll;
    public AgentAudioHolder AH_agentAudioHolder;
    [HideInInspector] public Vehicle V_curVehicle = null;
    [HideInInspector] public LevelGen_Bounds LGB_curBounds = null;
    public LayerMask LM_TerrainRay;

    [HideInInspector] public Texture2D T_icon { get; private set;}

    [HideInInspector] public GunClass gun_Equipped;
    [HideInInspector] public GunClass gun_secondaryEquipped;
    [HideInInspector] public GunClass[] gun_EquippedList = new GunClass[3];

    [HideInInspector] public bool b_grounded = false;

    private bool b_updateNav = true;
    private Vector3 prevPos = Vector3.zero;

    [HideInInspector] public int I_curRoom = -1;

    [HideInInspector] public List<BaseController> followers = new List<BaseController>();

    [Header("Animations")]
    public NetworkAnimator A_model;
    [HideInInspector]public Vector2 v2_animMove = Vector2.zero;

    public SyncVar<Vector3Int> V3ID = new (Vector3Int.left,0,true);

    protected bool isReady = false;

    public stateEnum state = stateEnum.protect;
    public enum stateEnum
    {
        idle = 0, wander = 1, patrol = 2, protect = 3,
        hunt = 10, scared = 11, tactical = 12, loud = 13, aggressive = 14, kamikaze = 15,
        ragdoll = -1,
        vehicle = -2,
        unchanged = -10
    };
    public IEnumerator ChangeState(stateEnum _state, float _timer)
    {
        yield return new WaitForSeconds(_timer);
        ChangeState(_state);
    }

    public virtual bool ChangeState(stateEnum _state, bool _force = false)
    {
        if (_state == stateEnum.unchanged ||
            _state == state) return false;

        if (info.F_curHealth <= 0 &&
            _state != stateEnum.ragdoll &&
            !_force)
            return false;

        ExitState(state);
        switch (_state)
        {
            case stateEnum.ragdoll:
                RM_ragdoll.SetRigidBodies(true);
                GroundedUpdate(false);
                RM_ragdoll.R_Rig.rig.weight = 0;
                A_model.enabled = false;
                break;
            case stateEnum.kamikaze:
                NMA.stoppingDistance = 0.1f;
                break;
            default:
                break;
        }
        state = _state;
        return true;
    }
    protected virtual void ExitState(stateEnum _state)
    {
        switch (_state)
        {
            case stateEnum.ragdoll:
                RM_ragdoll.transform.position = RM_ragdoll.T_transforms[0].position;
                //RM_ragdoll.T_transforms[0].localPosition = Vector3.zero;
                RB.transform.position = RM_ragdoll.transform.position;
                RM_ragdoll.transform.localPosition = Vector3.down;
                RM_ragdoll.transform.localRotation = new Quaternion();

                RM_ragdoll.SetRigidBodies(false);
                NavMeshHit navHit;
                if (NavMesh.SamplePosition(NMA.transform.position, out navHit, 2f, -1))
                {
                    Vector3 _pos = navHit.position;
                    _pos.y = NMA.transform.position.y;
                    NMA.Warp(_pos);
                }
                GroundedUpdate(true);
                RM_ragdoll.ApplyBaseTransforms();
                A_model.enabled = true;
                break;
            case stateEnum.kamikaze:
                NMA.stoppingDistance = 5;
                break;
            default:
                break;
        }
    }

    public void GroundedUpdate(bool _grounded)
    {
        b_grounded = _grounded;
        b_updateNav = _grounded;
        //NMA_player.updateRotation = _grounded;
        //NMA.updatePosition = _grounded;
        //NMA_player.isStopped = !_grounded;
        RB.isKinematic = _grounded;
        RB.useGravity = !_grounded;
    }
    public virtual void Awake()
    {
        info = GetComponent<BaseInfo>();
        NMA.updatePosition = false;
        prevPos = NMA.transform.position;
        V3ID.onChanged += AttachToBound;
        //NT_agent.onParentEvent += AttachToBound;
    }

    public virtual void Start()
    {

    }

    public virtual void Update()
    {
        if (b_updateNav && NMA.isOnNavMesh)
        {
            NMA.Move(NMA.transform.position - prevPos);
            NMA.transform.position = NMA.nextPosition;
        }
        prevPos = NMA.transform.position;
    }
    public void SetPosition(Vector3 pos)
    {
        NMA.transform.position = pos;
        prevPos = pos;
        NMA.Warp(pos);
    }
    public virtual void FixedUpdate()
    {
        
    }
    protected override void OnSpawned()
    {
        base.OnSpawned();
        isReady = true;
        onReadyEvent?.Invoke();
    }
    protected virtual void OnEnable()
    {
    }
    protected virtual void OnDisable()
    {
    }
    protected override void OnDestroy()
    {
        V3ID.onChanged -= AttachToBound;
        //NT_agent.onParentEvent -= AttachToBound;
        base.OnDestroy();
    }

    public virtual void OnHit(GunManager.bulletClass _bullet)
    {
        
    }

    public virtual void OnHeal(float _amt)
    {

    }
    public virtual void HealthUpdate()
    {

    }

    public virtual void Pickup_Treasure(Treasure _treasure)
    {

    }
    public virtual void PickedUp(PlayerController _pc)
    {
        if (_pc.info.owner != null)
            RM_ragdoll.Attach((PlayerID)_pc.info.owner);
    }
    public void OnDrop(PlayerController _player, bool _isSprinting = false)
    {
        Vector3 _forceDir = _player.C_camera.transform.forward;
        _forceDir += _player.C_camera.transform.up * 0.5f;
        float _mult = _isSprinting ? 2 : 1;
        RM_ragdoll.Detach(_forceDir * _mult * 400);
    }



    public virtual void Update_Objectives()
    {

    }
    public virtual void Revive()
    {

    }
    public virtual void OnDeath()
    {

    }
    public virtual void OnDeath_Server()
    {

    }
    public virtual void AttackTarget(PlayerID? _playerID)
    {

    }
    public virtual void Update_Objectives(Objective_Type _type, int _amt)
    {

    }
    public virtual void Update_Objectives(Objective_Type _type, Resource_Type _resource, int _amt)
    {

    }
    public virtual void EnterExit_Vehicle(bool _enter, Vehicle _vehicle)
    {

    }
    public virtual void UpdateRoom(LevelGen_Bounds _bounds, bool _enter = true)
    {
        LGB_curBounds = _bounds;
    }

    public virtual void AttachToBound()
    {
        if (!isController) return;
        if (LGB_curBounds != null)
        {
            V3ID.value = LGB_curBounds.V3ID;
            //RB.transform.parent = LGB_curBounds.transform;
            RB.transform.parent = LevelGen_Holder.Instance.List[V3ID.value.x].T_Holder;
        }
        else
        {
            V3ID.value = Vector3Int.left;
            RB.transform.parent = transform;
        }
    }
    public virtual void AttachToBound(Transform T)
    {
        if (!isController) return;
        RB.transform.parent = T;
    }

    public void AttachToBound(Vector3Int _V3ID)
    {
        if (isController)
           return;
        LevelGen_Holder.Instance.CheckReady(AttachToBound_Obs);
    }
    public void AttachToBound_Obs()
    {
        Transform T = transform;
        Vector3Int _V3ID = V3ID.value;
        ///Vector3Int _V3ID = NT_agent.V3ID;
        if (_V3ID.x >= 0)
        {
            if (_V3ID.x >= LevelGen_Holder.Instance.List.Count) return;
            //if (_V3ID.y >= LevelGen_Holder.Instance.List[_V3ID.x].LG_Blocks.Count) return;
            //if (_V3ID.z >= LevelGen_Holder.Instance.List[_V3ID.x].LG_Blocks[_V3ID.y].B_bounds.Count) return;
            //T = LevelGen_Holder.Instance.List[_V3ID.x].LG_Blocks[_V3ID.y].B_bounds[_V3ID.z].transform;
            T = LevelGen_Holder.Instance.List[V3ID.value.x].T_Holder;
        }
        RB.transform.parent = T;
    }

    public virtual void AddFollower(BaseController _base)
    {
        followers.Add(_base);
    }
    public virtual void RemoveFollower(BaseController _base)
    {
        followers.Remove(_base);
    }
    public virtual void UpdateFollowerHealth(BaseController _base)
    {

    }
    public virtual void UpdateVehicleHealth(Vehicle _vehicle)
    {

    }
    public virtual string GetName()
    {
        return "Undefined";
    }
    public virtual void SetIcon(Texture2D _icon)
    {
        T_icon = _icon;
    }
    public virtual void Weapon_Fired()
    {

    }
    public delegate void OnReadyEvent();
    public event OnReadyEvent onReadyEvent;
    public void CheckReady(OnReadyEvent _event)
    {
        if (!isReady)
            onReadyEvent += _event;
        else
            _event.Invoke();
    }
}
