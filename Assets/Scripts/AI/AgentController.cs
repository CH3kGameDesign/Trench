using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.Experimental.GraphView.GraphView;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;


public class AgentController : BaseController
{
    public GunManager gunManager;
    public FieldOfView fieldOfView;

    private TargetClass followTarget = new TargetClass();
    private TargetClass attackTarget = new TargetClass();

    [Header("Relationship Variables")]
    public string S_characterID;
    [HideInInspector] public Relationship.characterClass C_character = null;
    public List<Relationship.groupEnum> G_backupGroups = new List<Relationship.groupEnum>();
    private bool b_friendly = true;
    public Transform T_messageHook;

    [Header("Combat Variables")]
    public float F_attackDistance = 20;
    public Transform T_barrelHook;
    public List<Transform> T_targetTransforms = new List<Transform>();
    public LayerMask LM_hitScan;
    private GunClass gun_Equipped;
    private float f_fireTimer = 0;
    private float f_burstTimer = 0;
    private int i_burstRemaining = 0;
    private Vector3 v3_attackLocation;
    private float f_searchDelay = 0.2f;
    public float F_maxHealth = 100;
    private float f_curHealth = 100;
    private bool b_firing = false;

    [Header("Animations")]
    public Animator A_model;
    private Vector2 v2_animMove = Vector2.zero;

    [Header("Debug Variables")]
    public int DEBUG_EquippedGunNum = 0;
    public bool DEBUG_FollowPlayerImmediately = true;
    public AgentController DEBUG_TargetAgent;

    private Coroutine Coroutine_Relationship;
    private Coroutine Coroutine_Target;
    private Coroutine Coroutine_RandomPathing;

    public stateEnum state = stateEnum.protect;
    public enum stateEnum { idle, patrol, protect, hunt, ragdoll};

    class TargetClass
    {
        public targetTypeEnum targetType = targetTypeEnum.none;

        public PlayerController PC_tarPlayer;
        public AgentController AC_tarAgent;

        public Vehicle GetVehicle()
        {
            switch (targetType)
            {
                case targetTypeEnum.none:   return null;
                case targetTypeEnum.player: return PC_tarPlayer.V_curVehicle;
                case targetTypeEnum.agent:  return AC_tarAgent.V_curVehicle;
                default:                    return null;
            }
        }

        public enum targetTypeEnum { none, player, agent };

        public void FollowPlayer(PlayerController _player)
        {
            if (_player != null)
            {
                targetType = targetTypeEnum.player;
                PC_tarPlayer = _player;
            }
        }
        public void FollowAgent(AgentController _agent)
        {
            if (_agent != null)
            {
                targetType = targetTypeEnum.agent;
                AC_tarAgent = _agent;
            }
        }
        public void Stop()
        {
            targetType = targetTypeEnum.none;
        }
        public bool GetNavPos(out Vector3 pos)
        {
            switch (targetType)
            {
                case TargetClass.targetTypeEnum.player:
                    pos = PC_tarPlayer.GetPos();
                    break;
                case TargetClass.targetTypeEnum.agent:
                    pos = AC_tarAgent.NMA.transform.position;
                    break;
                default:
                    pos = Vector3.zero;
                    return false;
            }
            return true;
        }
        public bool GetWorldPos(out Vector3 pos)
        {
            switch (targetType)
            {
                case TargetClass.targetTypeEnum.player:
                    pos = PC_tarPlayer.NMA.transform.position;
                    break;
                case TargetClass.targetTypeEnum.agent:
                    pos = AC_tarAgent.NMA.transform.position;
                    break;
                default:
                    pos = Vector3.zero;
                    return false;
            }
            return true;
        }
        public bool GetTargetPos(Vector3 _fromPos, LayerMask _mask, out Vector3 pos)
        {
            Transform[] _potTargets;
            switch (targetType)
            {
                case targetTypeEnum.player:
                    _potTargets = PC_tarPlayer.RM_ragdoll.T_transforms;
                    break;
                case targetTypeEnum.agent:
                    _potTargets = AC_tarAgent.RM_ragdoll.T_transforms;
                    break;
                default:
                    pos = Vector3.zero;
                    return false;
            }

            RaycastHit hit;
            foreach (var item in _potTargets)
            {
                Vector3 _dir = item.position - _fromPos;
                float _dist = Vector3.Distance(item.position, _fromPos);
                if (Physics.Raycast(_fromPos, _dir, out hit, _dist, _mask))
                {
                    continue;
                }
                else
                {
                    pos = item.position;
                    return true;
                }
            }
            pos = Vector3.zero;
            return false;
        }
    }

    public void Awake()
    {
        NMA.updateRotation = false;
    }

    // Start is called before the first frame update
    public override void Start()
    {
        C_character = Relationship.Instance.GetCharacterFromID(S_characterID);

        if (DEBUG_FollowPlayerImmediately)
        {
            if (b_friendly)
                followTarget.FollowPlayer(FindFirstObjectByType<PlayerController>());
            else
                attackTarget.FollowPlayer(FindFirstObjectByType<PlayerController>());
        }
        attackTarget.FollowAgent(DEBUG_TargetAgent);

        gun_Equipped = gunManager.GetGunByInt(DEBUG_EquippedGunNum, this);
    }

