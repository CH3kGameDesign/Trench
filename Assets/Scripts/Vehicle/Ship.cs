using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ship : Vehicle
{
    public override string Type() { return "Ship"; }
    [HideInInspector] public Transform T_pilotSeat;
    [HideInInspector] public LevelGen LG = null;

    protected GunClass[] EquippedGuns;
    public GunClass[] DEBUG_gun;
    public Transform[] T_gunHook;

    public SS_MessageObject PF_message;
    public Sprite S_sprite;
    private SS_MessageObject _activeMessage;

    public AIVarClass AIVar;

    public enum agentBehaviourEnum { idle, wander, travel, attack }
    [System.Serializable]
    public class AIVarClass
    {
        public agentBehaviourEnum behaviourState { get; private set; } = agentBehaviourEnum.idle;

        [HideInInspector] public bool b_hasTarPosition = false;
        [HideInInspector] public BoxCollider wanderBounds = null;
        public float tarWanderDistance = 20f;
        [HideInInspector] public float wanderTimer = 0f;
        public float tarWanderTimer_Override = 15f;

        [HideInInspector] public Vector3 tarPosition;

        public void ChangeState(Ship _ship, agentBehaviourEnum _state)
        {
            LeaveState(_ship, behaviourState);
            EnterState(_ship, _state);
        }
        void EnterState(Ship _ship, agentBehaviourEnum _state)
        {
            behaviourState = _state;
            switch (_state)
            {
                case agentBehaviourEnum.wander:
                    _ship.Wander_FindNewPosition();
                    break;
                default:
                    break;
            }
        }
        void LeaveState(Ship _ship, agentBehaviourEnum _state)
        {
            b_hasTarPosition = false;
        }
    }

    //CAR SETUP

    [Space(20)]
    //[Header("CAR SETUP")]
    [Space(10)]
    [Range(100, 1000)]
    public int maxSpeed = 90; //The maximum speed that the car can reach in km/h.
    [Range(100, 1000)]
    public int maxReverseSpeed = 45; //The maximum speed that the car can reach while going on reverse in km/h.
    [Range(1, 100)]
    public int accelerationMultiplier = 2; // How fast the car can accelerate. 1 is a slow acceleration and 10 is the fastest.
    [Space(10)]
    [Range(10, 45)]
    public int maxSteeringAngle = 27; // The maximum angle that the tires can reach while rotating the steering wheel.
    [Range(0.1f, 10f)]
    public float steeringSpeed = 0.5f; // How fast the steering wheel turns.
    [Space(10)]
    [Range(-90f, 0f)]
    public float downXLimit = -40f;
    [Range(0f, 90f)]
    public float upXLimit = 40f;

    [Space(10)]
    [Range(1, 10)]
    public int brakeMultiplier = 6; // The strength of the wheel brakes.
    [Range(1, 10)]
    public int decelerationMultiplier = 2; // How fast the car decelerates when the user is not using the throttle.
    [Range(1, 10)]
    public int handbrakeDriftMultiplier = 5; // How much grip the car loses when the user hit the handbrake.
    [Space(10)]
    public Vector3 bodyMassCenter; // This is a vector that contains the center of mass of the car. I recommend to set this value
                                   // in the points x = 0 and z = 0 of your car. You can select the value that you want in the y axis,
                                   // however, you must notice that the higher this value is, the more unstable the car becomes.
                                   // Usually the y value goes from 0 to 1.5.
    [Space(10)]
    public Vector2 F_turnDeadzone = new Vector2(20,40);


    //PARTICLE SYSTEMS

    [Space(20)]
    //[Header("EFFECTS")]
    [Space(10)]
    //The following variable lets you to set up particle systems in your car
    public bool useEffects = false;

    // The following particle systems are used as tire smoke when the car drifts.
    public ParticleSystem[] RLWParticleSystem;
    public ParticleSystem[] RRWParticleSystem;

    [Space(10)]
    // The following trail renderers are used as tire skids when the car loses traction.
    public TrailRenderer[] RLWTireSkid;
    public TrailRenderer[] RRWTireSkid;

    //SPEED TEXT (UI)

    [Space(20)]
    //[Header("UI")]
    [Space(10)]
    //The following variable lets you to set up a UI text to display the speed of your car.
    public bool useUI = false;
    public Text carSpeedText; // Used to store the UI object that is going to show the speed of the car.
    public int maxThrottleBarSpeed = 180; //The maximum speed that the car can reach in km/h.

    //SOUNDS

    [Space(20)]
    //[Header("Sounds")]
    [Space(10)]
    //The following variable lets you to set up sounds for your car such as the car engine or tire screech sounds.
    public bool useSounds = false;
    public AudioSource carEngineSound; // This variable stores the sound of the car engine.
    public AudioSource tireScreechSound; // This variable stores the sound of the tire screech (when the car is drifting).
    float initialCarEngineSoundPitch; // Used to store the initial pitch of the car engine sound.

    //CONTROLS

    [Space(20)]

    //CAR DATA

    [HideInInspector]
    public float carSpeed; // Used to store the speed of the car.
    [HideInInspector]
    public bool isDrifting; // Used to know whether the car is drifting or not.
    [HideInInspector]
    public bool isTractionLocked; // Used to know whether the traction of the car is locked or not.

    //PRIVATE VARIABLES

    /*
    IMPORTANT: The following variables should not be modified manually since their values are automatically given via script.
    */
    Rigidbody carRigidbody; // Stores the car's rigidbody.
    float steeringAxis; // Used to know whether the steering wheel has reached the maximum value. It goes from -1 to 1.
    float throttleAxis; // Used to know whether the throttle has reached the maximum value. It goes from -1 to 1.
    float driftingAxis;
    float localVelocityZ;
    float localVelocityY;
    float localVelocityX;
    Quaternion tarRotation;
    float rotAngle = 0;
    bool deceleratingCar;
    bool touchControlsSetup = false;
    /*
    The following variables are used to store information about sideways friction of the wheels (such as
    extremumSlip,extremumValue, asymptoteSlip, asymptoteValue and stiffness). We change this values to
    make the car to start drifting.
    */
    WheelFrictionCurve wheelFriction;
    float WextremumSlip;

    [HideInInspector] public List<Space_LandingSpot> landingSpots = new List<Space_LandingSpot>();

    // Start is called before the first frame update
    void Start()
    {
        //In this part, we set the 'carRigidbody' value with the Rigidbody attached to this
        //gameObject. Also, we define the center of mass of the car with the Vector3 given
        //in the inspector.
        carRigidbody = gameObject.GetComponent<Rigidbody>();
        carRigidbody.centerOfMass = bodyMassCenter;
        tarRotation = transform.rotation;

        // We save the initial pitch of the car engine sound.
        if (carEngineSound != null)
        {
            initialCarEngineSoundPitch = carEngineSound.pitch;
        }

        // We invoke 2 methods inside this script. CarSpeedUI() changes the text of the UI object that stores
        // the speed of the car and CarSounds() controls the engine and drifting sounds. Both methods are invoked
        // in 0 seconds, and repeatedly called every 0.1 seconds.
        if (useUI)
        {
            InvokeRepeating("CarSpeedUI", 0f, 0.1f);
        }
        else if (!useUI)
        {
            if (carSpeedText != null)
            {
                carSpeedText.text = "0";
            }
        }

        if (useSounds)
        {
            InvokeRepeating("CarSounds", 0f, 0.1f);
        }
        else if (!useSounds)
        {
            if (carEngineSound != null)
            {
                carEngineSound.Stop();
            }
            if (tireScreechSound != null)
            {
                tireScreechSound.Stop();
            }
        }

        if (!useEffects)
        {
            foreach (var item in RLWParticleSystem) { item.Stop(); }
            foreach (var item in RRWParticleSystem) { item.Stop(); }
            foreach (var item in RLWTireSkid) { item.emitting = false; }
            foreach (var item in RRWTireSkid) { item.emitting = false; }
        }
        DEBUG_GunEquip();
        PlayerManager.Instance.CheckMain(SetupMessage);
    }
    void SetupMessage()
    {
        Canvas _canvas = PlayerManager.conversation.C_canvas;
        RectTransform _holder = PlayerManager.conversation.RT_messageHolder_Ship;
        _activeMessage = Instantiate(PF_message, _holder);
        _activeMessage.Setup(S_sprite, transform, _canvas, S_interactName, true, this);
    }

    void DEBUG_GunEquip()
    {
        int l = Mathf.Min(DEBUG_gun.Length, T_gunHook.Length);
        EquippedGuns = new GunClass[l];
        for (int i = 0; i < l; i++)
        {
            EquippedGuns[i] = DEBUG_gun[i].Clone(this);
            EquippedGuns[i].OnEquip(this, i);
        }
    }

    public override void OnUpdate_Driver(BaseController _player)
    {
        if (_player is PlayerController)
            UpdateDriver_Player(_player as PlayerController);
        else if (_player is AgentController)
            UpdateDriver_Agent(_player as AgentController);

        LevelGen_Holder.Instance.UpdateTransform(LG);
    }
    void UpdateDriver_Player(PlayerController _player)
    {
        if (_player.Inputs.v2_inputDir.y > 0)
        {
            CancelInvoke("DecelerateCar");
            deceleratingCar = false;
            float _mult = 1;
            if (_player.Inputs.b_sprinting)
            {
                _mult = 2;
                _player.Ref.speedLines.SetMaskActive(true);
            }
            else
                _player.Ref.speedLines.SetMaskActive(false);
            GoForward(_player.Inputs.v2_inputDir.y * _mult);
        }
        else
            _player.Ref.speedLines.SetMaskActive(false);
        if (_player.Inputs.v2_inputDir.y < 0)
        {
            CancelInvoke("DecelerateCar");
            deceleratingCar = false;
            GoReverse(_player.Inputs.v2_inputDir.y);
        }

        if (_player.Inputs.v2_inputDir.x < 0)
        {
            GoSideways(_player.Inputs.v2_inputDir.x);
            //Rotate(_temp.Inputs.v2_inputDir.x);
        }
        if (_player.Inputs.v2_inputDir.x > 0)
        {
            GoSideways(_player.Inputs.v2_inputDir.x);
            //Rotate(_temp.Inputs.v2_inputDir.x);
        }
        if (_player.Inputs.b_jumping)
        {
            CancelInvoke("DecelerateCar");
            deceleratingCar = false;
            Handbrake();
        }
        if (!_player.Inputs.b_jumping)
        {
            RecoverTraction();
        }
        if (_player.Inputs.v2_inputDir.y == 0)
        {
            ThrottleOff();
        }
        if (_player.Inputs.v2_inputDir.y == 0 && !_player.Inputs.b_jumping && !deceleratingCar)
        {
            InvokeRepeating("DecelerateCar", 0f, 0.1f);
            deceleratingCar = true;
        }
        if (_player.Inputs.v2_inputDir.x == 0 && steeringAxis != 0f)
        {

        }
        FireManager(_player);
    }

    void UpdateDriver_Agent(AgentController _agent)
    {
        switch (AIVar.behaviourState)
        {
            case agentBehaviourEnum.idle:
                break;
            case agentBehaviourEnum.wander:
                //Find Destination
                UpdateWander_Agent();
                //Rotate To Target
                Rotation_Agent(_agent);
                //Move Forward
                Movement_Agent(_agent);
                break;
            case agentBehaviourEnum.travel:
                break;
            case agentBehaviourEnum.attack:
                //Find Target
                UpdateTarget_Agent(_agent);
                //Rotate To Target
                Rotation_Agent(_agent);
                //Move Forward
                Movement_Agent(_agent);
                //Fire
                FireManager(_agent);
                break;
            default:
                break;
        }
    }

    void UpdateWander_Agent()
    {
        if (!AIVar.b_hasTarPosition)
            Wander_FindNewPosition();
        if (Vector3.Distance(transform.position, AIVar.tarPosition) <= AIVar.tarWanderDistance ||
            AIVar.wanderTimer > AIVar.tarWanderTimer_Override)
            Wander_FindNewPosition();

        AIVar.wanderTimer += Time.deltaTime;

        AIVar.b_hasTarPosition = true;
    }
    void Wander_FindNewPosition()
    {
        if (AIVar.wanderBounds != null)
            AIVar.tarPosition = AIVar.wanderBounds.bounds.GetRandomPoint();
        else
            AIVar.tarPosition = transform.position + (UnityEngine.Random.insideUnitSphere * 200);
        AIVar.wanderTimer = 0;
    }

    void UpdateTarget_Agent(AgentController _agent)
    {
        if (PlayerManager.main != null)
        {
            AIVar.tarPosition = PlayerManager.main.RB.transform.position;
            AIVar.b_hasTarPosition = true;
        }
    }
    void Movement_Agent(AgentController _agent)
    {
        if (!AIVar.b_hasTarPosition)
            return;
        //Go Forward
        CancelInvoke("DecelerateCar");
        deceleratingCar = false;
        float _mult = 1;
        GoForward(_mult);
        RecoverTraction();

    }
    void Rotation_Agent(AgentController _agent)
    {
        if (!AIVar.b_hasTarPosition)
            return;
        Vector3 _tarRot = transform.eulerAngles;
        _tarRot = Quaternion.LookRotation(AIVar.tarPosition - transform.position).eulerAngles;
        //_tarRot.x = Mathf.Clamp(_tarRot.x, downXLimit, upXLimit);
        Quaternion _target = Quaternion.Euler(_tarRot) * Quaternion.Euler(_rotate);
        _target *= Quaternion.Inverse(T_pilotSeat.rotation);
        _target = _target * transform.rotation;
        float _angle = Vector3.Distance(_tarRot, _curRot);

        tarRotation = _target;
        _curRot = _tarRot;

        Quaternion _rotation = Quaternion.Slerp(transform.rotation, tarRotation, Time.deltaTime * steeringSpeed);
        transform.rotation = _rotation;
    }
    void FireManager(PlayerController _player)
    {
        foreach (var item in EquippedGuns)
        {
            if (_player.Inputs.b_firing)
            {
                item.OnFire();
            }
            item.OnUpdate();
        }
    }
    void FireManager(AgentController _agent)
    {
        //Add Fire Logic
        bool _fire = true;

        foreach (var item in EquippedGuns)
        {
            if (_fire)
                item.OnFire();
            item.OnUpdate();
        }
    }

    public override void OnFixedUpdate_Driver(BaseController _player)
    {
        if (_player is PlayerController)
            FixedUpdateDriver_Player(_player as PlayerController);
        else if (_player is AgentController)
            FixedUpdateDriver_Agent(_player as AgentController);
    }
    void FixedUpdateDriver_Player(PlayerController _player)
    {
        _player.F_vehicleCamMult = Update_Rotation(_player);
        _player.UpdateVehicleUI(Vector3.Magnitude(GetLocalVelocity()), maxThrottleBarSpeed);
    }
    void FixedUpdateDriver_Agent(AgentController _agent)
    {
        switch (AIVar.behaviourState)
        {
            case agentBehaviourEnum.idle:
                break;
            case agentBehaviourEnum.wander:
                break;
            case agentBehaviourEnum.travel:
                break;
            case agentBehaviourEnum.attack:
                break;
            default:
                break;
        }
    }

    public override void OnEnter(BaseController _player)
    {
        if (useSounds && hasDriver())
            carEngineSound.Play();
        base.OnEnter(_player);

        BaseController _base;
        if (GetDriver(out _base))
        {
            if (_base == _player && _player is AgentController)
                AIVar.ChangeState(this, agentBehaviourEnum.wander);
        }

        if (_player == PlayerManager.main)
            _activeMessage.gameObject.SetActive(false);
    }
    public override void OnExit(int _num)
    {
        if (Seats[_num].BC_agent == PlayerManager.main)
            _activeMessage.gameObject.SetActive(true);

        base.OnExit(_num);
        if (useSounds && !hasDriver())
            carEngineSound.Stop();
    }
    public override Vector3 GetLocalVelocity()
    {
        return new Vector3(localVelocityX, localVelocityY, localVelocityZ);
    }
    public override Vector3 GetVelocity()
    {
        return carRigidbody.linearVelocity;
    }

    // Update is called once per frame
    public override void Update()
    {
        if (T_pilotSeat == null)
            return;
        //CAR DATA

        // We determine the speed of the car.
        carSpeed = carRigidbody.linearVelocity.magnitude;
        // Save the local velocity of the car in the x axis. Used to know if the car is drifting.
        localVelocityX = T_pilotSeat.InverseTransformDirection(carRigidbody.linearVelocity).x;
        // Save the local velocity of the car in the z axis. Used to know if the car is going forward or backwards.
        localVelocityZ = T_pilotSeat.InverseTransformDirection(carRigidbody.linearVelocity).z;
        localVelocityY = T_pilotSeat.InverseTransformDirection(carRigidbody.linearVelocity).y;

        bool _driver = false;
        for (int i = 0; i < SeatInUse.Count; i++)
            if (Seats[SeatInUse[i]].seatType == seatTypeEnum.driver)
                _driver = true;
        if (!_driver && !deceleratingCar)
        {
            ThrottleOff();
            InvokeRepeating("DecelerateCar", 0f, 0.1f);
            deceleratingCar = true;
        }


        base.Update();
    }

    Vector3 _curRot = Vector3.zero;
    float Update_Rotation(PlayerController _player)
    {
        Vector3 _tarRot = _player.v3_camDir;
        //_tarRot.x = Mathf.Clamp(_tarRot.x, downXLimit, upXLimit);
        Quaternion _target = Quaternion.Euler(_tarRot) * Quaternion.Euler(_rotate);
        _target *= Quaternion.Inverse(T_pilotSeat.rotation);
        _target =  _target * transform.rotation;
        float _angle = Vector3.Distance(_tarRot, _curRot);
        float f_turnAmount = 0;
        if (_angle > F_turnDeadzone.x)
        {
            f_turnAmount = _angle - F_turnDeadzone.x;
            f_turnAmount /= F_turnDeadzone.y - F_turnDeadzone.x;
            tarRotation = Quaternion.RotateTowards(tarRotation, _target, _angle * f_turnAmount);
            _curRot = Vector3.MoveTowards(_curRot, _tarRot, _angle * f_turnAmount);
        }

        Quaternion _rotation = Quaternion.Slerp(transform.rotation, tarRotation, Time.fixedDeltaTime * steeringSpeed);
        _player.UpdateVehicleReticle(_curRot);
        transform.rotation = _rotation;
        return f_turnAmount;
    }

    public override void RotLoop(bool yLoop, float _adjust)
    {
        if (DriverIsMain())
        {
            if (yLoop)
                _curRot.y += _adjust;
            else
                _curRot.x += _adjust;
        }
    }

    public Vector3 GetTargetPosition()
    {
        PlayerController PC;
        if (!DriverIsMain(out PC))
            return transform.position + T_pilotSeat.forward * 1000;

        Ray _ray = PC.C_camera.ScreenPointToRay(PC.Ref.RT_shipReticle.position);
        Vector3 _temp = _ray.GetPoint(1000);
        return _temp;
    }

    // This method converts the car speed data from float to string, and then set the text of the UI carSpeedText with this value.
    public void CarSpeedUI()
    {

        if (useUI)
        {
            try
            {
                float absoluteCarSpeed = Mathf.Abs(carSpeed);
                carSpeedText.text = Mathf.RoundToInt(absoluteCarSpeed).ToString();
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex);
            }
        }

    }

    // This method controls the car sounds. For example, the car engine will sound slow when the car speed is low because the
    // pitch of the sound will be at its lowest point. On the other hand, it will sound fast when the car speed is high because
    // the pitch of the sound will be the sum of the initial pitch + the car speed divided by 100f.
    // Apart from that, the tireScreechSound will play whenever the car starts drifting or losing traction.
    public void CarSounds()
    {

        if (useSounds)
        {
            try
            {
                if (carEngineSound != null)
                {
                    float engineSoundPitch = initialCarEngineSoundPitch + (Mathf.Abs(carRigidbody.linearVelocity.magnitude) / maxSpeed);
                    carEngineSound.pitch = engineSoundPitch;
                }
                if ((isDrifting) || (isTractionLocked && Mathf.Abs(carSpeed) > 12f))
                {
                    if (!tireScreechSound.isPlaying)
                    {
                        tireScreechSound.Play();
                    }
                }
                else if ((!isDrifting) && (!isTractionLocked || Mathf.Abs(carSpeed) < 12f))
                {
                    tireScreechSound.Stop();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex);
            }
        }
        else if (!useSounds)
        {
            if (carEngineSound != null && carEngineSound.isPlaying)
            {
                carEngineSound.Stop();
            }
            if (tireScreechSound != null && tireScreechSound.isPlaying)
            {
                tireScreechSound.Stop();
            }
        }

    }

    //ENGINE AND BRAKING METHODS
    //

    // This method apply positive torque to the wheels in order to go forward.
    public void GoForward(float _input)
    {
        //If the forces aplied to the rigidbody in the 'x' asis are greater than
        //3f, it means that the car is losing traction, then the car will start emitting particle systems.
        if (Mathf.Abs(localVelocityX) > 50f)
        {
            isDrifting = true;
            DriftCarPS();
        }
        else
        {
            isDrifting = false;
            DriftCarPS();
        }
        // The following part sets the throttle power to 1 smoothly.
        throttleAxis = throttleAxis + (Time.deltaTime * 3f);
        if (throttleAxis > 1f)
        {
            throttleAxis = 1f;
        }
        //If the car is going backwards, then apply brakes in order to avoid strange
        //behaviours. If the local velocity in the 'z' axis is less than -1f, then it
        //is safe to apply positive torque to go forward.
        //if (localVelocityZ < -1f)
        //{
        //    Brakes();
        //}
        //else
        //{
            if (Mathf.RoundToInt(carSpeed) < maxSpeed)
            {
                //Apply positive torque in all wheels to go forward if maxSpeed has not been reached.
                carRigidbody.AddForce(T_pilotSeat.forward *  _input * accelerationMultiplier * Time.deltaTime, ForceMode.Impulse);
            }
            else
            {
                // If the maxSpeed has been reached, then stop applying torque to the wheels.
                // IMPORTANT: The maxSpeed variable should be considered as an approximation; the speed of the car
                // could be a bit higher than expected.

            }
        //}
    }
    public void GoSideways(float _input)
    {
        //If the forces aplied to the rigidbody in the 'x' asis are greater than
        //3f, it means that the car is losing traction, then the car will start emitting particle systems.
        if (Mathf.Abs(localVelocityX) > 50f)
        {
            isDrifting = true;
            DriftCarPS();
        }
        else
        {
            isDrifting = false;
            DriftCarPS();
        }
        // The following part sets the throttle power to 1 smoothly.
        //throttleAxis = throttleAxis + (Time.deltaTime * 3f);
        //if (throttleAxis > 1f)
        //{
        //    throttleAxis = 1f;
        //}
        //If the car is going backwards, then apply brakes in order to avoid strange
        //behaviours. If the local velocity in the 'z' axis is less than -1f, then it
        //is safe to apply positive torque to go forward.
        //if (localVelocityZ < -1f)
        //{
        //    Brakes();
        //}
        //else
        //{
        if (Mathf.RoundToInt(carSpeed) < maxSpeed)
        {
            //Apply positive torque in all wheels to go forward if maxSpeed has not been reached.
            carRigidbody.AddForce(T_pilotSeat.right * _input * accelerationMultiplier * Time.deltaTime, ForceMode.Impulse);
        }
        else
        {
            // If the maxSpeed has been reached, then stop applying torque to the wheels.
            // IMPORTANT: The maxSpeed variable should be considered as an approximation; the speed of the car
            // could be a bit higher than expected.

        }
        //}
    }
    // This method apply negative torque to the wheels in order to go backwards.
    public void GoReverse(float _input)
    {
        //If the forces aplied to the rigidbody in the 'x' asis are greater than
        //3f, it means that the car is losing traction, then the car will start emitting particle systems.
        if (Mathf.Abs(localVelocityX) > 50f)
        {
            isDrifting = true;
            DriftCarPS();
        }
        else
        {
            isDrifting = false;
            DriftCarPS();
        }
        // The following part sets the throttle power to -1 smoothly.
        throttleAxis = throttleAxis - (Time.deltaTime * 3f);
        if (throttleAxis < -1f)
        {
            throttleAxis = -1f;
        }
        //If the car is still going forward, then apply brakes in order to avoid strange
        //behaviours. If the local velocity in the 'z' axis is greater than 1f, then it
        //is safe to apply negative torque to go reverse.
        //if (localVelocityZ > 1f)
       // {
       //     Brakes();
        //}
        //else
        //{
            if (Mathf.Abs(Mathf.RoundToInt(carSpeed)) < maxReverseSpeed)
            {
                //Apply negative torque in all wheels to go in reverse if maxReverseSpeed has not been reached.
                carRigidbody.AddForce(T_pilotSeat.forward * _input * accelerationMultiplier * Time.deltaTime, ForceMode.Impulse);
            }
            else
            {
                //If the maxReverseSpeed has been reached, then stop applying torque to the wheels.
                // IMPORTANT: The maxReverseSpeed variable should be considered as an approximation; the speed of the car
                // could be a bit higher than expected.

            }
        //}
    }

    private Vector3 _rotate = Vector3.zero;
    public void Rotate(float _input)
    {
        /*
        float _temp = _rotate.z - _input * 360 * steeringSpeed * Time.deltaTime;
        _rotate.z = _temp;
        */
    }

    //The following function set the motor torque to 0 (in case the user is not pressing either W or S).
    public void ThrottleOff()
    {

    }

    // The following method decelerates the speed of the car according to the decelerationMultiplier variable, where
    // 1 is the slowest and 10 is the fastest deceleration. This method is called by the function InvokeRepeating,
    // usually every 0.1f when the user is not pressing W (throttle), S (reverse) or Space bar (handbrake).
    public void DecelerateCar()
    {
        if (Mathf.Abs(localVelocityX) > 50f)
        {
            isDrifting = true;
            DriftCarPS();
        }
        else
        {
            isDrifting = false;
            DriftCarPS();
        }
        // The following part resets the throttle power to 0 smoothly.
        if (throttleAxis != 0f)
        {
            if (throttleAxis > 0f)
            {
                throttleAxis = throttleAxis - (Time.deltaTime * 10f);
            }
            else if (throttleAxis < 0f)
            {
                throttleAxis = throttleAxis + (Time.deltaTime * 10f);
            }
            if (Mathf.Abs(throttleAxis) < 0.15f)
            {
                throttleAxis = 0f;
            }
        }
        carRigidbody.linearVelocity = carRigidbody.linearVelocity * (1f / (1f + (0.025f * decelerationMultiplier)));
        // Since we want to decelerate the car, we are going to remove the torque from the wheels of the car.
        //carRigidbody.AddForce(-carRigidbody.linearVelocity, ForceMode.Impulse);
        // If the magnitude of the car's velocity is less than 0.25f (very slow velocity), then stop the car completely and
        // also cancel the invoke of this method.
        if (carRigidbody.linearVelocity.magnitude < 0.25f)
        {
            carRigidbody.linearVelocity = Vector3.zero;
            CancelInvoke("DecelerateCar");
        }
    }

    // This function applies brake torque to the wheels according to the brake force given by the user.
    public void Brakes()
    {
        carRigidbody.linearVelocity = carRigidbody.linearVelocity * (1f / (1f + (0.025f * brakeMultiplier)));
    }

    // This function is used to make the car lose traction. By using this, the car will start drifting. The amount of traction lost
    // will depend on the handbrakeDriftMultiplier variable. If this value is small, then the car will not drift too much, but if
    // it is high, then you could make the car to feel like going on ice.
    public void Handbrake()
    {
        CancelInvoke("RecoverTraction");
        // We are going to start losing traction smoothly, there is were our 'driftingAxis' variable takes
        // place. This variable will start from 0 and will reach a top value of 1, which means that the maximum
        // drifting value has been reached. It will increase smoothly by using the variable Time.deltaTime.
        driftingAxis = driftingAxis + (Time.deltaTime);
        float secureStartingPoint = driftingAxis * WextremumSlip * handbrakeDriftMultiplier;

        if (secureStartingPoint < WextremumSlip)
        {
            driftingAxis = WextremumSlip / (WextremumSlip * handbrakeDriftMultiplier);
        }
        if (driftingAxis > 1f)
        {
            driftingAxis = 1f;
        }
        //If the forces aplied to the rigidbody in the 'x' asis are greater than
        //3f, it means that the car lost its traction, then the car will start emitting particle systems.
        if (Mathf.Abs(localVelocityX) > 50f)
        {
            isDrifting = true;
        }
        else
        {
            isDrifting = false;
        }
        //If the 'driftingAxis' value is not 1f, it means that the wheels have not reach their maximum drifting
        //value, so, we are going to continue increasing the sideways friction of the wheels until driftingAxis
        // = 1f.
        if (driftingAxis < 1f)
        {

        }

        // Whenever the player uses the handbrake, it means that the wheels are locked, so we set 'isTractionLocked = true'
        // and, as a consequense, the car starts to emit trails to simulate the wheel skids.
        isTractionLocked = true;
        DriftCarPS();

    }

    // This function is used to emit both the particle systems of the tires' smoke and the trail renderers of the tire skids
    // depending on the value of the bool variables 'isDrifting' and 'isTractionLocked'.
    public void DriftCarPS()
    {

        if (useEffects)
        {
            try
            {
                if (isDrifting)
                {
                    foreach (var item in RLWParticleSystem) { item.Play(); }
                    foreach (var item in RRWParticleSystem) { item.Play(); }
                }
                else if (!isDrifting)
                {
                    foreach (var item in RLWParticleSystem) { item.Stop(); }
                    foreach (var item in RRWParticleSystem) { item.Stop(); }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex);
            }

            try
            {
                if ((isTractionLocked || Mathf.Abs(localVelocityX) > 5f) && Mathf.Abs(carSpeed) > 12f)
                {
                    foreach (var item in RLWTireSkid) { item.emitting = true; }
                    foreach (var item in RRWTireSkid) { item.emitting = true; }
                }
                else
                {
                    foreach (var item in RLWTireSkid) { item.emitting = false; }
                    foreach (var item in RRWTireSkid) { item.emitting = false; }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex);
            }
        }
        else if (!useEffects)
        {
            foreach (var item in RLWParticleSystem) { item.Stop(); }
            foreach (var item in RRWParticleSystem) { item.Stop(); }
            foreach (var item in RLWTireSkid) { item.emitting = false; }
            foreach (var item in RRWTireSkid) { item.emitting = false; }
        }

    }

    // This function is used to recover the traction of the car when the user has stopped using the car's handbrake.
    public void RecoverTraction()
    {
        isTractionLocked = false;
        driftingAxis = driftingAxis - (Time.deltaTime / 1.5f);
        if (driftingAxis < 0f)
        {
            driftingAxis = 0f;
        }

        //If the 'driftingAxis' value is not 0f, it means that the wheels have not recovered their traction.
        //We are going to continue decreasing the sideways friction of the wheels until we reach the initial
        // car's grip.
        if (wheelFriction.extremumSlip > WextremumSlip)
        {
            Invoke("RecoverTraction", Time.deltaTime);

        }
        else if (wheelFriction.extremumSlip < WextremumSlip)
        {


            driftingAxis = 0f;
        }
    }

    public void UpdateInteractText()
    {

    }

    public void OnHit(GunManager.bulletClass _bullet, DamageSource _source = null)
    {
        if (_bullet.B_player)
            AIVar.ChangeState(this, agentBehaviourEnum.attack);

        f_curHealth -= _bullet.F_damage;

        _activeMessage.UpdateHealth();

        BaseController BC;
        if (GetDriver(out BC))
            BC.UpdateVehicleHealth(this);
        if (f_curHealth < 0f)
            OnKilled();
    }
    void OnKilled()
    {
        for (int i = SeatInUse.Count - 1; i >= 0; i--)
            OnExit(SeatInUse[i]);
        _activeMessage.gameObject.SetActive(false);
        AgentLocation[] AL = GetComponentsInChildren<AgentLocation>();
        foreach (AgentLocation al in AL)
        {
            if (al.RM.isController)
                al.ClearRooms();
        }
        gameObject.SetActive(false);
    }
    public override float GetWeaponSpeed()
    {
        float _speed = 0;
        foreach (var item in EquippedGuns)
            _speed = Mathf.Max(_speed, item.bullet.B_info.F_speed);
        return _speed;
    }
}
