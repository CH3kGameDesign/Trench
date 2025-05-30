using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEditor.Experimental.GraphView.GraphView;
using static UnityEngine.GraphicsBuffer;

public class PlayerController : BaseController
{
    public static PlayerController Instance;
    public GunManager gunManager;

    public RefClass Ref = new RefClass();
    [System.Serializable]
    public class RefClass
    {
        public RadialMenu RM_radial;
        public TextMeshProUGUI TM_interactText;
        public Slider S_healthSlider;
        public TextMeshProUGUI TM_healthText;
        public Image I_curWeapon;

        public ObjectiveHUD HUD_objective;

        public Transform T_backPivot;

        public SpeedLines speedLines;
        public SpeedLines hurtFace;
    }

    [Header ("Camera Values")]
    public float F_camLatSpeed = 1;
    public float F_camRotSpeed = 240f;
    public float F_camAimRotSpeed = 80f;
    public float F_camSprintRotSpeed = 80f;

    private float f_camDistance = 5;
    public Camera C_camera;
    public Transform T_camHolder;
    public Transform T_camHook;
    public Transform T_camHookAiming;
    public Transform T_camHookCrouching;
    public Transform T_camHookArmorEquip;
    [HideInInspector] public Vector3 v3_camDir;
    public Vector3 V3_camOffset = new Vector3(2,0.25f,-5);
    public Vector3 V3_camOffset_Crouch = new Vector3(1f, 0.125f, -1.5f);
    private float f_camRadius = 0.15f;
    public LayerMask LM_CameraRay;
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

    [Header("Animations")]
    public Animator A_model;
    private Vector2 v2_animMove = Vector2.zero;

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

    [HideInInspector] public Treasure T_equippedTreasure = null;

    [Header("Debug Variables")]
    public int[] DEBUG_EquippedGunNum = { 0, 1, 2 };

    [Header("Inputs")]
    [HideInInspector] public inputClass Inputs = new inputClass();
    public class inputClass
    {
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
                b_melee;
        }