    private void OnEnable()
    {
        Coroutine_Relationship = StartCoroutine(UpdateIsFriendly());
        Coroutine_Target = StartCoroutine(UpdateTarget());
    }
    private void OnDisable()
    {
        StopAllCoroutines();
    }

    // Update is called once per frame
    public override void Update()
    {
        switch (state)
        {
            case stateEnum.idle:
                break;
            case stateEnum.patrol:
                ModelRotate(b_firing);
                break;
            case stateEnum.protect:
                b_firing = FireManager();
                ModelRotate(b_firing);
                break;
            case stateEnum.hunt:
                b_firing = FireManager();
                ModelRotate(b_firing);
                break;
            default:
                break;
        }
        AnimationUpdate(b_firing);
    }

    private IEnumerator UpdateIsFriendly()
    {
        while (true)
        {
            b_friendly = !IsHostile();
            yield return new WaitForSeconds(1f);
        }
    }
    private IEnumerator UpdateTarget()
    {
        while (true)
        {
            switch (state)
            {
                case stateEnum.idle:
                    yield return new WaitForSeconds(0.5f);
                    break;
                case stateEnum.patrol:
                    if (fieldOfView != null)
                        FindTarget();
                    GoToRandomPoint();
                    yield return new WaitForSeconds(f_searchDelay);
                    break;
                case stateEnum.protect:
                    Follow(followTarget);
                    if (fieldOfView != null && !b_firing)
                        FindTarget();
                    yield return new WaitForSeconds(f_searchDelay);
                    break;
                case stateEnum.hunt:
                    Follow(attackTarget);
                    if (fieldOfView != null && !b_firing)
                        FindTarget();
                    yield return new WaitForSeconds(f_searchDelay);
                    break;
                case stateEnum.ragdoll:
                    yield return new WaitForSeconds(0.5f);
                    break;
                default:
                    yield return new WaitForSeconds(0.5f);
                    break;
            }
        }
    }
    void GoToRandomPoint()
    {
        if (!NMA.isOnNavMesh || V_curVehicle != null)
            return;
        //PLACEHOLDER
        if (!NMA.hasPath || NMA.velocity.sqrMagnitude == 0f)
            SetDestination(NMA.navMeshOwner.GetComponent<LevelGen>().GetRandomPoint());
    }

    void FindTarget()
    {
        PlayerController _PC;
        AgentController _AC;
        foreach (var item in fieldOfView.visibleTargets)
        {
            if (item.parent.TryGetComponent<PlayerController>(out _PC))
            {
                if (!b_friendly)
                {
                    attackTarget.FollowPlayer(_PC);

                    if (state == stateEnum.patrol)
                    {
                        state = stateEnum.hunt;
                        Conversation.Instance.StartMessage("MSG_Found_001", T_messageHook);
                    }
                    break;
                }
            }
            if (item.TryGetComponent<AgentController>(out _AC))
            {
                if (b_friendly != _AC.b_friendly)
                {
                    attackTarget.FollowAgent(_AC);

                    if (state == stateEnum.patrol)
                    {
                        state = stateEnum.hunt;
                        Conversation.Instance.StartMessage("MSG_Found_001", T_messageHook);
                    }
                    break;
                }
            }
        }
    }

    void SetDestination(Vector3 _destination)
    {
        if (V_curVehicle == null)
            NMA.SetDestination(_destination);
    }

    void Follow(TargetClass _target)
    {
        Vector3 _tarPos;
        if (_target.GetNavPos(out _tarPos))
            SetDestination(_tarPos);
        Vehicle_FollowCheck(_target);
    }

    void Vehicle_FollowCheck(TargetClass _target)
    {
        Vehicle _vehicle = _target.GetVehicle();
        if (_vehicle != null)
        {
            if (V_curVehicle == null)
            {
                _vehicle.OnInteract(this);
            }
        }
        else
        {
            if (V_curVehicle != null)
            {
                V_curVehicle.OnInteract(this);
                transform.parent = null;
            }
        }
    }

    void ModelRotate(bool _attacking)
    {
        if (V_curVehicle == null)
        {
            if (_attacking)
            {
                Quaternion _look = Quaternion.LookRotation(v3_attackLocation - NMA.transform.position);
                _look = Quaternion.Euler(new Vector3(0, _look.eulerAngles.y, 0));
                NMA.transform.localRotation = Quaternion.Lerp(NMA.transform.localRotation, _look, Time.deltaTime * 6);
            }
            else if (NMA.remainingDistance > NMA.stoppingDistance)
            {
                Quaternion _look = Quaternion.LookRotation(NMA.desiredVelocity);
                NMA.transform.localRotation = Quaternion.Lerp(NMA.transform.localRotation, _look, Time.deltaTime * 4);
            }
        }
    }

