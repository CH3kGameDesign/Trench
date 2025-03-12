using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent (typeof (NavMeshAgent))]
public class AgentController : MonoBehaviour
{
    public GunManager gunManager;
    public FieldOfView fieldOfView;

    private NavMeshAgent NMA_agent;
    private TargetClass followTarget = new TargetClass();
    private TargetClass attackTarget = new TargetClass();

    [Header("Relationship Variables")]
    public string S_characterID;
    [HideInInspector] public Relationship.characterClass C_character = null;
    public List<Relationship.groupEnum> G_backupGroups = new List<Relationship.groupEnum>();
    private bool b_friendly = true;
    private float f_relationshipTimer = 0;
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
    private float f_searchTimer = 0;
    public float F_maxHealth = 100;
    private float f_curHealth = 100;

    [Header("Animations")]
    public Animator A_model;
    private Vector2 v2_animMove = Vector2.zero;

    [Header("Debug Variables")]
    public int DEBUG_EquippedGunNum = 0;
    public bool DEBUG_FollowPlayerImmediately = true;
    public AgentController DEBUG_TargetAgent;

    public stateEnum state = stateEnum.protect;
    public enum stateEnum { idle, patrol, protect, hunt};

    class TargetClass
    {
        public targetTypeEnum targetType = targetTypeEnum.none;

        public PlayerController PC_tarPlayer;
        public AgentController AC_tarAgent;

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
                    pos = AC_tarAgent.NMA_agent.transform.position;
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
                    pos = PC_tarPlayer.NMA_player.transform.position;
                    break;
                case TargetClass.targetTypeEnum.agent:
                    pos = AC_tarAgent.NMA_agent.transform.position;
                    break;
                default:
                    pos = Vector3.zero;
                    return false;
            }
            return true;
        }
        public bool GetTargetPos(Vector3 _fromPos, LayerMask _mask, out Vector3 pos)
        {
            List<Transform> _potTargets;
            switch (targetType)
            {
                case targetTypeEnum.player:
                    _potTargets = PC_tarPlayer.T_targetTransforms;
                    break;
                case targetTypeEnum.agent:
                    _potTargets = AC_tarAgent.T_targetTransforms;
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

    // Start is called before the first frame update
    void Start()
    {
        NMA_agent = GetComponent<NavMeshAgent>();
        NMA_agent.updateRotation = false;

        C_character = Relationship.Instance.GetCharacterFromID(S_characterID);

        if (DEBUG_FollowPlayerImmediately)
        {
            if (b_friendly)
                followTarget.FollowPlayer(FindObjectOfType<PlayerController>());
            else
                attackTarget.FollowPlayer(FindObjectOfType<PlayerController>());
        }
        attackTarget.FollowAgent(DEBUG_TargetAgent);

        gun_Equipped = gunManager.GetGunByInt(DEBUG_EquippedGunNum, this);
    }

    // Update is called once per frame
    void Update()
    {
        bool _firing = false;
        UpdateIsFriendly();
        switch (state)
        {
            case stateEnum.idle:
                break;
            case stateEnum.patrol:
                if (fieldOfView != null)
                    FindTarget();
                break;
            case stateEnum.protect:
                Follow(followTarget);
                _firing = FireManager();
                if (fieldOfView != null && !_firing)
                    FindTarget();
                ModelRotate(_firing);
                break;
            case stateEnum.hunt:
                Follow(attackTarget);
                _firing  = FireManager();
                ModelRotate(_firing);
                break;
            default:
                break;
        }
        AnimationUpdate(_firing);
    }

    void UpdateIsFriendly()
    {
        if (f_relationshipTimer <= 0)
        {
            f_relationshipTimer = 1f;
            b_friendly = !IsHostile();
        }
        else
            f_relationshipTimer -= Time.deltaTime;
    }

    void FindTarget()
    {
        if (f_searchTimer <= 0)
        {
            f_searchTimer = f_searchDelay;
            PlayerController _PC;
            AgentController _AC;
            foreach (var item in fieldOfView.visibleTargets)
            {
                if (item.parent.TryGetComponent<PlayerController> (out _PC))
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
        else
            f_searchTimer -= Time.deltaTime;
    }

    void Follow(TargetClass _target)
    {
        Vector3 _tarPos;
        if (_target.GetNavPos(out _tarPos))
            NMA_agent.SetDestination(_tarPos);
    }

    void ModelRotate(bool _attacking)
    {
        if (_attacking)
        {
            Quaternion _look = Quaternion.LookRotation(v3_attackLocation - NMA_agent.transform.position);
            _look = Quaternion.Euler(new Vector3(0, _look.eulerAngles.y, 0));
            NMA_agent.transform.localRotation = Quaternion.Lerp(NMA_agent.transform.localRotation, _look, Time.deltaTime * 6);
        }
        else if (NMA_agent.remainingDistance > NMA_agent.stoppingDistance)
        {
            Quaternion _look = Quaternion.LookRotation(NMA_agent.desiredVelocity);
            NMA_agent.transform.localRotation = Quaternion.Lerp(NMA_agent.transform.localRotation, _look, Time.deltaTime * 4);
        }
    }

    void AnimationUpdate(bool _firing)
    {
        Vector3 _input = NMA_agent.desiredVelocity;
        if (_firing)
            _input = Quaternion.Inverse(NMA_agent.transform.localRotation) * _input;
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

    public void OnHit(GunManager.bulletClass _bullet)
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
        gameObject.SetActive(false);
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
