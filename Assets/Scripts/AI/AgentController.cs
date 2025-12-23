using PurrNet;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;


public class AgentController : BaseController
{
    public GunManager gunManager;
    public FieldOfView fieldOfView;

    [HideInInspector] public TargetClass followTarget = new TargetClass();
    [HideInInspector] public TargetClass attackTarget = new TargetClass();

    [Header("Relationship Variables")]
    public string S_characterID;
    [HideInInspector] public Relationship.characterClass C_character = null;
    public List<Relationship.groupEnum> G_backupGroups = new List<Relationship.groupEnum>();
    [HideInInspector] public SyncVar<bool> b_friendly = new(true);
    public Transform T_messageHook;

    [Header("Combat Variables")]
    public float F_attackDistance = 20;
    public List<Transform> T_targetTransforms = new List<Transform>();
    public LayerMask LM_hitScan;
    private float f_searchDelay = 0.2f;
    private bool b_firing = false;


    [Header("Loot Table")]
    public Resource.resourceDrop ResourceDrop = new Resource.resourceDrop();

    [Header("Debug Variables")]
    public int DEBUG_EquippedGunNum = 0;
    public bool DEBUG_FollowPlayerImmediately = true;

    private Coroutine Coroutine_Relationship;
    private Coroutine Coroutine_Target;
    private Coroutine Coroutine_RandomPathing;

    public behaviourClass behaviour = new behaviourClass();
    [System.Serializable]
    public class behaviourClass
    {
        public staggerClass stagger = new staggerClass();
        [Space(10)]
        public stateClass onFound = new stateClass()
        {
            state = stateEnum.hunt,
            B_voiceLine = true,
            banter = ConversationID.Banter_Found_001
        };
        public stateClass onHit_Surprise = new stateClass()
        {
            state = stateEnum.aggressive,
            B_voiceLine = true,
            banter = ConversationID.Banter_Attacked_001
        };
        [Space(10)]
        public stateClass onHit_FriendlyFire = new stateClass()
        {
            state = stateEnum.unchanged,
            B_voiceLine = true,
            banter = ConversationID.Banter_FriendlyFire_001
        };
        [Space(10)]
        [Range(0,1)]public float f_healthPercent = 0.5f;
        public stateClass onHit_LowHealth = new stateClass()
        {
            state = stateEnum.scared,
            B_voiceLine = false,
            banter = ConversationID.Banter_Attacked_001
        };
        [Space(10)]
        public float f_allyDeathRange = 3f;
        public stateClass onDeath_Ally = new stateClass()
        {
            state = stateEnum.scared,
            B_voiceLine = false,
            banter = ConversationID.Banter_Attacked_001
        };
        [Space(10)]
        public float f_enemyDeathRange = 15f;
        public stateClass onDeath_Enemy = new stateClass()
        {
            state = stateEnum.loud,
            B_voiceLine = false,
            banter = ConversationID.Banter_Attacked_001
        };

        public void OnFound(AgentController AC)
        {
            if (AC.info.b_alive && (int)AC.state < 10)
            {
                EnemyTimer.Instance.StartTimer_Boss();
                onFound.Activate(AC);
                if (SaveData.missionCurrent)
                    SaveData.missionCurrent.SpawnEnemies(Mission.eventEnum.playerDiscovered);
            }
        }
        public void OnHit(AgentController AC, bool b_player)
        {
            if (AC.info.b_alive)
            {
                //Friendly Fire
                if (AC.b_friendly && b_player)
                {
                    onHit_FriendlyFire.Activate(AC);
                    return;
                }
                //Surprised
                if ((int)AC.state < 3)
                {
                    EnemyTimer.Instance.StartTimer_Boss();
                    onHit_Surprise.Activate(AC);
                    return;
                }
                //Low Health
                if ((AC.info.F_curHealth / AC.info.F_maxHealth) <= f_healthPercent)
                {
                    onHit_LowHealth.Activate(AC);
                    return;
                }
            }
        }
        public void OnDeath(AgentController AC, AgentController _dead)
        {
            if (AC.info.b_alive)
            {
                float _dist = Vector3.Distance(AC.NMA.transform.position, _dead.NMA.transform.position);
                //Ally Death
                if (AC.b_friendly == _dead.b_friendly)
                {
                    if (_dist <= f_allyDeathRange)
                        onDeath_Ally.Activate(AC);
                    return;
                }
                //Enemy Death
                else
                {
                    if (_dist <= f_enemyDeathRange)
                        onDeath_Enemy.Activate(AC);
                    return;
                }
            }
        }
    }
    [System.Serializable]
    public class stateClass
    {
        public stateEnum state = stateEnum.unchanged;
        public bool B_voiceLine = false;
        public ConversationID banter = 0;