    void AnimationUpdate(bool _firing)
    {
        Vector3 _input = NMA.desiredVelocity;
        if (_firing)
            _input = Quaternion.Inverse(NMA.transform.localRotation) * _input;
        else
            _input = Vector2.up * _input.magnitude;

        v2_animMove = Vector2.Lerp(v2_animMove, _input, Time.deltaTime / 0.25f);
        A_model.SetFloat("posX", v2_animMove.x);
        A_model.SetFloat("posY", v2_animMove.y);
    }
    void AnimationEnd()
    {
        v2_animMove = Vector2.Lerp(v2_animMove, Vector2.zero, Time.deltaTime / 0.25f);
        A_model.SetFloat("posX", v2_animMove.x);
        A_model.SetFloat("posY", v2_animMove.y);
    }

    bool FireManager()
    {
        Vector3 _tarPos;
        bool _firing = false;
        bool _startShot = false;
        if (f_fireTimer <= 0)
        {
            if (attackTarget.GetTargetPos(T_barrelHook.position, LM_hitScan,out _tarPos))
            {
                i_burstRemaining = Mathf.Max(gun_Equipped.fireVariables.burstAmount, 1);
                f_fireTimer = gun_Equipped.fireVariables.fireRate;

                v3_attackLocation = _tarPos;
                _startShot = true;
            }
        }
        else
            _firing = true;

        if (i_burstRemaining > 0)
        {
            if (f_burstTimer <= 0)
            {
                if (!_startShot)
                    if (attackTarget.GetTargetPos(T_barrelHook.position, LM_hitScan, out _tarPos))
                        v3_attackLocation = _tarPos;

                Bullet GO = Instantiate(gun_Equipped.bullet, T_barrelHook.position, T_barrelHook.rotation);
                GO.transform.LookAt(v3_attackLocation);
                GO.OnCreate(gun_Equipped.fireVariables.damage, this, gun_Equipped);

                f_burstTimer = gun_Equipped.fireVariables.burstRate;
                i_burstRemaining--;
            }
            _firing = true;
        }

        if (f_burstTimer > 0) f_burstTimer -= Time.deltaTime;
        if (f_fireTimer > 0) f_fireTimer -= Time.deltaTime;

        return _firing;
    }

    public void TargetDead()
    {
        attackTarget.Stop();
    }

    public override void OnHit(GunManager.bulletClass _bullet)
    {
        if (_bullet.B_player)
        {
            if (C_character != null)
                C_character.soloRelationship.hostile += 1;
            else
            {
                List<Relationship.groupEnum> _groups = G_backupGroups;
                foreach (var item in _groups)
                {
                    Relationship.groupClass _group = Relationship.Instance.GetGroupFromEnum(item);
                    _group.relationship.hostile += 1;
                }
            }
            if (IsHostile())
            {
                attackTarget.FollowPlayer(_bullet.con_Player);
                if (state == stateEnum.patrol)
                {
                    state = stateEnum.hunt;
                    Conversation.Instance.StartMessage("MSG_Attacked_001", T_messageHook);
                }
                else if (b_friendly)
                    Conversation.Instance.StartMessage("MSG_Betray_001", T_messageHook);
            }
        }
        else
        {
            if (_bullet.con_Agent != null)
            {
                if (IsHostile() != _bullet.con_Agent.IsHostile())
                {
                    attackTarget.FollowAgent(_bullet.con_Agent);
                    if (state == stateEnum.patrol)
                    {
                        state = stateEnum.hunt;
                        Conversation.Instance.StartMessage("MSG_Attacked_001", T_messageHook);
                    }
                }
            }
        }

        f_curHealth -= _bullet.F_damage;
        if (f_curHealth <= 0)
            OnDeath(_bullet);
    }

    void OnDeath(GunManager.bulletClass _bullet)
    {
        if (_bullet.B_player)
        {
            List<Relationship.groupEnum> _groups = G_backupGroups;
            if (C_character != null)
                _groups = C_character.groups;
            foreach (var item in _groups)
            {
                Relationship.groupClass _group = Relationship.Instance.GetGroupFromEnum(item);
                _group.relationship.hostile += 10;
            }
        }
        else
        {
            if (_bullet.con_Agent != null)
                _bullet.con_Agent.TargetDead();
        }

        state = stateEnum.ragdoll;
        RM_ragdoll.EnableRigidbodies(true);
        GroundedUpdate(false);
        A_model.enabled = false;
        //gameObject.SetActive(false);
    }

    void GroundedUpdate(bool _grounded)
    {
        NMA.updatePosition = _grounded;
    }

    bool IsHostile()
    {
        Relationship.meterClass _temp = GetRelationship();
        return (_temp.hostile > _temp.friendly);
    }

    Relationship.meterClass GetRelationship()
    {
        Relationship.meterClass _temp;
        if (C_character != null)
            _temp = C_character.getRelationship();
        else
        {
            _temp = new Relationship.meterClass();
            foreach (var item in G_backupGroups)
                _temp.Add(Relationship.Instance.GetGroupFromEnum(item).relationship);
        }

        return _temp;
    }
}
