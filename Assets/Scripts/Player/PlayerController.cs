using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using TMPro;

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
    }

    [Header ("Camera Values")]
    public float F_camLatSpeed = 1;
    public float F_camRotSpeed = 120f;
    public Camera C_camera;
    public Transform T_camHolder;
    public Transform T_camHook;
    public Transform T_camHookAiming;
    [HideInInspector] public Vector3 v3_camDir;
    public Vector3 V3_camOffset = new Vector3(2,0.25f,-5);
    private float f_camRadius = 0.15f;
    public LayerMask LM_CameraRay;
    public LayerMask LM_TerrainRay;
    public LayerMask LM_GunRay;

    [Header("Movement Values")]
    public NavMeshAgent NMA_player;
    public Rigidbody RB_player;
    public Transform T_model;
    public float F_moveSpeed = 5;
    public float F_sprintModifier = 1.5f;
    [Space(10)]
    public float F_jumpForce = 10f;
    public float F_aerialSpeed = 10f;

    private Vector3 v3_moveDir;
    private Vector3 v3_lastNavPos;
    private Vector3 v3_modelLocalPos = Vector3.zero;
    private bool b_grounded = true;
    private float f_jumpDelay = 0.3f;
    private float f_jumpTimer = 0;

    private int i_currentNavID = 0;

    private int i_humanoidNavID = 0;
    private int i_crouchingNavID = 0;

    [Header("Animations")]
    public Animator A_model;
    private Vector2 v2_animMove = Vector2.zero;

    [Header("Combat Variables")]
    public float F_maxHealth = 100;
    private float f_curHealth = 100;
    public Reticle reticle;
    public RectTransform RT_hitPoint;
    public RectTransform RT_lockOnPoint;
    public Transform T_barrelHook;
    private GunClass gun_Equipped;
    private GunClass[] gun_EquippedList = new GunClass[3];

    private bool b_radialOpen = false;
    private Coroutine C_timeScale = null;
    private Coroutine C_interactCoyote = null;
    private Coroutine C_updateHealth = null;

    [Header("Debug Variables")]
    public int[] DEBUG_EquippedGunNum = { 0, 1, 2 };

    [Header("Inputs")]
    [HideInInspector] public inputClass Inputs = new inputClass();
    public class inputClass
    {
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
    }

    public static gameStateEnum GameState = gameStateEnum.active;
    public enum gameStateEnum { inactive, active, dialogue, vehicle, ragdoll}
    [HideInInspector] public Vehicle V_curVehicle = null;
    [HideInInspector] public Interactable I_curInteractable = null;

    void Awake()
    {
        Instance = this;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public override void Start()
    {
        f_curHealth = F_maxHealth;
        Ref.S_healthSlider.maxValue = F_maxHealth;
        Ref.S_healthSlider.value = f_curHealth;
        Ref.TM_healthText.text = f_curHealth.ToString();
        v3_camDir = T_camHolder.localEulerAngles;
        v3_camDir.z = 0;
        SetNavIDs();

        for (int i = 0; i < Mathf.Min(DEBUG_EquippedGunNum.Length, gun_EquippedList.Length); i++)
        {
            gun_EquippedList[i] = gunManager.GetGunByInt(DEBUG_EquippedGunNum[i], this);
        }
        Gun_Equip(0);
        reticle.UpdateRoundCount(gun_Equipped);

        NMA_player.updateRotation = false;
        Setup_Radial();
    }

    void Setup_Radial()
    {
        Ref.RM_radial.Setup(new int[] { 3, 4, 5 });
        Update_Radial();
    }

    void Update_Radial()
    {
        Ref.RM_radial.Setup_Guns(gun_Equipped, gun_EquippedList);
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
            case gameStateEnum.vehicle:
                Update_Vehicle();
                break;
            default:
                break;
        }
        Inputs.b_interact = false;
        Inputs.b_confirm = false;
    }

    private void FixedUpdate()
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
    }

    void Update_Active()
    {
        Movement();
        JumpHandler();
        CrouchHandler();
        ModelRotate();
        AnimationUpdate();

        InteractHandler();

        CamMovement();
        CamCollision();

        FireManager();
        ReloadHandler();

        RadialHandler();
    }

    void Update_Dialogue()
    {
        DialogueInput();
    }

    void Update_Vehicle()
    {
        V_curVehicle.OnUpdate(this);

        if (Inputs.b_interact) V_curVehicle.OnInteract(this);
    }
    void FixedUpdate_Vehicle()
    {
        CamMovement();
        CamCollision(V_curVehicle.T_camHook, V_curVehicle.V3_camOffset, false);
    }

    void FixedUpdate_Active()
    {
        AerialMovement();
    }

    void AnimationUpdate()
    {
        Vector2 _input = Inputs.v2_inputDir;
        if (Inputs.b_sprinting)
            _input *= 2;
        if (!Inputs.b_firing && !Inputs.b_aiming)
            _input = Vector2.up * _input.magnitude;
        v2_animMove = Vector2.Lerp(v2_animMove, _input, Time.deltaTime / 0.25f);
        A_model.SetFloat("posX", v2_animMove.x);
        A_model.SetFloat("posY", v2_animMove.y);
        A_model.SetBool("Crouching", i_currentNavID == i_crouchingNavID);
    }
    void AnimationEnd()
    {
        v2_animMove = Vector2.Lerp(v2_animMove, Vector2.zero, Time.deltaTime / 0.25f);
        A_model.SetFloat("posX", v2_animMove.x);
        A_model.SetFloat("posY", v2_animMove.y);
    }

    void Movement()
    {
        if (Inputs.b_sprinting)
            NMA_player.speed = F_moveSpeed * F_sprintModifier;
        else
            NMA_player.speed = F_moveSpeed;

        if (Inputs.v2_inputDir.magnitude > 0.05f)
        {
            Vector3 _tarPos = new Vector3(Inputs.v2_inputDir.x, 0, Inputs.v2_inputDir.y);
            v3_moveDir = Quaternion.Euler(0, v3_camDir.y, 0) * _tarPos;

            if (Inputs.b_sprinting)
                v3_moveDir *= F_sprintModifier;

            if (b_grounded)
            {
                v3_moveDir *= F_moveSpeed;

                NMA_player.Move(v3_moveDir * Time.deltaTime);
            }
        }
    }

    void AerialMovement()
    {
        if (!b_grounded && Inputs.v2_inputDir.magnitude > 0.05f)
        {
            Vector3 _tarPos = RB_player.position;
            _tarPos += v3_moveDir * F_aerialSpeed * Time.fixedDeltaTime;
            RB_player.MovePosition(_tarPos);
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
                    string _temp = "Press [0] to interact with " + I_curInteractable.S_interactName;
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

    IEnumerator InteractCoyote ()
    {
        yield return new WaitForSecondsRealtime(0.2f);
        Ref.TM_interactText.gameObject.SetActive(false);
        I_curInteractable = null;
        C_interactCoyote = null;
    }
    
    void JumpHandler()
    {
        if (b_grounded)
        {
            if (Inputs.b_jumping && f_jumpTimer <= 0)
            {
                Jump_Force();
            }
        }
        else
        {
            if (f_jumpTimer <= 0)
            {
                RaycastHit hit;
                if (Physics.SphereCast(NMA_player.transform.position, 0.2f, Vector3.down, out hit, 1f, LM_TerrainRay))
                {
                    GroundedUpdate(true);
                    Vector3 _modelPos = T_model.position;
                    NavMeshHit navHit;
                    if (NavMesh.SamplePosition(NMA_player.transform.position, out navHit, 2f, -1))
                    {
                        Vector3 _pos = navHit.position;
                        _pos.y = NMA_player.transform.position.y;
                        NMA_player.Warp(_pos);
                    }
                    StartCoroutine(MoveModel(_modelPos));
                }
            }
        }
        if (f_jumpTimer > 0)
            f_jumpTimer -= Time.deltaTime;
    }
    public void Jump_Force(float _forceMultiplier = 1)
    {
        if (b_grounded)
        {
            v3_lastNavPos = NMA_player.transform.position;
            GroundedUpdate(false);
        }
        RB_player.AddForce(Vector3.up * F_jumpForce * _forceMultiplier, ForceMode.VelocityChange);
        f_jumpTimer = f_jumpDelay;
    }
    void CrouchHandler()
    {
        if (Inputs.b_crouch && i_currentNavID != i_crouchingNavID)
        {
            NavMesh_SwitchAgentID(i_crouchingNavID);
            //T_model.localScale = new Vector3(1, 0.5f, 1);
            //v3_modelLocalPos = new Vector3(0, -0.5f, 0);
            //T_model.localPosition = v3_modelLocalPos;
            //RB_player.GetComponent<CapsuleCollider>().height = 0.6666f;
            //RB_player.GetComponent<CapsuleCollider>().center = new Vector3(0,-0.6f,0);
        }
        if (!Inputs.b_crouch && i_currentNavID != i_humanoidNavID)
        {
            if (CheckStandingRoom())
            {
                NavMesh_SwitchAgentID(i_humanoidNavID);
                T_model.localScale = Vector3.one;
                v3_modelLocalPos = Vector3.zero;
                T_model.localPosition = v3_modelLocalPos;
                //RB_player.GetComponent<CapsuleCollider>().height = 2;
                //RB_player.GetComponent<CapsuleCollider>().center = Vector3.zero;
            }
        }
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
            }
            Ref.RM_radial.MoveCursor(Inputs.v2_camInputDir);
        }
        else if (b_radialOpen)
        {
            Ref.RM_radial.Hide();
            Ref.RM_radial.Confirm();
            b_radialOpen = false;
            SetTimeScale(1f);
        }
    }

    bool CheckStandingRoom()
    {
        RaycastHit hit;
        if (Physics.SphereCast(NMA_player.transform.position - (Vector3.up * 0.5f), 0.4f, Vector3.up, out hit, 1.2f, LM_TerrainRay))
            return false;
        return true;
    }

    void NavMesh_SwitchAgentID (int _id)
    {
        NMA_player.agentTypeID = _id;
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
        NMA_player.updatePosition = _grounded;
        //NMA_player.isStopped = !_grounded;
        RB_player.isKinematic = _grounded;
        RB_player.useGravity = !_grounded;
    }    
    
    void ModelRotate()
    {
        if (Inputs.b_aiming || Inputs.b_firing)
        {
            NMA_player.transform.eulerAngles = new Vector3(0, T_camHolder.eulerAngles.y, 0);
        }
        else if (Inputs.v2_inputDir.magnitude > 0.05f)
        {
            Quaternion lookRot = Quaternion.LookRotation(v3_moveDir, Vector3.up);
            NMA_player.transform.localRotation = Quaternion.Lerp(NMA_player.transform.localRotation, lookRot, Time.deltaTime * 4);
        }
    }

    void CamMovement()
    {
        if (!b_radialOpen)
        {
            v3_camDir += new Vector3(-Inputs.v2_camInputDir.y, Inputs.v2_camInputDir.x, 0) * F_camRotSpeed * Time.deltaTime;
            v3_camDir.x = Mathf.Clamp(v3_camDir.x, -80, 80);
        }
        T_camHolder.transform.rotation = Quaternion.Slerp(T_camHolder.transform.rotation, Quaternion.Euler(v3_camDir), Time.deltaTime * 10);
    }

    void CamCollision() { CamCollision(T_camHook, V3_camOffset); }
    void CamCollision(Transform _camHook, Vector3 _camOffset, bool _canAim = true)
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
            RaycastHit hit;
            if (Physics.SphereCast(T_camHolder.position, f_camRadius, T_camHolder.rotation * _camOffset, out hit, _camOffset.magnitude, LM_CameraRay))
                T_camHolder.GetChild(0).localPosition = _camOffset.normalized * hit.distance;
            else
                T_camHolder.GetChild(0).localPosition = Vector3.Slerp(T_camHolder.GetChild(0).localPosition, _camOffset, Time.deltaTime * F_camLatSpeed);
        }
    }

    void FireManager()
    {
        Update_HitPoint();
        if (Inputs.b_firing)
            gun_Equipped.OnFire();
        if (Inputs.b_melee)
            gun_Equipped.OnMelee();
        gun_Equipped.OnUpdate();
    }

    void Update_HitPoint()
    {
        RaycastHit hit;
        if (Physics.SphereCast(Camera.main.transform.position, 0.2f, Camera.main.transform.forward, out hit, 100, LM_GunRay))
        {
            Vector3 _tarPos = Camera.main.WorldToScreenPoint(hit.point);
            _tarPos /= Conversation.Instance.C_canvas.scaleFactor;

            RT_hitPoint.anchoredPosition = _tarPos;
        }
        else
            RT_hitPoint.anchoredPosition = new Vector2(Screen.width / 2, Screen.height / 2) / Conversation.Instance.C_canvas.scaleFactor;
    }

    void DialogueInput()
    {
        if (Inputs.b_confirm)
        {
            Conversation.Instance.NextLine();
        }
    }

    public override void OnHit(GunManager.bulletClass _bullet)
    {
        f_curHealth -= _bullet.F_damage;

        if (C_updateHealth != null) StopCoroutine(C_updateHealth);
        C_updateHealth = StartCoroutine(Health_Update(f_curHealth));

        if (f_curHealth <= 0)
            OnDeath();
    }

    public void Gun_Equip(int _invNum)
    {
        if (gun_Equipped != null)
            gun_Equipped.OnUnEquip();
        gun_Equipped = gun_EquippedList[_invNum];
        gun_Equipped.OnEquip();
        Ref.I_curWeapon.sprite = gun_Equipped.sprite;
    }

    void OnDeath()
    {
        GameState = gameStateEnum.ragdoll;
        GroundedUpdate(false);
        RM_ragdoll.EnableRigidbodies(true);
        A_model.enabled = false;
        Invoke(nameof(Restart), 3f);
    }

    void Restart()
    {
        GameState = gameStateEnum.active;
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    void SetTimeScale(float _scale)
    {
        if (C_timeScale != null) StopCoroutine(C_timeScale);
        C_timeScale = StartCoroutine(TimeScale(_scale));
    }

    public void GameState_Change(gameStateEnum _state)
    {
        GameState = _state;
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
            return NMA_player.transform.position;
        else
            return v3_lastNavPos;
    }
    #endregion
}