        public void Activate(AgentController AC)
        {
            if (state != AC.state)
            {
                if (B_voiceLine)
                    PlayerManager.conversation.StartMessage(banter, AC.T_messageHook);
                AC.ChangeState(state);
            }
        }
    }
    [System.Serializable]
    public class staggerClass
    {
        public bool isStaggered => f_timer > 0;
        public float F_duration = 1f;
        private float f_timer = 0;
        [Space(10)]
        public float F_res_Bullet = 100;
        public float F_res_Fire = 150;
        public float F_res_Explosive = 30;
        public float F_res_Melee = 10;

        public void Stagger(HitObject.damageTypeEnum _type, float _amt, AgentController _AC)
        {
            if (isStaggered) return;
            float _res;
            switch (_type)
            {
                case HitObject.damageTypeEnum.bullet: _res = F_res_Bullet; break;
                case HitObject.damageTypeEnum.fire: _res = F_res_Fire; break;
                case HitObject.damageTypeEnum.explosive: _res = F_res_Explosive; break;
                case HitObject.damageTypeEnum.melee: _res = F_res_Melee; break;
                default: _res = F_res_Bullet; break;
            }
            float _chance = Mathf.Pow(_amt / (_amt + _res),2);

            if (Random.Range(0f, 1f) <= _chance)
                Stagger(_AC);
        }
        public void Stagger(AgentController _AC)
        {
            if (isStaggered) return;
            f_timer = F_duration;

            _AC.A_model.SetTrigger("Staggered");
            //_AC.RM_ragdoll.Aiming(false, true);
            _AC.RM_ragdoll.DisableRig(F_duration);
            _AC.NMA.isStopped = true;
        }
        public void Stagger_Update(AgentController _AC)
        {
            f_timer -= Time.deltaTime;
            if (f_timer <= 0)
                Stagger_End(_AC);
        }
        public void Stagger_End(AgentController _AC)
        {
            f_timer = 0;
            _AC.NMA.isStopped = false;
        }
    }

    public class TargetClass
    {
        public targetTypeEnum targetType = targetTypeEnum.none;

