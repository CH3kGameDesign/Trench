using PurrNet;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(BaseInfo))]
public class BaseController : MonoBehaviour
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
    [HideInInspector] public Transform T_surface = null;
    private Vector3 v3_surfacePos;
    public LayerMask LM_TerrainRay;

    [HideInInspector] public Texture2D T_icon { get; private set;}

    [HideInInspector] public GunClass gun_Equipped;
    [HideInInspector] public GunClass gun_secondaryEquipped;
    [HideInInspector] public GunClass[] gun_EquippedList = new GunClass[3];

    [HideInInspector] public bool b_grounded = false;

    [HideInInspector] public int I_curRoom = -1;

    [HideInInspector] public List<BaseController> followers = new List<BaseController>();

    [Header("Animations")]
    public NetworkAnimator A_model;
    [HideInInspector]public Vector2 v2_animMove = Vector2.zero;

    public stateEnum state = stateEnum.protect;
    public enum stateEnum
    {
        idle = 0, wander = 1, patrol = 2, protect = 3,
        hunt = 10, scared = 11, tactical = 12, loud = 13, aggressive = 14, kamikaze = 15,
        ragdoll = -1,
        vehicle = -2,
        unchanged = -10
    };
    public virtual void ChangeState(stateEnum _state, bool _force = false)
    {
        if (_state == stateEnum.unchanged ||
            _state == state) return;

        if (info.F_curHealth <= 0 &&
            _state != stateEnum.ragdoll &&
            !_force)
            return;

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
    }
    void ExitState(stateEnum _state)
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
                    T_surface_Update(NMA.navMeshOwner.GetComponent<Transform>());
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
        //NMA_player.updateRotation = _grounded;
        NMA.updatePosition = _grounded;
        //NMA_player.isStopped = !_grounded;
        RB.isKinematic = _grounded;
        RB.useGravity = !_grounded;
    }
    public virtual void Awake()
    {
        info = GetComponent<BaseInfo>();
    }

    public virtual void Start()
    {

    }

    public virtual void Update()
    {

    }

    public virtual void FixedUpdate()
    {
        UpdateParentPosition();
    }

    bool onSurface = false;
    public void UpdateParentPosition()
    {
        if (T_surface != null && V_curVehicle == null)
        {
            if (onSurface)
            {
                NMA.Move(T_surface.position - v3_surfacePos);
                RB.transform.position += T_surface.position - v3_surfacePos;
            }
            T_surface_Update();
        }
        else
            onSurface = false;
    }
    public void T_surface_Update()
    {
        v3_surfacePos = T_surface.position;
        onSurface = true;
    }
    public void T_surface_Update(Transform _temp)
    {
        T_surface = _temp;
        v3_surfacePos = _temp.position;
        onSurface = true;
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
        RM_ragdoll.Attach(_pc.RM_ragdoll.RB_backJoint);
    }
    public void OnDrop(PlayerController _player, bool _isSprinting = false)
    {
        RM_ragdoll.Detach();
        Vector3 _forceDir = _player.C_camera.transform.forward;
        _forceDir += _player.C_camera.transform.up * 0.5f;
        float _mult = _isSprinting ? 2 : 1;
        RM_ragdoll.RB_backJoint.AddForce(_forceDir * 400 * _mult, ForceMode.Impulse);
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
    public virtual void Update_Objectives(Objective_Type _type, int _amt)
    {

    }
    public virtual void Update_Objectives(Objective_Type _type, Resource_Type _resource, int _amt)
    {

    }
    public virtual void EnterExit_Vehicle(bool _enter, Vehicle _vehicle)
    {

    }
    public virtual void UpdateRoom(int _roomNum)
    {

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
}
