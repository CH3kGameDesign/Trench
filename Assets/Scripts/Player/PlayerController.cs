using PurrNet;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerController : BaseController
{
    #region References
    [HideInInspector] public gameStateEnum GameState = gameStateEnum.active;
    public enum gameStateEnum { inactive, active, dialogue, dialogueResponse, menu }

    public GunManager gunManager;
    public Conversation conversation;

    public RefClass Ref = new RefClass();
    [System.Serializable]
    public class RefClass
    {
        public Recall R_recall;
        public RadialMenu RM_radial;
        public TextMeshProUGUI TM_interactText;
        public TextMeshProUGUI TM_controlText;

        public RectTransform RT_canvasPivot;
        [HideInInspector] public Vector3 V3_canvasRot;

        public HealthUI HUI_health;
        public GunUI GUI_gun;

        public ObjectiveHUD HUD_objective;

        public Transform T_backPivot;

        public SpeedLines speedLines;
        public SpeedLines hurtFace;
    }
    [Header("UI")]
    public HealthUI PF_followerHealth;
    public RectTransform RT_followerHealthHolder;
    private List<HealthUI> FollowerHealthList = new List<HealthUI>();
    [Header ("Camera Values")]
    public float F_camLatSpeed = 1;
    public float F_camRotSpeed = 240f;
    public float F_camAimRotSpeed = 80f;
    public float F_camSprintRotSpeed = 80f;

    float f_camSensitivity = 1f;

    private float f_camDistance = 5;
    public Camera C_camera;
    public Transform T_camHolder;
    public Transform T_camHook;
    public Transform T_camHookAiming;
    public Transform T_camHookCrouching;
    public Transform T_camHookArmorEquip;
    public Transform T_camHookStore;
    [HideInInspector] public Vector3 v3_camDir;
    public Vector3 V3_camOffset = new Vector3(2,0.25f,-5);
    public Vector3 V3_camOffset_Crouch = new Vector3(1f, 0.125f, -1.5f);
    private float f_camRadius = 0.15f;
    public LayerMask LM_CameraRay;
    public LayerMask LM_CameraRayVehicle;
    public LayerMask LM_GunRay;
    public LayerMask LM_AutoAimRay;

    [Header("Movement Values")]
    public float F_moveSpeed = 5;
    public float F_sprintModifier = 1.5f;
    [Space(10)]
    public float F_jumpForce = 10f;
    public float F_aerialSpeed = 10f;

    private Vector3 v3_moveDir;
    private Vector3 v3_lastNavPos;
    private Vector3 v3_modelLocalPos = Vector3.zero;
    private bool b_isMoving = false;
    private bool b_isSprinting = false;
    private bool b_isCrouching = false;
    private float f_jumpDelay = 0.3f;
    private float f_jumpTimer = 0;

    private int i_currentNavID = 0;

    private int i_humanoidNavID = 0;
    private int i_crouchingNavID = 0;


    [Header("Combat Refs")]
    public Reticle reticle;
    public RectTransform RT_hitPoint;
    public RectTransform RT_lockOnPoint;

    public autoAimClass autoAim = new autoAimClass();
    [System.Serializable]
    public class autoAimClass
    {
        public bool _active = true;
        public float F_autoAimAngle = 20f;
        [Range(0.1f, 1)] public float slowMultiplier = 0.5f;
        public Vector2 hipDriftMultiplier = Vector2.zero;
        public Vector2 aimDriftMultiplier = Vector2.zero;
    }

    private bool b_radialOpen = false;
    private Coroutine C_timeScale = null;
    private Coroutine C_interactCoyote = null;
    private Coroutine C_updateHealth = null;
    private Coroutine C_idleAnim = null;

    [HideInInspector] public BaseController BC_equippedController = null;
    [HideInInspector] public Treasure T_equippedTreasure = null;


    [Header("Inputs")]
    [HideInInspector] public inputClass Inputs = new inputClass();
    public class inputClass
    {
        public string s_inputType = "";
        public bool b_isGamepad = false;
        public Vector2 v2_inputDir;
        public Vector2 v2_camInputDir;
        public bool b_sprinting = false;
        public bool b_jumping = false;
        public bool b_firing = false;
        public bool b_aiming = false;
        public bool b_interact = false;
        public bool b_confirm = false;
        public bool b_crouch = false;
        public bool b_reload = false;
        public bool b_radial = false;
        public bool b_melee = false;
        public bool b_purchase = false;
        public bool b_recall = false;

        public PlayerInput playerInput;

        public bool IsInput()
        {
            return
                b_sprinting ||
                b_jumping ||
                b_firing ||
                b_aiming ||
                b_interact ||
                b_confirm ||
                b_crouch ||
                b_reload ||
                b_radial ||
                b_melee ||
                b_purchase ||
                b_recall;
        }


        [HideInInspector]
        public inputActions[] baseInputs = new inputActions[]
        {
            inputActions.Jump,
            inputActions.Crouch,
            inputActions.Melee,
            inputActions.Reload,
            inputActions.Sprint,
        };
        [HideInInspector]
        public inputActions[] spaceInputs = new inputActions[]
        {
            inputActions.Jump,
            inputActions.Crouch,
            inputActions.Melee,
            inputActions.Reload,
            inputActions.Sprint,
            inputActions.Recall,
        };
        [HideInInspector]
        public inputActions[] vehicleInputs = new inputActions[]
        {
            inputActions.Interact,
            inputActions.Sprint,
        };
        public string[] inputs = new string[0];
    }
    public enum inputActions
    {
        Movement,
        CamMovement,
        Sprint,
        Fire,
        Aim,
        Jump,
        Interact,
        Crouch,
        Reload,
        RadialMenu,
        Melee,
        Menu,
        Recall,
        Confirm,
        Back,
        LeftTab,
        RightTab,
        Purchase
    }

    [HideInInspector] public Interactable I_curInteractable = null;
    [HideInInspector] public BaseController BC_curBaseController = null;
    [HideInInspector] public Space_LandingSpot I_curLandingSpot = null;

    private NetworkOwnershipToggle NetworkOwner;
    #endregion

    public override void Awake()
    {
        base.Awake();
        SetNetworkOwner();
        StartCoroutine(AwakeLate());
    }

    IEnumerator AwakeLate()
    {
        while (!NetworkOwner.isFullySpawned || !LevelGen_Holder.Instance.isReady)
            yield return new WaitForEndOfFrame();
        if (NetworkOwner.isOwner)
        {
            PlayerManager.Instance.AddPlayer(info);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            SetRecallPos();
        }
    }

    public void SetRecallPos()
    {
        Transform t = NetworkManager.main.GetComponent<PlayerSpawner>().GetSpawn();
        if (t == null)
            return;
        Ref.R_recall.SetRecallPos(t);
    }

    public override void Start()
    {
        base.Start();
        SetNetworkOwner();
        Ref.R_recall.Setup(this);
        StartCoroutine(StartLate());
    }
    IEnumerator StartLate()
    {
        while (PlayerManager.main == null)
            yield return new WaitForEndOfFrame();
        if (PlayerManager.main == this)
        {
            Ref.HUI_health.Setup(this);
            Ref.GUI_gun.Setup(this);
            v3_camDir = T_camHolder.localEulerAngles;
            v3_camDir.z = 0;
            f_camDistance = V3_camOffset.magnitude;
            SetNavIDs();

            DebugGunList();
            gun_secondaryEquipped = gun_EquippedList[SaveData.i_equippedGunNum.y];
            Gun_Equip(SaveData.i_equippedGunNum.x, true);
            reticle.UpdateRoundCount(gun_Equipped);

            NMA.updateRotation = false;
            Setup_Camera();
            Setup_Consumables();
            Setup_Radial();
            Update_Objectives();


            GameState_Change(gameStateEnum.active);
            ChangeState(stateEnum.idle);

            Ref.R_recall.Activate();
        }
    }

    public void DebugGunList()
    {
        List<GunClass> _list = new List<GunClass>();
        for (int i = 0; i < SaveData.equippedGuns.Length; i++)
        {
            GunClass _temp = gunManager.GetGunByType(SaveData.equippedGuns[i], this);
            if (_temp.ownedAmt > 0)
            {
                _list.Add(_temp);
            }
        }
        gun_EquippedList = _list.ToArray();
    }

    void Setup_InteractStrings()
    {
        PlayerManager.Instance.AddPlayer(info);
        if (!Inputs.playerInput)
            Inputs.playerInput = GetComponent<PlayerInput>();

        TextMeshProUGUI _TM = Ref.TM_controlText;
        _TM.SetupInputSpriteSheet();
        Interactable.enumType _E = Interactable.enumType.input;
        List<string> _list = new List<string>();
        string _body = "";
        string[] _names = System.Enum.GetNames(typeof(inputActions));
        int mainAmt = Inputs.playerInput.actions.actionMaps[0].actions.Count;
        for (int i = 0; i < _names.Length; i++)
        {
            string _input = "".ToString_InputSetup((inputActions)i, _E);
            _list.Add(_input);
            inputActions[] _inputListed;
            switch (state)
            {
                case stateEnum.idle:
                    if (SaveData.themeCurrent == Themes.themeEnum.ship)
                        _inputListed = Inputs.spaceInputs;
                    else
                        _inputListed = Inputs.baseInputs;
                        break;
                case stateEnum.vehicle:
                    _inputListed = Inputs.vehicleInputs;
                    break;
                default:
                    _inputListed = new inputActions[0];
                    break;
            }
            if (Array.IndexOf(_inputListed, (inputActions)i) > -1)
                _body += _names[i].ToString_Interact(_input, Interactable.enumType.combineReverse) + "\n";
        }
        Inputs.inputs = _list.ToArray();
        if (_body.Length > 0)
            _body = _body.Remove(_body.Length - 1, 1);
        _TM.text = _body;
    }

    void Setup_Consumables()
    {
        if (SaveData.consumables.Count == 0)
        {
            SaveData.consumables.Add(Consumable.save.Create(Consumable_Type.Item_HealthPotion, 10));
            SaveData.consumables.Add(Consumable.save.Create(Consumable_Type.Item_RevivePotion, 1));
        }
        else
        {
            foreach (var item in SaveData.consumables)
                item._amt = item._totalAmt;
        }
    }

    void Setup_Camera()
    {
        v3_camDir.y = T_model.eulerAngles.y;
        T_camHolder.transform.rotation = Quaternion.Euler(v3_camDir);

        StartCoroutine(CanvasPivot());
    }

    public void Setup_Radial()
    {
        int[] _amt =
        {
            gun_EquippedList.Length,
            SaveData.consumables.Count,
        };
        Ref.RM_radial.Setup(_amt);
        Update_Radial();
    }

    public override void Update_Objectives()
    {
        Ref.HUD_objective.UpdateObjectives();
        base.Update_Objectives();
    }

    public override void Update_Objectives(Objective_Type _type, int _amt)
    {
        bool _affected = false;
        foreach (var item in SaveData.objectives)
        {
            if (item._type == _type)
            {
                item.amt += _amt;
                _affected = true;
                AH_agentAudioHolder.Play(AgentAudioHolder.type.objectiveTick);
            }
        }
        if (_affected)
            Update_Objectives();
    }
    public override void Update_Objectives(Objective_Type _type, Resource_Type _resource, int _amt)
    {
        bool _affected = false;
        foreach (var item in SaveData.objectives)
        {
            if (item._type == _type && item._resource == _resource)
            {
                item.amt += _amt;
                _affected = true;
                AH_agentAudioHolder.Play(AgentAudioHolder.type.objectiveTick);
            }
        }
        if (_affected)
            Update_Objectives();
    }

    void Update_Radial()
    {
        Ref.RM_radial.Setup_Guns(gun_Equipped, gun_EquippedList);
        Ref.RM_radial.Setup_Consumables(SaveData.consumables);
    }

    void SetNavIDs()
    {
        int? _temp = GetNavMeshAgentID("Humanoid");
        if (_temp != null) i_humanoidNavID = _temp.Value;
        _temp = GetNavMeshAgentID("Crouching");
        if (_temp != null) i_crouchingNavID = _temp.Value;
    }

    public override void Update()
    {
        if (PlayerManager.main != this)
            return;
        switch (GameState)
        {
            case gameStateEnum.inactive:
                AnimationEnd();
                break;
            case gameStateEnum.active:
                switch (state)
                {
                    case stateEnum.idle:
                        Update_Active();
                        break;
                    case stateEnum.ragdoll:
                        break;
                    case stateEnum.vehicle:
                        Update_Vehicle();
                        break;
                    default:
                        break;
                }
                break;
            case gameStateEnum.dialogue:
                Update_Dialogue();
                AnimationEnd();
                break;
            case gameStateEnum.dialogueResponse:
                Update_DialogueResponse();
                AnimationEnd();
                break;
            default:
                break;
        }
        Inputs.b_interact = false;
        Inputs.b_confirm = false;
        base.Update();
    }

    public override void FixedUpdate()
    {
        if (PlayerManager.main != this)
            return;
        switch (GameState)
        {
            case gameStateEnum.inactive:
                break;
            case gameStateEnum.active:
                switch (state)
                {
                    case stateEnum.idle:
                        FixedUpdate_Active();
                        break;
                    case stateEnum.ragdoll:
                        FixedUpdate_Ragdoll();
                        break;
                    case stateEnum.vehicle:
                        FixedUpdate_Vehicle();
                        break;
                    default:
                        break;
                }
                break;
            case gameStateEnum.dialogue:
                break;
            default:
                break;
        }
        base.FixedUpdate();
    }

    void Update_Active()
    {
        Movement();


        CamMovement(false);
        CamCollision(false);

        ModelRotate();
        AnimationUpdate();

        InteractHandler();

        FireManager();
        ReloadHandler();

        RadialHandler();

        if (SaveData.themeCurrent == Themes.themeEnum.ship)
            Ref.R_recall._Update();
    }

    void Update_Dialogue()
    {
        DialogueInput();
    }
    void Update_DialogueResponse()
    {
        DialogueResponseInput();
    }

    void Update_Vehicle()
    {
        V_curVehicle.OnUpdate(this);
        LandingHandler();
        CamCollision(V_curVehicle.T_camHook, V_curVehicle.V3_camOffset, false, true, false);

        if (Inputs.b_interact) V_curVehicle.OnInteract(this);
    }
    void FixedUpdate_Vehicle()
    {
        V_curVehicle.OnFixedUpdate(this);
        CamMovement(true);
    }

    void FixedUpdate_Active()
    {
        JumpHandler();
        CrouchHandler();
        AerialMovement();
    }
    void FixedUpdate_Ragdoll()
    {
        CamMovement(true);
        CamCollision(true);
    }

    void AnimationUpdate()
    {
        Vector2 _input = Inputs.v2_inputDir;
        if (b_isSprinting)
            _input.y = 2;
        if (!Inputs.b_firing && !Inputs.b_aiming)
            _input = Vector2.up * _input.magnitude;
        v2_animMove = Vector2.Lerp(v2_animMove, _input, Time.deltaTime / 0.25f);
        A_model.SetFloat("posX", v2_animMove.x);
        A_model.SetFloat("posY", v2_animMove.y);
        A_model.SetBool("Crouching", i_currentNavID == i_crouchingNavID);
        A_model.SetBool("Moving", v2_animMove.magnitude > 0.1f);
        A_model.SetBool("InAir", !b_grounded);
    }
    void AnimationEnd()
    {
        v2_animMove = Vector2.Lerp(v2_animMove, Vector2.zero, Time.deltaTime / 0.25f);
        A_model.SetFloat("posX", v2_animMove.x);
        A_model.SetFloat("posY", v2_animMove.y);
    }

    void Movement()
    {
        /*
        if (Inputs.b_sprinting && !Inputs.b_crouch)
            NMA_player.speed = F_moveSpeed * F_sprintModifier;
        else
            NMA_player.speed = F_moveSpeed;
        */
        if (Inputs.v2_inputDir.magnitude > 0.05f || b_isSprinting)
        {
            Vector3 _tarPos = new Vector3(Inputs.v2_inputDir.x, 0, Inputs.v2_inputDir.y);
            v3_moveDir = Quaternion.Euler(0, v3_camDir.y, 0) * _tarPos;
            //v3_moveDir = NMA.transform.parent.rotation * v3_moveDir;
            if (LGB_curBounds != null)
                v3_moveDir = Vector3.ProjectOnPlane(v3_moveDir, LGB_curBounds.transform.up);
            b_isMoving = true;

            if (b_grounded)
            {
                if (Inputs.b_sprinting && !Inputs.b_crouch)
                {
                    _tarPos = new Vector3(Inputs.v2_inputDir.x / 2, 0, 1);
                    v3_moveDir = Quaternion.Euler(0, v3_camDir.y, 0) * _tarPos;
                    //v3_moveDir = NMA.transform.parent.rotation * v3_moveDir;
                    v3_moveDir *= F_sprintModifier;
                    SprintStart();
                }
                else
                {
                    SprintStop();
                }

                v3_moveDir *= F_moveSpeed;

                NMA.Move(v3_moveDir * Time.deltaTime);
            }
            else
            {
                SprintStop();
            }
            if (C_idleAnim != null)
            {
                StopCoroutine(C_idleAnim);
                C_idleAnim = null;
            }
        }
        else
        {
            if (C_idleAnim == null && !Inputs.IsInput())
                C_idleAnim = StartCoroutine(Idle_Anim());
            b_isMoving = false;
            SprintStop();
        }
    }

    void SprintStart()
    {
        gun_Equipped.OnSprint(true);
        if (b_isSprinting)
            return;
        AH_agentAudioHolder.Play(AgentAudioHolder.type.sprint);
        MusicHandler.SetVolume(MusicHandler.typeEnum.drums, 1);
        b_isSprinting = true;
        Ref.speedLines.SetMaskActive(true);
    }
    void SprintStop()
    {
        gun_Equipped.OnSprint(false);
        if (!b_isSprinting)
            return;
        AH_agentAudioHolder.Stop(AgentAudioHolder.type.sprint);
        MusicHandler.SetVolume(MusicHandler.typeEnum.drums, 0);
        b_isSprinting = false;
        Ref.speedLines.SetMaskActive(false);
    }

    void AerialMovement()
    {
        if (!b_grounded && Inputs.v2_inputDir.magnitude > 0.05f)
        {
            Vector3 _tarPos = RB.position;
            _tarPos += v3_moveDir * F_aerialSpeed * Time.fixedDeltaTime;
            RB.MovePosition(_tarPos);
        }
    }

    void InteractHandler()
    {
        RaycastHit _hit;
        bool _foundObject = false;
        if (Physics.SphereCast(Camera.main.transform.position, 0.2f, Camera.main.transform.forward, out _hit, 5, LM_GunRay))
        {
            if (_hit.collider.tag == "Interactable" || _hit.collider.tag == "Vehicle")
            {
                if (C_interactCoyote != null) { StopCoroutine(C_interactCoyote); C_interactCoyote = null; }

                Interactable _interact = _hit.collider.GetComponent<Interactable>().GetInteractable();
                if (_interact != I_curInteractable)
                {
                    I_curInteractable = _interact;
                    string _temp = I_curInteractable.S_interactName.ToString_Input(inputActions.Interact, Ref.TM_interactText, _interact.I_type);
                    Ref.TM_interactText.gameObject.SetActive(true);
                    Ref.TM_interactText.text = _temp;

                    _foundObject = true;
                }
            }
            if (!_foundObject)
            {
                if (_hit.collider.gameObject.layer == 11)
                {
                    if (C_interactCoyote != null) { StopCoroutine(C_interactCoyote); C_interactCoyote = null; }

                    BaseController _bc = _hit.collider.GetComponent<HitObject>().RM_ragdollManager.controller;
                    if (_bc != BC_curBaseController && _bc.info.F_curHealth <= 0)
                    {
                        BC_curBaseController = _bc;
                        string _temp = _bc.GetName().ToString_Input(inputActions.Interact, Ref.TM_interactText, Interactable.enumType.interact);
                        Ref.TM_interactText.gameObject.SetActive(true);
                        Ref.TM_interactText.text = _temp;

                        _foundObject = true;
                    }
                }
            }
        }
        if (!_foundObject)
        {
            if (C_interactCoyote == null)
                C_interactCoyote = StartCoroutine(InteractCoyote());
        }


        if (I_curInteractable != null)
        {
            if (Inputs.b_interact)
            {
                I_curInteractable.OnInteract(this);
            }
        }
        if (BC_curBaseController != null)
        {
            if (Inputs.b_interact)
            {
                Drop();
                BC_curBaseController.PickedUp(this);
                BC_equippedController = BC_curBaseController;
            }
        }
    }

    void LandingHandler()
    {
        if (V_curVehicle.Type() == "Ship")
        {
            Ship _ship = V_curVehicle as Ship;
            if (_ship.landingSpots.Count > 0)
            {
                if (C_interactCoyote != null) { StopCoroutine(C_interactCoyote); C_interactCoyote = null; }

                Space_LandingSpot _landingSpot = _ship.landingSpots[0];
                if (_landingSpot != I_curLandingSpot)
                {
                    I_curLandingSpot = _landingSpot;
                    string _temp = _landingSpot.landingName.ToString_Input(inputActions.Jump, Ref.TM_interactText, Interactable.enumType.landing);
                    Ref.TM_interactText.gameObject.SetActive(true);
                    Ref.TM_interactText.text = _temp;
                }

                if (C_interactCoyote == null)
                    C_interactCoyote = StartCoroutine(LandingCoyote());

                if (I_curLandingSpot != null)
                {
                    if (Inputs.b_jumping)
                    {
                        I_curLandingSpot.Land(this);
                    }
                }
            }
        }
    }

    public override void Pickup_Treasure(Treasure _treasure)
    {
        Drop();
        T_equippedTreasure = _treasure;
        _treasure.OnPickUp(this);
        AH_agentAudioHolder.Play(AgentAudioHolder.type.pickup);
    }

    public void Pickup_Resource(Resource.resourceClass _resource)
    {
        Update_Objectives(Objective_Type.Collect_Resource, _resource._type, _resource.amt);

        bool _collected = false;
        foreach (var item in SaveData.resources)
        {
            if (item._type == _resource._type)
            {
                _collected = true;
                item.amt += _resource.amt;
                break;
            }
        }
        if (!_collected)
            SaveData.resources.Add(_resource.Clone());
        AH_agentAudioHolder.Play(AgentAudioHolder.type.pickupSmall);
        MusicHandler.AdjustVolume(MusicHandler.typeEnum.synth, 0.1f);
    }

    public void Pickup_Consumable(Consumable.consumableClass _consumable)
    {
        bool _collected = false;
        foreach (var item in SaveData.consumables)
        {
            if (item._type == _consumable._type)
            {
                _collected = true;
                item._amt += _consumable._amt;
                break;
            }
        }
        if (!_collected)
            SaveData.consumables.Add(_consumable.CloneToSave());
        AH_agentAudioHolder.Play(AgentAudioHolder.type.pickupSmall);
        MusicHandler.AdjustVolume(MusicHandler.typeEnum.synth, 0.1f);
    }

    IEnumerator InteractCoyote ()
    {
        yield return new WaitForSecondsRealtime(0.2f);
        Ref.TM_interactText.gameObject.SetActive(false);
        I_curInteractable = null;
        BC_curBaseController = null;
        C_interactCoyote = null;
    }
    IEnumerator LandingCoyote()
    {
        yield return new WaitForSecondsRealtime(0.2f);
        Ref.TM_interactText.gameObject.SetActive(false);
        I_curLandingSpot = null;
        C_interactCoyote = null;
    }

    void JumpHandler()
    {
        if (b_grounded)
        {
            if (Inputs.b_jumping && f_jumpTimer <= 0)
            {
                A_model.Play("Jump");
                AH_agentAudioHolder.Play(AgentAudioHolder.type.jump);
                if (C_idleAnim != null)
                {
                    StopCoroutine(C_idleAnim);
                    C_idleAnim = null;
                }
                Jump_Force();
            }
        }
        else
        {
            if (f_jumpTimer <= 0)
            {
                RaycastHit hit;
                if (Physics.SphereCast(NMA.transform.position, 0.2f, Vector3.down, out hit, 1f, LM_TerrainRay))
                {
                    GroundedUpdate(true);
                    Vector3 _modelPos = T_model.position;
                    NavMeshHit navHit;
                    if (NavMesh.SamplePosition(NMA.transform.position, out navHit, 2f, -1))
                    {
                        Vector3 _pos = navHit.position;
                        _pos.y = NMA.transform.position.y;
                        NMA.Warp(_pos);
                    }
                    StartCoroutine(MoveModel(_modelPos));
                    AH_agentAudioHolder.Play(AgentAudioHolder.type.land);
                }
            }
        }
        if (f_jumpTimer > 0)
            f_jumpTimer -= Time.fixedDeltaTime;
    }
    public void Jump_Force(float _forceMultiplier = 1)
    {
        if (b_grounded)
        {
            v3_lastNavPos = NMA.transform.position;
            GroundedUpdate(false);
        }
        RB.AddForce(Vector3.up * F_jumpForce * _forceMultiplier, ForceMode.VelocityChange);
        f_jumpTimer = f_jumpDelay;
    }
    public void Apply_Force(Vector3 _dir, float _force = -1)
    {
        if (b_grounded)
        {
            v3_lastNavPos = NMA.transform.position;
            GroundedUpdate(false);
        }

        if (_force < 0) _force = F_jumpForce;
        RB.AddForce(_dir * _force, ForceMode.Impulse);
        f_jumpTimer = f_jumpDelay;
    }
    void CrouchHandler()
    {
        if (Inputs.b_crouch && i_currentNavID != i_crouchingNavID)
        {
            NavMesh_SwitchAgentID(i_crouchingNavID);
            if (b_grounded)
            {
                b_isCrouching = true;
                AH_agentAudioHolder.Play(AgentAudioHolder.type.crouch);
                MusicHandler.Muffle(true);
            }
        }
        if (!Inputs.b_crouch && i_currentNavID != i_humanoidNavID)
        {
            if (CheckStandingRoom())
            {
                NavMesh_SwitchAgentID(i_humanoidNavID);
                T_model.localScale = Vector3.one;
                v3_modelLocalPos = Vector3.zero;
                T_model.localPosition = v3_modelLocalPos;
                b_isCrouching = false;
                AH_agentAudioHolder.Play(AgentAudioHolder.type.stand);
                MusicHandler.Muffle(false);
            }
        }
        if (!b_grounded)
            b_isCrouching = false;
    }

    void ReloadHandler()
    {
        if (Inputs.b_reload)
            gun_Equipped.OnReload();
    }

    float _radialTimer = 0;
    void RadialHandler()
    {
        if (Inputs.b_radial)
        {
            _radialTimer += Time.deltaTime;
            if (_radialTimer > 0.2f)
            {
                if (!b_radialOpen)
                {
                    Update_Radial();
                    Ref.RM_radial.Show();
                    b_radialOpen = true;
                    SetTimeScale(0.1f);
                    AH_agentAudioHolder.Play(AgentAudioHolder.type.radial);
                }
                if (Inputs.b_isGamepad)
                    Ref.RM_radial.MoveCursor_Gamepad(Inputs.v2_camInputDir);
                else
                    Ref.RM_radial.MoveCursor(Inputs.v2_camInputDir);
            }
        }
        else if (b_radialOpen)
        {
            Ref.RM_radial.Confirm();
            CloseRadial();
            _radialTimer = 0;
        }
        else if (_radialTimer > 0)
        {
            if (gun_secondaryEquipped != null)
                Gun_Equip(gun_secondaryEquipped);
            _radialTimer = 0;
        }
    }
    void CloseRadial()
    {
        Ref.RM_radial.Hide();
        b_radialOpen = false;
        SetTimeScale(1f);
        AH_agentAudioHolder.Stop(AgentAudioHolder.type.radial);
    }

    bool CheckStandingRoom()
    {
        RaycastHit hit;
        if (Physics.SphereCast(NMA.transform.position - (Local_Up() * 0.5f), 0.4f, Local_Up(), out hit, 1.2f, LM_TerrainRay))
            return false;
        return true;
    }

    void NavMesh_SwitchAgentID (int _id)
    {
        NMA.agentTypeID = _id;
        i_currentNavID = _id;
    }    

    private int? GetNavMeshAgentID(string name)
    {
        for (int i = 0; i < NavMesh.GetSettingsCount(); i++)
        {
            NavMeshBuildSettings settings = NavMesh.GetSettingsByIndex(index: i);
            if (name == NavMesh.GetSettingsNameFromID(agentTypeID: settings.agentTypeID))
                return settings.agentTypeID;
        }
        Debug.LogError("Couldn't Find NavMeshSettingID for: " + name);
        return null;
    }
    
    void ModelRotate()
    {
        Vector3 _temp = NMA.transform.eulerAngles;
        _temp.y = v3_camDir.y;
        if (Inputs.b_aiming || Inputs.b_firing)
        {
            NMA.transform.eulerAngles = _temp;
            RM_ragdoll.Aiming(true);
        }
        else
        {
            float f = Quaternion.Angle(NMA.transform.rotation, Quaternion.Euler(_temp));
            RM_ragdoll.Aiming(false, f > 90);
            if (b_isMoving)
            {
                Quaternion lookRot = Quaternion.LookRotation(v3_moveDir, Local_Up());
                NMA.transform.rotation = Quaternion.Lerp(NMA.transform.rotation, lookRot, Time.deltaTime * 4);
            }
        }
    }

    Vector3 Local_Up()
    {
        if (LGB_curBounds != null)
            return LGB_curBounds.transform.up;
        else
            return Vector3.up;
    }

    Quaternion _lastOffset = Quaternion.identity;
    void CamMovement(bool _fixedDelta = false)
    {
        if (!b_radialOpen)
        {
            float _mult = Time.deltaTime;
            if (Inputs.b_aiming) _mult *= F_camAimRotSpeed;
            else if (b_isSprinting) _mult *= F_camSprintRotSpeed;
            else _mult *= F_camRotSpeed;
            if (AutoAim()) _mult *= autoAim.slowMultiplier;
            v3_camDir += new Vector3(-Inputs.v2_camInputDir.y, Inputs.v2_camInputDir.x, 0) * _mult * f_camSensitivity;

            v3_camDir.x = Mathf.Clamp(v3_camDir.x, -80, 80);

            //Canvas Pivot
            Ref.V3_canvasRot += new Vector3(Inputs.v2_camInputDir.y, Inputs.v2_camInputDir.x, 0);
        }
        Vector3 _offset = (T_camHolder.parent.rotation * Quaternion.Inverse(_lastOffset)).eulerAngles;
        v3_camDir.y += _offset.y;
        _lastOffset = T_camHolder.parent.rotation;

        if (v3_camDir.y > 180)
        {
            v3_camDir.y -= 360;
            T_camHolder.transform.localEulerAngles += new Vector3(0, -360, 0);
        }
        if (v3_camDir.y < -180)
        {
            v3_camDir.y += 360;
            T_camHolder.transform.localEulerAngles += new Vector3(0, 360, 0);
        }

        float _delta = _fixedDelta ?
            Time.fixedDeltaTime : Time.deltaTime;

        T_camHolder.transform.rotation = Quaternion.Lerp(T_camHolder.transform.rotation, Quaternion.Euler(v3_camDir), _delta * 10);
    }

    bool AutoAim()
    {
        RaycastHit[] hits = Physics.SphereCastAll(Camera.main.transform.position + (Camera.main.transform.forward * (f_camDistance + 0.5f)), 1f, Camera.main.transform.forward, 100, LM_AutoAimRay);
        foreach (var item in hits)
        {
            Vector3 dirToTarget = (item.point - Camera.main.transform.position).normalized;
            if (Vector3.Angle(Camera.main.transform.forward, dirToTarget) < autoAim.F_autoAimAngle / 2)
            {
                float dstToTarget = Vector3.Distance(item.point, Camera.main.transform.position);
                if (!Physics.Raycast(Camera.main.transform.position, dirToTarget, dstToTarget, LM_CameraRay))
                {
                    HitObject HO;
                    if (!item.rigidbody)
                        continue;
                    if (item.rigidbody.TryGetComponent<HitObject>(out HO))
                    {
                        AgentController AC;
                        if (HO.RM_ragdollManager.GetAgentController(out AC))
                        {
                            if (AC.b_friendly || !AC.info.b_alive)
                                continue;
                        }
                        else
                            continue;
                    }
                    Vector2 _mult = Inputs.b_aiming ? autoAim.aimDriftMultiplier : autoAim.hipDriftMultiplier;
                    Vector3 dirToIntended = (item.transform.position - Camera.main.transform.position).normalized;
                    Vector3 angle = Vector3.zero;
                    if (_mult.x > 0)
                    {
                        angle.y = Mathf.Atan2(dirToIntended.x, dirToIntended.z) * Mathf.Rad2Deg;
                        float dif = angle.y - v3_camDir.y;
                        bool point = (dif > 0 && Inputs.v2_camInputDir.x >= 0) || (dif < 0 && Inputs.v2_camInputDir.x <= 0);
                        if (point)
                            v3_camDir.y = Mathf.Lerp(v3_camDir.y, angle.y, Time.deltaTime * _mult.x);
                    }
                    /*
                    if (_mult.y > 0)
                    {
                        angle.x = Mathf.Atan2(dirToIntended.y, dirToIntended.x) * Mathf.Rad2Deg;
                        //Debug.Log("Angle.x = " + angle.x.ToString() + "\nCam.x = " + v3_camDir.x);
                        v3_camDir.x = Mathf.Lerp(v3_camDir.x, angle.x, Time.deltaTime * _mult.y);
                    }
                    */
                    return true;
                }
            }
        }
        return false;
    }

    void CamCollision(bool _fixedDelta = false)
    { 
        if (b_isCrouching || b_isSprinting)
            CamCollision(T_camHookCrouching, V3_camOffset_Crouch, true, false, _fixedDelta);
        else
            CamCollision(T_camHook, V3_camOffset, true, false, _fixedDelta);
    }
    void CamCollision(Transform _camHook, Vector3 _camOffset, bool _canAim = true, bool _inVehicle = false, bool _fixedDelta = false)
    {
        float _delta = _fixedDelta ? 
            Time.fixedDeltaTime : Time.deltaTime;

        if (Inputs.b_aiming && _canAim)
        {
            gun_Equipped.OnAim(true);
            T_camHolder.position = Vector3.Slerp(T_camHolder.position, T_camHookAiming.position, _delta * F_camLatSpeed * 10);
            T_camHolder.GetChild(0).localPosition = Vector3.Slerp(T_camHolder.GetChild(0).localPosition, Vector3.zero, _delta * F_camLatSpeed);
        }
        else
        {
            gun_Equipped.OnAim(false);
            if (_inVehicle)
                T_camHolder.position = Vector3.Slerp(T_camHolder.position, _camHook.position, _delta * F_camLatSpeed);
            else
                T_camHolder.position = _camHook.position;

            LayerMask _LM = _inVehicle ?
                    LM_CameraRayVehicle : LM_CameraRay;

            RaycastHit hit;
            if (Physics.SphereCast(T_camHolder.position, f_camRadius, T_camHolder.rotation * _camOffset, out hit, _camOffset.magnitude, _LM))
            {
                f_camDistance = hit.distance;
                T_camHolder.GetChild(0).localPosition = _camOffset.normalized * f_camDistance;
            }
            else
            {
                f_camDistance = V3_camOffset.magnitude;
                T_camHolder.GetChild(0).localPosition = Vector3.Slerp(T_camHolder.GetChild(0).localPosition, _camOffset, _delta * F_camLatSpeed);
            }
        }
    }

    void FireManager()
    {
        if (!gun_Equipped)
            return;
        Update_HitPoint();
        if (Inputs.b_firing)
        {
            if (!b_isSprinting)
                gun_Equipped.OnFire();
            else
                gun_Equipped.OnSprintFire();  
        }
        else

        if (Inputs.b_melee)
        {
            gun_Equipped.OnMelee(b_isSprinting);

            Drop();
        }
        gun_Equipped.OnUpdate();
    }

    public void Drop()
    {
        DropTreasure();
        DropController();
    }

    public void DropTreasure()
    {
        if (T_equippedTreasure != null)
        {
            T_equippedTreasure.OnDrop(this, b_isSprinting);
            T_equippedTreasure = null;
            AH_agentAudioHolder.Play(AgentAudioHolder.type.throww);
        }
    }
    public void DropController()
    {
        if (BC_equippedController != null)
        {
            BC_equippedController.OnDrop(this, b_isSprinting);
            BC_equippedController = null;
            AH_agentAudioHolder.Play(AgentAudioHolder.type.throww);
        }
    }

    void Update_HitPoint()
    {
        RaycastHit hit;
        if (Physics.SphereCast(Camera.main.transform.position + (Camera.main.transform.forward * (f_camDistance + 0.5f)), 0.2f, Camera.main.transform.forward, out hit, 100, LM_GunRay))
        {
            T_aimPoint.position = hit.point;
            Vector3 _tarPos = Camera.main.WorldToScreenPoint(hit.point);
            _tarPos /= PlayerManager.conversation.C_canvas.scaleFactor;

            RT_hitPoint.anchoredPosition = _tarPos;

            HitObject HO;
            Interactable I;
            if (hit.collider.TryGetComponent<HitObject>(out HO))
            {
                if (!HO.RM_ragdollManager)
                {
                    reticle.ColorReticle(Reticle.colorEnum.none);
                    return;
                }
                if (!HO.RM_ragdollManager.controller)
                {
                    reticle.ColorReticle(Reticle.colorEnum.none);
                    return;
                }
                if (!HO.RM_ragdollManager.controller.info.b_alive)
                {
                    if (hit.distance <= 5)
                        reticle.ColorReticle(Reticle.colorEnum.interactable);
                    else
                        reticle.ColorReticle(Reticle.colorEnum.none);
                }
                else if (HO.RM_ragdollManager.controller is PlayerController)
                    reticle.ColorReticle(Reticle.colorEnum.ally);
                else if (((AgentController)HO.RM_ragdollManager.controller).b_friendly)
                    reticle.ColorReticle(Reticle.colorEnum.ally);
                else
                    reticle.ColorReticle(Reticle.colorEnum.enemy);
            }
            else if (hit.collider.TryGetComponent<Interactable>(out I) && hit.distance <= 5)
            {
                reticle.ColorReticle(Reticle.colorEnum.interactable);
            }
            else
                reticle.ColorReticle(Reticle.colorEnum.none);
        }
        else
        {
            reticle.ColorReticle(Reticle.colorEnum.none);
            RT_hitPoint.anchoredPosition = new Vector2(Screen.width / 2, Screen.height / 2) / PlayerManager.conversation.C_canvas.scaleFactor;
            T_aimPoint.position = Camera.main.transform.position + (Camera.main.transform.forward * (f_camDistance + 100.5f));
        }
    }

    void DialogueInput()
    {
        if (Inputs.b_confirm)
        {
            PlayerManager.conversation.NextLine();
        }
    }

    void DialogueResponseInput()
    {
        if (Inputs.b_confirm)
        {
            PlayerManager.conversation.ConfirmDialogueChoice();
            gun_Equipped.f_fireTimer = 0.4f;
        }

        if (Inputs.v2_inputDir.y > 0.1f)
            PlayerManager.conversation.MoveDialogueChoice(-1);
        if (Inputs.v2_inputDir.y < -0.1f)
            PlayerManager.conversation.MoveDialogueChoice(1);
    }

    public override void OnHit(GunManager.bulletClass _bullet)
    {
        if (_bullet.con_Player == this && !_bullet.D_damageType.isSelfHittable())
            return;

        if (_bullet.ragdollHitTimer > 0 && state != stateEnum.ragdoll)
            Ragdoll(_bullet.ragdollHitTimer);
        if (_bullet.pushBackForce > 0)
        {
            Vector3 dir;
            if (_bullet.B_player) dir = _bullet.con_Player.NMA.transform.forward;
            else dir = _bullet.con_Agent.NMA.transform.forward;
            dir *= _bullet.pushBackForce;
            dir += Vector3.up;
            if (owner != null)
                Push((PlayerID)owner, dir);
        }

        info.Hurt(_bullet.F_damage);

        AggroAllies(_bullet);
        AH_agentAudioHolder.Play(AgentAudioHolder.type.hurt);

        base.OnHit(_bullet);
    }

    [ObserversRpc]
    void Ragdoll(float _ragdollHitTimer)
    {
        StartCoroutine(ChangeState(state, _ragdollHitTimer));
        ChangeState(stateEnum.ragdoll);
    }
    [TargetRpc]
    void Push(PlayerID _player, Vector3 _dir)
    {
        if (state != stateEnum.ragdoll)
        {
            GroundedUpdate(false);
            RB.AddForce(_dir, ForceMode.VelocityChange);
        }
        else
            RM_ragdoll.RB_rigidbodies[0].AddForce(_dir, ForceMode.VelocityChange);
    }

    void AggroAllies(GunManager.bulletClass _bullet)
    {
        if (_bullet.B_player)
            return;
        if (!_bullet.con_Agent)
            return;
        if (_bullet.con_Agent.b_friendly)
            return;
        foreach (var item in followers)
        {
            if (item is AgentController)
            {
                AgentController AC = (AgentController)item;
                AC.AssignTarget_Attack(_bullet.con_Agent);
            }
        }
    }
    public void AggroAllies(AgentController _AC)
    {
        foreach (var item in followers)
        {
            if (item is AgentController)
            {
                AgentController AC = (AgentController)item;
                AC.AssignTarget_Attack(_AC);
            }
        }
    }
    public override void OnHeal(float _amt)
    {
        info.Heal(_amt);

        base.OnHeal(_amt);
    }
    public override void Revive()
    {
        if (PlayerManager.main != this)
            return;
        gun_Equipped.OnEquip(this);
        info.Revive();

        GameState_Change(gameStateEnum.active);
        ChangeState(stateEnum.idle);
    }

    public override void HealthUpdate()
    {
        base.HealthUpdate();
        if (PlayerManager.main != this)
            return;
        Ref.HUI_health.UpdateHealth();
        float _scale = Mathf.Clamp(Mathf.Pow((info.F_curHealth / info.F_maxHealth), 2) * 2, 0, 1);
        Ref.hurtFace.SetMaskScale(_scale, 0.05f);
    }

    public override void AddFollower(BaseController _base)
    {
        base.AddFollower(_base);
        HealthUI _GO = Instantiate(PF_followerHealth, RT_followerHealthHolder);
        FollowerHealthList.Add(_GO);
        _GO.Setup(_base);
        _base.info.F_curHealth.onChanged += delegate { UpdateFollowerHealth(_base); };
    }
    public bool CheckFollower(BaseController _base)
    {
        for (int i = 0; i < followers.Count; ++i)
        {
            if (followers[i] == _base)
            {
                return true;
            }
        }
        return false;
    }
    public override void RemoveFollower(BaseController _base)
    {
        for (int i = 0; i < followers.Count; ++i)
        {
            if (followers[i] == _base)
            {
                Destroy(FollowerHealthList[i].gameObject);
                followers.RemoveAt(i);
                FollowerHealthList.RemoveAt(i);
                break;
            }
        }
    }

    public override void UpdateFollowerHealth(BaseController _base)
    {
        for (int i = 0; i < followers.Count; ++i)
        {
            if (followers[i] == _base)
            {
                FollowerHealthList[i].UpdateHealth();
                break;
            }
        }
    }

    public void Gun_Equip(int _invNum, bool _force = false)
    {
        //Check if already equipped
        if (SaveData.i_equippedGunNum.x != _invNum || _force)
        {
            if (SaveData.i_equippedGunNum.x != _invNum)
                SaveData.i_equippedGunNum.y = SaveData.i_equippedGunNum.x;
            SaveData.i_equippedGunNum.x = _invNum;
            Gun_Equip(gun_EquippedList[_invNum]);
        }
    }

    public void Gun_Equip(GunClass _gun)
    {
        if (gun_Equipped != null)
        {
            gun_secondaryEquipped = gun_Equipped;
            gun_Equipped.OnUnEquip();
        }
        gun_Equipped = _gun;
        gun_Equipped.OnEquip(this);

        Ref.GUI_gun.Equip(gun_Equipped, gun_secondaryEquipped);

        AH_agentAudioHolder.Gun_Equip(gun_Equipped);
    }

    public void Consumable_Use(Item_Consumable _consumable)
    {
        if (_consumable.Use(this))
        {

        }
    }

    public void OnKill(AgentController AC, bool _player = true)
    {
        Update_Objectives(Objective_Type.Kill_Any, 1);
        if (_player)
            reticle.Kill();
    }

    public override void OnDeath()
    {
        ChangeState(stateEnum.ragdoll);
        Invoke(nameof(Restart), 3f);
        CloseRadial();

        if (gun_Equipped != null)
            gun_Equipped.OnUnEquip();

        Ref.speedLines.SetMaskActive(false);
        AH_agentAudioHolder.Play(AgentAudioHolder.type.death);
    }

    void Restart()
    {
        info.Restart();
    }

    void SetTimeScale(float _scale)
    {
        if (C_timeScale != null) StopCoroutine(C_timeScale);
        C_timeScale = StartCoroutine(TimeScale(_scale));
    }

    public override void EnterExit_Vehicle(bool _enter, Vehicle _vehicle)
    {
        base.EnterExit_Vehicle(_enter, _vehicle);
        Ref.speedLines.SetMaskActive(false);
    }
    public override string GetName()
    {
        return "Player";
    }

    public override void SetIcon(Texture2D _icon)
    {
        base.SetIcon(_icon);
        Ref.HUI_health.Setup(this);
    }
    public override void Weapon_Fired()
    {
        Ref.GUI_gun.UpdateClip(gun_Equipped);
    }
    
    protected override void OnDisable()
    {
        PlayerManager.Instance.RemovePlayer(info);
        base.OnDisable();
    }

    public void SetNetworkOwner()
    {
        if (NetworkOwner == null)
            NetworkOwner = GetComponent<NetworkOwnershipToggle>();
    }

    IEnumerator TimeScale (float _scale)
    {
        float _timer = 0;
        float _oldScale = TimeManager.TimeScale.value;
        while (_timer < 1)
        {
            TimeManager.TimeScale.value = Mathf.Lerp(_oldScale, _scale, _timer);
            _timer += Time.deltaTime / 0.1f;
            yield return new WaitForEndOfFrame();
        }
        TimeManager.TimeScale.value = _scale;
    }

    IEnumerator MoveModel(Vector3 startPos)
    {
        T_model.position = startPos;
        Vector3 modelPos = T_model.localPosition;
        float timer = 0;
        while (timer < 1)
        {
            T_model.localPosition = Vector3.Slerp(modelPos, v3_modelLocalPos, timer);
            timer += Time.deltaTime / 0.2f;
            yield return new WaitForEndOfFrame();
        }
        T_model.localPosition = v3_modelLocalPos;
    }

    IEnumerator CanvasPivot()
    {
        while (true)
        {
            Ref.V3_canvasRot = Vector3.ClampMagnitude(Ref.V3_canvasRot, 10);
            Ref.RT_canvasPivot.localEulerAngles = Ref.V3_canvasRot;
            Ref.RT_canvasPivot.anchoredPosition = new Vector3(Ref.V3_canvasRot.y, Ref.V3_canvasRot.x) * 3;
            Ref.V3_canvasRot = Vector3.MoveTowards(Ref.V3_canvasRot, Vector3.zero, Time.deltaTime * 20);
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator Idle_Anim(float _animWait = 20f)
    {
        while (true)
        {
            yield return new WaitForSeconds(_animWait);
            if (RB.linearVelocity.magnitude < 0.1f && !Inputs.IsInput())
            {
                A_model.SetInt("Idle", UnityEngine.Random.Range(0, 4));
                A_model.Play("Idle");
            }
        }
    }

    public override void UpdateRoom(LevelGen_Bounds _bounds, bool _enter = true)
    {
        if (!isController)
            return;
        base.UpdateRoom(_bounds, _enter);

        if (_bounds) I_curRoom = _bounds.I_roomNum;
        else I_curRoom = -1;

        if (V_curVehicle == null)
            CheckReady(AttachToBound);
    }
    public override void AttachToBound()
    {
        if (!isController) return;
        if (LGB_curBounds != null)
        {
            V3ID.value = LGB_curBounds.V3ID;

            //RB.transform.parent = LGB_curBounds.transform;
            //T_camHolder.parent = LGB_curBounds.transform;
            Transform T = LevelGen_Holder.Instance.List[V3ID.value.x].T_Holder;
            RB.transform.parent = T;
            T_camHolder.parent = T;
        }
        else
        {
            V3ID.value = Vector3Int.left;

            RB.transform.parent = transform;
            T_camHolder.parent = transform;
        }
        _lastOffset = T_camHolder.parent.rotation;
    }
    public override void AttachToBound(Transform T)
    {
        if (!isController) return;
        RB.transform.parent = T;
        T_camHolder.parent = transform;
        _lastOffset = T_camHolder.parent.rotation;
    }

    #region Input Actions
    public void GameState_Change(gameStateEnum _state)
    {
        if (PlayerManager.main != this)
            return;
        if (!Inputs.playerInput)
            Inputs.playerInput = GetComponent<PlayerInput>();
        switch (_state)
        {
            case gameStateEnum.dialogue:
                Inputs.playerInput.SwitchCurrentActionMap("Menu");
                break;
            case gameStateEnum.dialogueResponse:
                Inputs.playerInput.SwitchCurrentActionMap("Menu");
                break;
            case gameStateEnum.menu:
                Inputs.playerInput.SwitchCurrentActionMap("Menu");
                break;
            default:
                Inputs.playerInput.SwitchCurrentActionMap("Base");
                break;
        }
        GameState = _state;
        Setup_InteractStrings();
    }

    public void Input_Movement(InputAction.CallbackContext cxt) { Inputs.v2_inputDir = Input_GetVector2(cxt); }
    public void Input_CamMovement(InputAction.CallbackContext cxt) { Inputs.v2_camInputDir = Input_GetVector2(cxt); }
    public void Input_Sprint(InputAction.CallbackContext cxt) { Inputs.b_sprinting = Input_GetPressed(cxt); }
    public void Input_Fire(InputAction.CallbackContext cxt) { Inputs.b_firing = Input_GetPressed(cxt); }
    public void Input_Aim(InputAction.CallbackContext cxt) { Inputs.b_aiming = Input_GetPressed(cxt); }
    public void Input_Jump(InputAction.CallbackContext cxt) { Inputs.b_jumping = Input_GetPressed(cxt); }
    public void Input_Interact(InputAction.CallbackContext cxt) { Inputs.b_interact = Input_GetPressed(cxt); }
    public void Input_Confirm(InputAction.CallbackContext cxt) { Inputs.b_confirm = Input_GetPressed(cxt); }
    public void Input_Crouch(InputAction.CallbackContext cxt) { Inputs.b_crouch = Input_GetPressed(cxt); }
    public void Input_Reload(InputAction.CallbackContext cxt) { Inputs.b_reload = Input_GetPressed(cxt); }
    public void Input_Radial(InputAction.CallbackContext cxt) { Inputs.b_radial = Input_GetPressed(cxt); }
    public void Input_Melee(InputAction.CallbackContext cxt) { Inputs.b_melee = Input_GetPressed(cxt); }
    public void Input_Recall(InputAction.CallbackContext cxt) { Inputs.b_recall = Input_GetPressed(cxt); }
    public void Input_Menu(InputAction.CallbackContext cxt) { if (cxt.phase == InputActionPhase.Started) MainMenu.Instance.Menu_Tapped(); }

    public void Input_Purchase(InputAction.CallbackContext cxt)
    {
        Inputs.b_purchase = Input_GetPressed(cxt);
        if (cxt.phase == InputActionPhase.Started)
            if (GameState == gameStateEnum.menu)
                MainMenu.Instance.Purchase_Pressed();
    }
    public void Input_Back(InputAction.CallbackContext cxt)
    {
        if (cxt.phase == InputActionPhase.Started)
            if (GameState == gameStateEnum.menu)
                MainMenu.Instance.BackButton();
    }
    public void Input_LeftTab(InputAction.CallbackContext cxt)
    {
        if (cxt.phase == InputActionPhase.Started)
            if (GameState == gameStateEnum.menu)
                MainMenu.Instance.Tab_Switch(true);
    }
    public void Input_RightTab(InputAction.CallbackContext cxt)
    {
        if (cxt.phase == InputActionPhase.Started)
            if (GameState == gameStateEnum.menu)
                MainMenu.Instance.Tab_Switch(false);
    }

    public void Input_ChangedInput(PlayerInput input)
    {
        SetNetworkOwner();
        StartCoroutine(Input_ChangedInputLate(input));
    }

    IEnumerator Input_ChangedInputLate(PlayerInput input)
    {
        while (PlayerManager.main == null)
            yield return new WaitForEndOfFrame();
        if (PlayerManager.main == this)
        {
            Inputs.s_inputType = input.devices[0].displayName;
            Inputs.b_isGamepad = input.currentControlScheme == "Gamepad";
            Setup_InteractStrings();
            MainMenu.Instance.GamepadSwitch();
            Ref.R_recall.TextUpdate();
            Ref.GUI_gun.UpdateControl();

            UpdateSensitivity();
        }
    }

    public void UpdateSensitivity()
    {
        float _temp;
        if (Inputs.b_isGamepad) _temp = SaveData.settings.sensitivityController;
        else _temp = SaveData.settings.sensitivityMouse;
        f_camSensitivity = _temp / 50;
    }

    bool Input_GetPressed(InputAction.CallbackContext cxt)
    {
        switch (cxt.phase)
        {
            case InputActionPhase.Performed:
                return true;
            default:
                return false;
        }
    }

    Vector2 Input_GetVector2(InputAction.CallbackContext cxt)
    {
        switch (cxt.phase)
        {
            case InputActionPhase.Performed:
                return cxt.action.ReadValue<Vector2>();
            default:
                return Vector2.zero;
        }
    }

    public Vector3 GetPos()
    {
        if (b_grounded)
            return NMA.transform.position;
        else
            return v3_lastNavPos;
    }
    #endregion
}