        public PlayerController PC_tarPlayer;
        public AgentController AC_tarAgent;
        public BaseController _base;

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
                _base = _player;
            }
        }
        public void FollowAgent(AgentController _agent)
        {
            if (_agent != null)
            {
                targetType = targetTypeEnum.agent;
                AC_tarAgent = _agent;
                _base = AC_tarAgent;
            }
        }
        public void Stop()
        {
            targetType = targetTypeEnum.none;
            PC_tarPlayer = null;
            AC_tarAgent = null;
            _base = null;
        }
        public bool Check()
        {
            if (targetType != targetTypeEnum.none)
            {
                if (_base.info.b_alive)
                    return true;
                else
                {
                    Stop();
                    return false;
                }
            }
            return false;
        }
        public bool IsActive()
        {
            switch (targetType)
            {
                case targetTypeEnum.none: return false;
                case targetTypeEnum.player: return PC_tarPlayer != null;
                case targetTypeEnum.agent: return AC_tarAgent != null;
                default: return false;
            }
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
                if (item == null) continue;
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

    public override void Awake()
    {
        base.Awake();
        NMA.updateRotation = false;
    }

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
    }

    protected override void OnSpawned()
    {
        base.OnSpawned();
        if (!isController) return;
        C_character = Relationship.Instance.GetCharacterFromID(S_characterID);
        ChangeState(state);

        if (DEBUG_EquippedGunNum < 0)
            DEBUG_EquippedGunNum = Random.Range(0, 3);

        gun_Equipped = gunManager.GetGunByInt(DEBUG_EquippedGunNum, this);
        gun_Equipped.OnEquip(this);

        if (DEBUG_FollowPlayerImmediately)
            PlayerManager.Instance.FollowPlayer(this, b_friendly);
        Coroutine_Relationship = StartCoroutine(UpdateIsFriendly());
        Coroutine_Target = StartCoroutine(UpdateTarget());
    }

    void NavSurface_Update(bool _override = false)
    {
        if (!b_grounded || _override)
        {
            RaycastHit hit;
            if (Physics.SphereCast(NMA.transform.position, 0.2f, Vector3.down, out hit, 1f, LM_TerrainRay))
            {
                NavMeshHit navHit;
                if (NavMesh.SamplePosition(NMA.transform.position, out navHit, 2f, -1))
                {
                    b_grounded = true;
                }
            }
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }
    protected override void OnDisable()
    {
        base.OnDisable();
    }
    protected override void OnDestroy()
    {
        StopAllCoroutines();
        base.OnDestroy();
    }

    // Update is called once per frame
    public override void Update()
    {
        if (!isController)
            return;
        if (behaviour.stagger.isStaggered)
        {
            b_firing = false;
            behaviour.stagger.Stagger_Update(this);
        }
        else
        {
            switch (state)
            {
                case stateEnum.idle:
                    break;
                case stateEnum.wander:
                    ModelRotate(b_firing);
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
                case stateEnum.kamikaze:
                    ModelRotate(false);
                    break;
                case stateEnum.vehicle:
                    if (V_curVehicle)
                        V_curVehicle.OnUpdate(this);
                    break;
                default:
                    break;
            }
        }
        AnimationUpdate(b_firing);
        NavSurface_Update();
        base.Update();
    }
    public override void FixedUpdate()
    {
        if (!isController)
            return;
        switch (state)
        {
            case stateEnum.vehicle:
                if (V_curVehicle)
                    V_curVehicle.OnFixedUpdate(this);
                break;
            default:
                break;
        }
        base.FixedUpdate();
    }

    private IEnumerator UpdateIsFriendly()
    {
        while (true)
        {
            Relationship.meterClass _temp = GetRelationship();
            b_friendly.value = _temp.hostile <= _temp.friendly;
            yield return new WaitForSeconds(1f);
        }
    }
    private IEnumerator UpdateTarget()
    {
        while (true)
        {
            if (behaviour.stagger.isStaggered)
                yield return new WaitForSeconds(behaviour.stagger.F_duration);
            switch (state)
            {
                case stateEnum.idle:
                    if (fieldOfView != null)
                        FindTarget();
                    yield return new WaitForSeconds(0.5f);
                    break;
                case stateEnum.wander:
                    if (fieldOfView != null)
                        FindTarget();
                    GoToRandomPoint(true);
                    yield return new WaitForSeconds(f_searchDelay);
                    break;
                case stateEnum.patrol:
                    if (fieldOfView != null)
                        FindTarget();
                    GoToRandomPoint(false);
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
                case stateEnum.kamikaze:
                    Follow(attackTarget);
                    if (fieldOfView != null && !b_firing)
                        FindTarget();
                    yield return new WaitForSeconds(f_searchDelay);
                    break;
                default:
                    yield return new WaitForSeconds(0.5f);
                    break;
            }
        }
    }
    void GoToRandomPoint(bool _sameRoom = false)
    {
        if (!NMA.isOnNavMesh || V_curVehicle != null)
            return;
        //PLACEHOLDER
        if (!NMA.hasPath || NMA.velocity.sqrMagnitude == 0f)
        {
            if (NMA.navMeshOwner == null)
                return;
            if (_sameRoom)
            {
                if (I_curRoom >= 0)
                    SetDestination(NMA.navMeshOwner.GetComponentInParent<LevelGen>().GetRandomPoint(I_curRoom));
            }
            else
                SetDestination(NMA.navMeshOwner.GetComponentInParent<LevelGen>().GetRandomPoint());
        }
    }

    void FindTarget()
    {
        AgentLocation _AL;
        foreach (var item in fieldOfView.visibleTargets)
        {
            if (item.TryGetComponent<AgentLocation>(out _AL))
            {
                if (!_AL.RM) continue;
                if (!_AL.RM.controller) continue;
                if (_AL.RM.controller == this) continue;
                if (!_AL.RM.controller.info.b_alive) continue;
                if (_AL.RM.controller is PlayerController)
                {
                    PlayerController _PC = (PlayerController)_AL.RM.controller;
                    if (!b_friendly.value)
                    {
                        info.AttackTarget(_PC.info.owner);
                        behaviour.OnFound(this);
                        break;
                    }
                }
                else
                {
                    AgentController _AC = (AgentController)_AL.RM.controller;

                    if (b_friendly.value != _AC.b_friendly.value)
                    {
                        attackTarget.FollowAgent(_AC);
                        behaviour.OnFound(this);
                        break;
                    }
                }
            }
        }
    }

    void SetDestination(Vector3 _destination)
    {
        if (V_curVehicle == null && NMA.isOnNavMesh)
            NMA.SetDestination(_destination);
    }

    void Follow(TargetClass _target)
    {
        if (_target.IsActive())
        {
            Vector3 _tarPos;
            if (_target.GetNavPos(out _tarPos))
                SetDestination(_tarPos);
            Vehicle_FollowCheck(_target);
        }
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
            }
        }
    }

    void ModelRotate(bool _attacking)
    {
        if (V_curVehicle == null && NMA.isOnNavMesh)
        {
            RM_ragdoll.Aiming(_attacking);
            if (_attacking)
            {
                Quaternion _look = Quaternion.LookRotation(T_aimPoint.position - NMA.transform.position);
                _look = Quaternion.Euler(new Vector3(0, _look.eulerAngles.y, 0));
                NMA.transform.rotation = Quaternion.Lerp(NMA.transform.rotation, _look, Time.deltaTime * 6);
            }
            else if (NMA.remainingDistance > NMA.stoppingDistance)
            {
                Quaternion _look = Quaternion.LookRotation(NMA.desiredVelocity);
                NMA.transform.rotation = Quaternion.Lerp(NMA.transform.rotation, _look, Time.deltaTime * 4);
            }
        }
    }

    void AnimationUpdate(bool _firing)
    {
        Vector3 _input = NMA.desiredVelocity;
        if (_firing)
            _input = Quaternion.Inverse(NMA.transform.localRotation) * _input;
        else
            _input = Vector2.ClampMagnitude(Vector2.up * _input.magnitude,1);

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
        if (!gun_Equipped)
            return false;
        Vector3 _tarPos;
        bool _firing = false;
        bool _startShot = false;
        if (gun_Equipped.f_fireTimer <= 0)
        {
            if (gun_Equipped.clipAmmo <= 0)
                gun_Equipped.OnReload();
            else
            {
                if (attackTarget.GetTargetPos(T_barrelHook.position, LM_hitScan, out _tarPos))
                {
                    if (attackTarget.Check())
                    {
                        gun_Equipped.OnFire();
                        T_aimPoint.position = _tarPos;
                        _startShot = true;
                    }
                    else
                        TargetDead();
                }
            }
        }
        else
        {
            if (!_startShot)
                if (attackTarget.GetTargetPos(T_barrelHook.position, LM_hitScan, out _tarPos))
                    T_aimPoint.position = _tarPos;
            _firing = true;
        }
        gun_Equipped.OnUpdate();

        return _firing;
    }

    public void TargetDead()
    {

    }

    public override void OnHit(GunManager.bulletClass _bullet, DamageSource _source = null, HitObject _limb = null)
    {
        if (!info.b_alive)
            return;

        if (_bullet.B_player)
        {
            if (C_character != null)
                C_character.soloRelationship.chaotic += 1;
            else
            {
                List<Relationship.groupEnum> _groups = G_backupGroups;
                foreach (var item in _groups)
                {
                    Relationship.groupClass _group = Relationship.Instance.GetGroupFromEnum(item);
                    _group.relationship.hostile += 1;
                }
            }
            if (!b_friendly)
            {
                _bullet.con_Gun.Damage_Objective(Mathf.FloorToInt(_bullet.F_damage * _limb.F_damageMult));
                info.AttackTarget(_bullet.con_Player.info.owner);
                _bullet.con_Player.AggroAllies(this);
                OnHit_Behaviour(true);
            }
            else
                PlayerManager.conversation.StartMessage(ConversationID.Banter_FriendlyFire_001, T_messageHook);
        }
        else
        {
            if (_bullet.con_Agent != null)
            {
                if (b_friendly != _bullet.con_Agent.b_friendly)
                {
                    attackTarget.FollowAgent(_bullet.con_Agent);
                    OnHit_Behaviour(false);
                }
                else
                    return;
            }
        }

        if (_bullet.ragdollHitTimer > 0 && state != stateEnum.ragdoll)
        {
            StartCoroutine(ChangeState(state, _bullet.ragdollHitTimer));
            ChangeState(stateEnum.ragdoll);
        }
        if (_bullet.pushBackForce > 0)
        {
            Vector3 dir;
            if (_bullet.B_player) dir = _bullet.con_Player.NMA.transform.forward;
            else dir = _bullet.con_Agent.NMA.transform.forward;
            dir *= _bullet.pushBackForce;
            dir += Vector3.up;
            if (state != stateEnum.ragdoll)
            {
                GroundedUpdate(false);
                RB.AddForce(dir, ForceMode.VelocityChange);
            }
            else
                RM_ragdoll.RB_rigidbodies[0].AddForce(dir, ForceMode.VelocityChange);
        }
        float _finalDamage = _bullet.F_damage * _limb.F_damageMult;
        behaviour.stagger.Stagger(_bullet.D_damageType, _finalDamage, this);
        info.Hurt(_finalDamage);
        if (info.F_curHealth <= 0)
            OnDeath(_bullet, _source, _limb);
        else
            AH_agentAudioHolder.Play(AgentAudioHolder.type.hurt);
        HealthUpdate();
        base.OnHit(_bullet);
    }
    [ServerRpc]
    public void OnHit_Behaviour(bool _player)
    {
        behaviour.OnHit(this, _player);
    }

    public override void OnHeal(float _amt)
    {
        if (info.b_alive)
        {
            info.Heal(_amt);
            HealthUpdate();
        }
    }

    public void OnDeath(GunManager.bulletClass _bullet, DamageSource _source = null, HitObject _limb = null)
    {
        if (_bullet.B_player)
        {
            List<Relationship.groupEnum> _groups = G_backupGroups;
            if (C_character != null)
                _groups = C_character.groups;
            foreach (var item in _groups)
            {
                Relationship.groupClass _group = Relationship.Instance.GetGroupFromEnum(item);
                _group.relationship.chaotic += 10;
            }
            _bullet.con_Player.OnKill(this, true);
            _bullet.con_Player.KillMarker_World(_limb.transform);
        }
        else
        {
            if (_bullet.con_Agent != null)
            {
                if (_bullet.con_Agent.b_friendly)
                    PlayerManager.main.OnKill(this, false);
            }
        }

        if (gun_Equipped != null)
            gun_Equipped.OnUnEquip();

        ChangeState(stateEnum.ragdoll);

        MusicHandler.AdjustVolume(MusicHandler.typeEnum.brass, 0.5f);
        //gameObject.SetActive(false);
    }

    public override void AttackTarget(PlayerID? _playerID)
    {
        if (_playerID == null)
            return;
        base.AttackTarget(_playerID);
        foreach (var item in PlayerManager.Instance.Players)
        {
            if(item.owner == _playerID)
            {
                attackTarget.FollowPlayer(item.GetController());
            }
        }
    }

    public override void OnDeath_Server()
    {
        ResourceDrop.Drop(T_model.position);
        ChangeState(stateEnum.ragdoll);
        LevelGen_Holder.Instance.AgentDeath(this);
        AH_agentAudioHolder.Play(AgentAudioHolder.type.death);
    }

    public override void HealthUpdate()
    {
        base.HealthUpdate();
        if (followTarget.PC_tarPlayer != null)
            followTarget.PC_tarPlayer.UpdateFollowerHealth(this);
    }
    public virtual void AssignSlider(UnityEngine.UI.Slider _slider)
    {

    }

    public override bool Revive(bool _forced = false)
    {
        if (!base.Revive(_forced))
            return false;
        if (gun_Equipped)
            gun_Equipped.OnEquip(this);
        info.Revive();

        ChangeState(stateEnum.protect);
        return true;
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
    public override void UpdateRoom(LevelGen_Bounds _bounds, bool _enter = true)
    {
        if (!isController)
            return;
        if (I_curRoom <= -1)
        {
            base.UpdateRoom(_bounds, _enter);

            if (_bounds) I_curRoom = _bounds.I_roomNum;
            else I_curRoom = -1;

            if (V_curVehicle == null)
                CheckReady(AttachToBound);
        }
        else
        {
            switch (state)
            {
                case stateEnum.idle:
                    break;
                case stateEnum.wander:
                    break;
                default:
                    base.UpdateRoom(_bounds, _enter);

                    if (_bounds) I_curRoom = _bounds.I_roomNum;
                    else I_curRoom = -1;

                    if (V_curVehicle == null)
                        CheckReady(AttachToBound);
                    break;
            }
        }
    }
    public void AssignTarget_Attack(PlayerController _con, bool _force = false)
    {
        if (attackTarget.Check() && !_force)
            return;
        attackTarget.FollowPlayer(_con);
    }
    public void AssignTarget_Attack(AgentController _con, bool _force = false)
    {
        if (attackTarget.Check() && !_force)
            return;
        attackTarget.FollowAgent(_con);
    }
    public override string GetName()
    {
        if (C_character != null)
            return C_character.name;
        return "???";
    }
}