        public string[] s_inputStrings = new string[2];
    }

    [HideInInspector] public Interactable I_curInteractable = null;
    [HideInInspector] public Space_LandingSpot I_curLandingSpot = null;


    void Awake()
    {
        Instance = this;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public override void Start()
    {
        F_curHealth = F_maxHealth;
        Ref.S_healthSlider.maxValue = F_maxHealth;
        Ref.S_healthSlider.value = F_curHealth;
        Ref.TM_healthText.text = F_curHealth.ToString();
        v3_camDir = T_camHolder.localEulerAngles;
        v3_camDir.z = 0;
        f_camDistance = V3_camOffset.magnitude;
        SetNavIDs();

        for (int i = 0; i < Mathf.Min(DEBUG_EquippedGunNum.Length, gun_EquippedList.Length); i++)
        {
            gun_EquippedList[i] = gunManager.GetGunByInt(DEBUG_EquippedGunNum[i], this);
        }
        Gun_Equip(0);
        reticle.UpdateRoundCount(gun_Equipped);

        NMA.updateRotation = false;
        Setup_Camera();
        Setup_Consumables();
        Setup_Radial();
        Setup_InteractStrings();
        Update_Objectives();

        ArmorManager.EquipArmor_Static(RM_ragdoll, SaveData.equippedArmor);

        GameState = gameStateEnum.active;
        base.Start();
    }

    void Setup_InteractStrings()
    {
        PlayerInput playerInput = GetComponent<PlayerInput>();
        string[] _temp = {
            playerInput.actions.FindAction("Interact").GetBindingDisplayString(0).Remove(0,6),
            playerInput.actions.FindAction("Interact").GetBindingDisplayString(1),
            playerInput.actions.FindAction("Jump").GetBindingDisplayString(0),
            playerInput.actions.FindAction("Jump").GetBindingDisplayString(1),
        };
        Inputs.s_inputStrings = _temp;
    }

    void Setup_Consumables()
    {
        SaveData.consumables = new List<Consumable.consumableClass>();
        SaveData.consumables.Add(Consumable.consumableClass.Create(Consumable_Type.Potion_Health, 1));
        SaveData.consumables.Add(Consumable.consumableClass.Create(Consumable_Type.Potion_Revive, 1));
    }

    void Setup_Camera()
    {
        v3_camDir.y = T_model.eulerAngles.y;
        T_camHolder.transform.rotation = Quaternion.Euler(v3_camDir);
    }

    void Setup_Radial()
    {
        Ref.RM_radial.Setup(new int[] { 3, 2, 5 });
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
        switch (GameState)
        {
            case gameStateEnum.inactive:
                AnimationEnd();
                break;
            case gameStateEnum.active:
                Update_Active();
                break;
            case gameStateEnum.dialogue:
                Update_Dialogue();
                AnimationEnd();
                break;
            case gameStateEnum.dialogueResponse:
                Update_DialogueResponse();
                AnimationEnd();
                break;
            case gameStateEnum.vehicle:
                Update_Vehicle();
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
        switch (GameState)
        {
            case gameStateEnum.inactive:
                break;
            case gameStateEnum.active:
                FixedUpdate_Active();
                break;
            case gameStateEnum.dialogue:
                break;
            case gameStateEnum.vehicle:
                FixedUpdate_Vehicle();
                break;
            default:
                break;
        }
        base.FixedUpdate();
    }

    void Update_Active()
    {
        ModelRotate();
        AnimationUpdate();

        InteractHandler();


        FireManager();
        ReloadHandler();

        RadialHandler();
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
        if (Inputs.b_interact) V_curVehicle.OnInteract(this);
    }
    void FixedUpdate_Vehicle()
    {
        CamMovement();
        CamCollision(V_curVehicle.T_camHook, V_curVehicle.V3_camOffset, true, true);
    }

    void FixedUpdate_Active()
    {
        Movement();
        JumpHandler();
        CrouchHandler();
        AerialMovement();
        CamMovement();
        CamCollision();
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
            if (T_surface != null)
                v3_moveDir = Vector3.ProjectOnPlane(v3_moveDir, T_surface.up);
            b_isMoving = true;

            if (b_grounded)
            {
                if (Inputs.b_sprinting && !Inputs.b_crouch)
                {
                    _tarPos = new Vector3(Inputs.v2_inputDir.x / 2, 0, 1);
                    v3_moveDir = Quaternion.Euler(0, v3_camDir.y, 0) * _tarPos;
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
                b_isSprinting = false;
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
        if (b_isSprinting)
            return;
        AH_agentAudioHolder.Play(AgentAudioHolder.type.sprint);
        MusicHandler.SetVolume(MusicHandler.typeEnum.drums, 1);
        b_isSprinting = true;
        Ref.speedLines.SetMaskActive(true);
    }
    void SprintStop()
    {
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
                    int _controlScheme = Inputs.b_isGamepad ? 1 : 0;
                    string _temp = I_curInteractable.S_interactName.ToString_Input(Inputs.s_inputStrings[_controlScheme], _interact.I_type);
                    Ref.TM_interactText.gameObject.SetActive(true);
                    Ref.TM_interactText.text = _temp;

                    _foundObject = true;
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
                    int _controlScheme = Inputs.b_isGamepad ? 3 : 2;
                    string _temp = _landingSpot.landingName.ToString_Input(Inputs.s_inputStrings[_controlScheme], Interactable.enumType.landing);
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
        if (T_equippedTreasure == null)
        {
            T_equippedTreasure = _treasure;
            _treasure.OnPickUp(this);
            AH_agentAudioHolder.Play(AgentAudioHolder.type.pickup);
        }    
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
                item.amt += _consumable.amt;
                break;
            }
        }
        if (!_collected)
            SaveData.consumables.Add(_consumable.Clone());
        AH_agentAudioHolder.Play(AgentAudioHolder.type.pickupSmall);
        MusicHandler.AdjustVolume(MusicHandler.typeEnum.synth, 0.1f);
    }

    IEnumerator InteractCoyote ()
    {
        yield return new WaitForSecondsRealtime(0.2f);
        Ref.TM_interactText.gameObject.SetActive(false);
        I_curInteractable = null;
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
                        T_surface_Update(NMA.navMeshOwner.GetComponent<Transform>());
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

    void RadialHandler()
    {
        if (Inputs.b_radial)
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
        else if (b_radialOpen)
        {
            Ref.RM_radial.Confirm();
            CloseRadial();
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

    void GroundedUpdate(bool _grounded)
    {
        b_grounded = _grounded;
        //NMA_player.updateRotation = _grounded;
        NMA.updatePosition = _grounded;
        //NMA_player.isStopped = !_grounded;
        RB.isKinematic = _grounded;
        RB.useGravity = !_grounded;
    }    
    
    void ModelRotate()
    {
        Vector3 _temp = NMA.transform.localEulerAngles;
        _temp.y = v3_camDir.y;
        if (Inputs.b_aiming || Inputs.b_firing)
        {
            NMA.transform.localEulerAngles = _temp;
            RM_ragdoll.Aiming(true);
        }
        else
        {
            float f = Quaternion.Angle(NMA.transform.localRotation, Quaternion.Euler(_temp));
            RM_ragdoll.Aiming(false, f > 90);
            if (b_isMoving)
            {
                Quaternion lookRot = Quaternion.LookRotation(v3_moveDir, Local_Up());
                NMA.transform.localRotation = Quaternion.Lerp(NMA.transform.localRotation, lookRot, Time.deltaTime * 4);
            }
        }
    }

    Vector3 Local_Up()
    {
        if (T_surface != null)
            return T_surface.up;
        else
            return Vector3.up;
    }

    void CamMovement()
    {
        if (!b_radialOpen)
        {
            float _mult = Time.deltaTime;
            if (Inputs.b_aiming)        _mult *= F_camAimRotSpeed;
            else if (b_isSprinting)     _mult *= F_camSprintRotSpeed;
            else                        _mult *= F_camRotSpeed;
            if (AutoAim()) _mult *= autoAim.slowMultiplier;
            v3_camDir += new Vector3(-Inputs.v2_camInputDir.y, Inputs.v2_camInputDir.x, 0) * _mult;
            if (v3_camDir.y > 180)
            {
                v3_camDir.y -= 360;
                T_camHolder.transform.eulerAngles += new Vector3(0, -360, 0);
            }
            if (v3_camDir.y < -180)
            {
                v3_camDir.y += 360;
                T_camHolder.transform.eulerAngles += new Vector3(0, 360, 0);
            }
            v3_camDir.x = Mathf.Clamp(v3_camDir.x, -80, 80);
        }
            T_camHolder.transform.rotation = Quaternion.Lerp(T_camHolder.transform.rotation, Quaternion.Euler(v3_camDir), Time.deltaTime * 10);
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
                    if (item.rigidbody.TryGetComponent<HitObject>(out HO))
                    {
                        AgentController AC;
                        if(HO.RM_ragdollManager.GetAgentController(out AC))
                        {
                            if (AC.b_friendly || !AC.b_alive)
                                continue;
                        }
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

    void CamCollision()
    { 
        if (b_isCrouching || b_isSprinting)
            CamCollision(T_camHookCrouching, V3_camOffset_Crouch);
        else
            CamCollision(T_camHook, V3_camOffset);
    }
    void CamCollision(Transform _camHook, Vector3 _camOffset, bool _canAim = true, bool _inVehicle = false)
    {
        if (Inputs.b_aiming && _canAim)
        {
            gun_Equipped.OnAim(true);
            T_camHolder.position = Vector3.Slerp(T_camHolder.position, T_camHookAiming.position, Time.deltaTime * F_camLatSpeed * 10);
            T_camHolder.GetChild(0).localPosition = Vector3.Slerp(T_camHolder.GetChild(0).localPosition, Vector3.zero, Time.deltaTime * F_camLatSpeed);
        }
        else
        {
            gun_Equipped.OnAim(false);
            T_camHolder.position = Vector3.Slerp(T_camHolder.position, _camHook.position, Time.deltaTime * F_camLatSpeed);
            if (!_inVehicle)
            {
                RaycastHit hit;
                if (Physics.SphereCast(T_camHolder.position, f_camRadius, T_camHolder.rotation * _camOffset, out hit, _camOffset.magnitude, LM_CameraRay))
                {
                    f_camDistance = hit.distance;
                    T_camHolder.GetChild(0).localPosition = _camOffset.normalized * f_camDistance;
                }
                else
                {
                    f_camDistance = V3_camOffset.magnitude;
                    T_camHolder.GetChild(0).localPosition = Vector3.Slerp(T_camHolder.GetChild(0).localPosition, _camOffset, Time.deltaTime * F_camLatSpeed);
                }
            }
            else
                T_camHolder.GetChild(0).localPosition = Vector3.Slerp(T_camHolder.GetChild(0).localPosition, _camOffset, Time.deltaTime * F_camLatSpeed);
        }
    }

    void FireManager()
    {
        Update_HitPoint();
        if (Inputs.b_firing && !b_isSprinting)
            gun_Equipped.OnFire();
        if (Inputs.b_melee)
        {
            gun_Equipped.OnMelee(b_isSprinting);

            if (T_equippedTreasure != null)
            {
                T_equippedTreasure.OnDrop(this, b_isSprinting);
                T_equippedTreasure = null;
                AH_agentAudioHolder.Play(AgentAudioHolder.type.throww);
            }
        }
        gun_Equipped.OnUpdate();
    }

    void Update_HitPoint()
    {
        RaycastHit hit;
        if (Physics.SphereCast(Camera.main.transform.position + (Camera.main.transform.forward * (f_camDistance + 0.5f)), 0.2f, Camera.main.transform.forward, out hit, 100, LM_GunRay))
        {
            T_aimPoint.position = hit.point;
            Vector3 _tarPos = Camera.main.WorldToScreenPoint(hit.point);
            _tarPos /= Conversation.Instance.C_canvas.scaleFactor;

            RT_hitPoint.anchoredPosition = _tarPos;
        }
        else
        {
            RT_hitPoint.anchoredPosition = new Vector2(Screen.width / 2, Screen.height / 2) / Conversation.Instance.C_canvas.scaleFactor;
            T_aimPoint.position = Camera.main.transform.position + (Camera.main.transform.forward * (f_camDistance + 100.5f));
        }
    }

    void DialogueInput()
    {
        if (Inputs.b_confirm)
        {
            Conversation.Instance.NextLine();
        }
    }

    void DialogueResponseInput()
    {
        if (Inputs.b_confirm)
        {
            Conversation.Instance.ConfirmDialogueChoice();
            gun_Equipped.f_fireTimer = 0.4f;
        }

        if (Inputs.v2_inputDir.y > 0.1f)
            Conversation.Instance.MoveDialogueChoice(-1);
        if (Inputs.v2_inputDir.y < -0.1f)
            Conversation.Instance.MoveDialogueChoice(1);
    }

    public override void OnHit(GunManager.bulletClass _bullet)
    {
        if (_bullet.B_player && !_bullet.D_damageType.isSelfHittable())
            return;
        F_curHealth -= _bullet.F_damage;

        if (C_updateHealth != null) StopCoroutine(C_updateHealth);
        C_updateHealth = StartCoroutine(Health_Update(F_curHealth));

        if (F_curHealth <= 0)
            OnDeath();
        else
        {
            AH_agentAudioHolder.Play(AgentAudioHolder.type.hurt);
        }
        float _scale = Mathf.Clamp(Mathf.Pow((F_curHealth / F_maxHealth), 2) * 2, 0, 1);
        Ref.hurtFace.SetMaskScale(_scale, 0.05f);
    }
    public override void OnHeal(float _amt)
    {
        F_curHealth = Mathf.Clamp(F_curHealth + _amt, 0, F_maxHealth);

        if (C_updateHealth != null) StopCoroutine(C_updateHealth);
        C_updateHealth = StartCoroutine(Health_Update(F_curHealth));

        float _scale = Mathf.Clamp(Mathf.Pow((F_curHealth / F_maxHealth), 2) * 2, 0, 1);
        Ref.hurtFace.SetMaskScale(_scale, 0.05f);

        base.OnHeal(_amt);
    }

    public void Gun_Equip(int _invNum)
    {
        if (gun_Equipped != null)
            gun_Equipped.OnUnEquip();
        gun_Equipped = gun_EquippedList[_invNum];
        gun_Equipped.OnEquip(this);
        Ref.I_curWeapon.sprite = gun_Equipped.sprite;

        AH_agentAudioHolder.Gun_Equip(gun_Equipped);
    }

    public void Consumable_Use(Consumable.consumableClass _consumable)
    {
        if (_consumable.amt <= 0)
            return;
        if (Consumable.Use(this, _consumable))
        {

        }
    }

    void OnDeath()
    {
        GameState_Change(gameStateEnum.ragdoll);
        GroundedUpdate(false);
        RM_ragdoll.EnableRigidbodies(true);
        A_model.enabled = false;
        Invoke(nameof(Restart), 3f);
        CloseRadial();

        Ref.speedLines.SetMaskActive(false);
        AH_agentAudioHolder.Play(AgentAudioHolder.type.death);
    }

    void Restart()
    {
        GameState_Change(gameStateEnum.active);
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    void SetTimeScale(float _scale)
    {
        if (C_timeScale != null) StopCoroutine(C_timeScale);
        C_timeScale = StartCoroutine(TimeScale(_scale));
    }

    public override void EnterExit_Vehicle(bool _enter, Vehicle _vehicle)
    {
        Ref.speedLines.SetMaskActive(false);
    }

    IEnumerator TimeScale (float _scale)
    {
        float _timer = 0;
        float _oldScale = Time.timeScale;
        while (_timer < 1)
        {
            Time.timeScale = Mathf.Lerp(_oldScale, _scale, _timer);
            _timer += Time.deltaTime / 0.1f;
            yield return new WaitForEndOfFrame();
        }
        Time.timeScale = _scale;
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

    IEnumerator Health_Update(float _health)
    {
        float _timer = 0;
        float _oldHealth = Ref.S_healthSlider.value;
        Ref.S_healthSlider.maxValue = F_maxHealth;
        while (_timer < 1)
        {
            Ref.S_healthSlider.value = Mathf.Lerp(_oldHealth, _health, _timer);
            Ref.TM_healthText.text = Mathf.RoundToInt(Ref.S_healthSlider.value).ToString();
            _timer += Time.deltaTime / 0.2f;
            yield return new WaitForEndOfFrame();
        }
        Ref.S_healthSlider.value = _health;
    }

    IEnumerator Idle_Anim(float _animWait = 20f)
    {
        while (true)
        {
            yield return new WaitForSeconds(_animWait);
            if (RB.linearVelocity.magnitude < 0.1f && !Inputs.IsInput())
            {
                A_model.SetInteger("Idle", Random.Range(0, 4));
                A_model.Play("Idle");
            }
        }
    }

    #region Input Actions
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
    public void Input_Menu(InputAction.CallbackContext cxt) { if (cxt.phase == InputActionPhase.Started) MainMenu.Instance.Menu_Tapped(); }
    public void Input_Back(InputAction.CallbackContext cxt)
    {
        if (cxt.phase == InputActionPhase.Started)
            if (GameState == gameStateEnum.menu)
                MainMenu.Instance.BackButton();
    }

    public void Input_ChangedInput(PlayerInput input)
    {
        Inputs.b_isGamepad = input.currentControlScheme == "Gamepad";
        MainMenu.Instance.GamepadSwitch();
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
