using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PurrNet;

[CreateAssetMenu(menuName = "Trench/AssetLists/Gun", fileName = "New Default Gun")]
public class GunClass : ItemClass
{
    public override enumType GetEnumType() { return enumType.gun; }
    [Space(10)]
    public descriptionClass functionText;
    [System.Serializable]
    public class descriptionClass
    {
        [TextArea] public string primary;
        public string trigger1;
        [TextArea] public string description1;
        public string trigger2;
        [TextArea] public string description2;
        public string trigger3;
        [TextArea] public string description3;
    }
    [Space(10)]
    public GunPrefab prefab;
    public Bullet bullet;
    public Bullet melee;
    public Sprite sprite;
    [Space(10)]
    public audioClipClass audioClips = new audioClipClass();
    [Space(10)]
    public fireClass fireVariables;
    public fireClass meleeVariables;
    public clipClass clipVariables;

    public int clipAmmo = 20;
    public int reserveAmmo = 100;

    [HideInInspector]
    public bool b_aiming = false;
    [HideInInspector]
    public bool b_sprinting = false;
    [HideInInspector]
    public float f_fireTimer;

    private int i_burstRemaining;
    private float f_burstTimer;
    
    private float f_recoilCam;
    private float f_recoilUpwards;

    [HideInInspector] public LayerMask LM_GunRay;
    private Transform T_camHolder;
    [HideInInspector]
    public PlayerController PM_player;
    [HideInInspector]
    public AgentController AC_agent;
    [HideInInspector]
    public BaseController baseController;
    [HideInInspector]
    public NetworkAnimator A_charModel;
    [HideInInspector]
    public Ship shipController;
    private int i_hookNum = 0;

    private GunPrefab G_gunModel = null;

    [HideInInspector]
    public bool isPlayer = false;
    [HideInInspector]
    public gunTypeEnum equippedType = gunTypeEnum.notSetup;
    public enum gunTypeEnum { notSetup, foot, ship};

    private float _damage = 10;

    [System.Serializable]
    public class audioClipClass
    {
        public AudioClip[] fire = new AudioClip[0];
        public AudioClip[] melee = new AudioClip[0];
        public AudioClip[] reload = new AudioClip[0];
        public AudioClip[] equip = new AudioClip[0];
    }

    public GunClass Clone(PlayerController _player)
    {
        GunClass _temp = Clone();
        _temp.Setup(_player);
        return _temp;
    }
    public GunClass Clone(AgentController _agent)
    {
        GunClass _temp = Clone();
        _temp.Setup(_agent);
        return _temp;
    }
    public GunClass Clone(Ship _ship)
    {
        GunClass _temp = Clone();
        _temp.Setup(_ship);
        return _temp;
    }
    public virtual GunClass Clone()
    {
        GunClass _temp = Clone(CreateInstance<GunClass>());
        return _temp;
    }
    public virtual GunClass Clone(GunClass _temp)
    {
        _temp._name = _name;
        _temp._id = _id;

        _temp.functionText = functionText;

        _temp.prefab = prefab;
        _temp.bullet = bullet;
        _temp.melee = melee;
        _temp.sprite = sprite;

        _temp.fireVariables = fireVariables;
        _temp.meleeVariables = meleeVariables;
        _temp.clipVariables = clipVariables;

        _temp.audioClips = audioClips;

        _temp.clipAmmo = clipVariables.clipSize;
        _temp.reserveAmmo = clipVariables.clipSize * (clipVariables.clipAmount - 1);

        _temp.rarity = rarity;
        _temp.cost = cost;
        _temp.ownedAmt = ownedAmt;
        _temp.sortOrder = sortOrder;
        _temp.image = image;
        _temp._description = _description;
        return _temp;
    }

    [System.Serializable]
    public class fireClass
    {
        public float fireRate = 1;
        public float damage = 10;

        public int burstAmount = 1;
        public float burstRate = 0.1f;

        public float recoilCam = 0.25f;
        public float recoilUpwards = 1f;
        [Space(10)]
        public float bulletSprayPlayer = 0;
        public float bulletSprayAI = 15;

    }
    [System.Serializable]
    public class clipClass
    {
        public float reloadSpeed = 2;
        public int clipSize = 20;
        public int clipAmount = 5;
    }

    public virtual void Setup(PlayerController _player)
    {
        PM_player = _player;
        baseController = _player;
        A_charModel = PM_player.A_model;
        T_camHolder = _player.T_camHolder;
        LM_GunRay = _player.LM_GunRay;
        equippedType = gunTypeEnum.foot;
        isPlayer = true;
    }
    public virtual void Setup(AgentController _agent)
    {
        AC_agent = _agent;
        baseController = _agent;
        equippedType = gunTypeEnum.foot;
        A_charModel = _agent.A_model;
        isPlayer = false;
    }
    public virtual void Setup(Ship _ship)
    {
        shipController = _ship;
        equippedType = gunTypeEnum.ship;
    }

    public bool IsPlayer(out PlayerController _PC)
    {
        switch (equippedType)
        {
            case gunTypeEnum.notSetup:
                _PC = null;
                return false;
            case gunTypeEnum.ship:
                return shipController.DriverIsMain(out _PC);
            default:
                _PC = PM_player;
                return isPlayer;
        }
    }

    public virtual void OnFire()
    {
        if (f_fireTimer <= 0)
        {
            if (clipAmmo > 0)
                Fire(fireVariables);
            else
                OnReload();
        }
    }
    public virtual void OnSprintFire()
    {
        
    }
    public void Fire(fireClass _fireVariables)
    {
        G_gunModel.FireReady(false);
        _damage = _fireVariables.damage;
        i_burstRemaining = Mathf.Max(_fireVariables.burstAmount, 1);
        f_fireTimer = _fireVariables.fireRate;
        f_recoilCam = _fireVariables.recoilCam;
        f_recoilUpwards = _fireVariables.recoilUpwards;
        PlayerController _PC;
        if (IsPlayer(out _PC))
        {
            _PC.reticle().RotateReticle(f_fireTimer);
            MusicHandler.AdjustVolume(MusicHandler.typeEnum.guitar, _fireVariables.fireRate / 4);
        }
        else
            MusicHandler.AdjustVolume(MusicHandler.typeEnum.bass, _fireVariables.fireRate / 8);
    }

    public virtual void OnMelee(bool _isSprinting = false)
    {
        if (f_fireTimer <= 0)
        {
            G_gunModel.FireReady(false);
            f_fireTimer = meleeVariables.fireRate;
            A_charModel.Play("Melee_Rifle", 1);
            baseController.AH_agentAudioHolder.Play(AgentAudioHolder.type.melee);

            Transform _barrel;
            _barrel = baseController.T_barrelHook;
            Bullet GO = Instantiate(melee, _barrel.position, baseController.NMA.transform.rotation);

            PlayerController _PC;
            if (IsPlayer(out _PC))
            {
                GO.OnCreate(meleeVariables.damage, _PC, this);
                MusicHandler.AdjustVolume(MusicHandler.typeEnum.guitar, meleeVariables.fireRate / 4);
                _PC.reticle().UpdateRoundCount(this);
                _PC.reticle().RotateReticle(f_fireTimer);
            }
            else
            {
                GO.OnCreate(meleeVariables.damage, AC_agent, this);
                MusicHandler.AdjustVolume(MusicHandler.typeEnum.bass, meleeVariables.fireRate / 8);
            }
        }
    }
    public virtual void OnReload()
    {
        if (f_fireTimer <= 0 && clipAmmo < clipVariables.clipSize)
        {
            G_gunModel.FireReady(false);
            f_fireTimer = clipVariables.reloadSpeed;
            PlayerController _PC;
            if (IsPlayer(out _PC))
                _PC.reticle().ReloadReticle(f_fireTimer, this);
            clipAmmo = clipVariables.clipSize;
            switch (equippedType)
            {
                case gunTypeEnum.foot:
                    baseController.Weapon_Fired();
                    A_charModel.Play("Reload_Rifle", 1);
                    baseController.AH_agentAudioHolder.Play(AgentAudioHolder.type.reload);
                    break;
                case gunTypeEnum.ship:
                    break;
                default:
                    break;
            }
        }
    }
    public virtual void OnAim(bool _aiming)
    {
        b_aiming = _aiming;
    }
    public virtual void OnSprint(bool _sprinting)
    {
        b_sprinting = _sprinting;
    }
    public virtual void OnEquip(BaseController baseController)
    {
        PlayerController _PC;
        if (IsPlayer(out _PC))
        {
            _PC.reticle().UpdateRoundCount(this);
            _PC.RT_lockOnPoint.gameObject.SetActive(false);
        }
        if (prefab != null)
        {
            G_gunModel = Instantiate(prefab, baseController.T_gunHook);
            G_gunModel.transform.localPosition = Vector3.zero;
            G_gunModel.transform.localEulerAngles = Vector3.zero;

            baseController.RM_ragdoll.T_secondHand.position = G_gunModel.T_secondHandPoint.position;
            baseController.RM_ragdoll.T_secondElbow.position = G_gunModel.T_secondElbowPoint.position;
        }
        A_charModel.Play("Equip_Rifle", 1);
        baseController.AH_agentAudioHolder.Play(AgentAudioHolder.type.equip);
    }
    public virtual void OnEquip(Ship ship, int _gunHook)
    {
        i_hookNum = _gunHook;
        PlayerController _PC;
        if (IsPlayer(out _PC))
        {
            _PC.reticle().UpdateRoundCount(this);
            _PC.RT_lockOnPoint.gameObject.SetActive(false);
        }
        if (prefab != null)
        {
            G_gunModel = Instantiate(prefab, ship.T_gunHook[_gunHook]);
            G_gunModel.transform.localPosition = Vector3.zero;
            G_gunModel.transform.localEulerAngles = Vector3.zero;
        }
    }
    public virtual void OnUnEquip()
    {
        if (G_gunModel)
        {
            Destroy(G_gunModel.gameObject);
            G_gunModel = null;
        }
    }

    public virtual void OnUpdate()
    {
        if (clipAmmo <= 0)
            i_burstRemaining = 0;
        if (i_burstRemaining > 0 && f_burstTimer <= 0)
        {
            f_burstTimer = fireVariables.burstRate;
            i_burstRemaining--;
            clipAmmo--;
            Vector3 _tarPos;
            Transform _barrel;
            switch (equippedType)
            {
                case gunTypeEnum.notSetup:
                    return;
                case gunTypeEnum.ship:
                    _barrel = shipController.T_gunHook[i_hookNum];
                    _tarPos = shipController.transform.position + shipController.T_pilotSeat.forward * 100;
                    break;
                default:
                    baseController.Weapon_Fired();
                    _barrel = baseController.T_barrelHook;
                    _tarPos = baseController.T_aimPoint.position;
                    break;
            }

            if (G_gunModel)
                G_gunModel.Shoot();

            Bullet GO = Instantiate(bullet, _barrel.position, _barrel.rotation);
            GO.transform.LookAt(_tarPos);
            PlayerController _PC;
            if (IsPlayer(out _PC))
            {
                BulletSpray(GO, fireVariables.bulletSprayPlayer);
                GO.OnCreate(_damage, _PC, this);
                _PC.reticle().UpdateRoundCount(this);

                _PC.T_camHolder.GetChild(0).position -= _PC.T_camHolder.forward * f_recoilCam;
                _PC.v3_camDir.x -= f_recoilUpwards;
            }
            else
            {
                BulletSpray(GO, fireVariables.bulletSprayAI);
                GO.OnCreate(_damage, AC_agent, this);
            }
            OnBullet(GO);
        }
        if (f_burstTimer > 0) f_burstTimer -= Time.deltaTime;
        if (f_fireTimer > 0)
            f_fireTimer -= Time.deltaTime;
        else
            G_gunModel.FireReady(true);
    }

    void BulletSpray(Bullet GO, float _maxSpray)
    {
        Vector2 v2 = Random.insideUnitCircle;
        v2 *= Random.Range(0, _maxSpray);
        GO.transform.rotation *= Quaternion.Euler(v2.x, v2.y, 0);
    }

    public virtual void OnBullet(Bullet _bullet)
    {
        if (equippedType == gunTypeEnum.foot)
        {
            A_charModel.Play("Fire_Rifle", 1);
            baseController.AH_agentAudioHolder.Play(AgentAudioHolder.type.fire);
        }
    }

    public virtual void OnHit(Bullet _bullet)
    {

    }
    public virtual Gun_Type GetEnum()
    {
        string _string = _id.Replace('/', '_');
        if (Gun_Type.TryParse(_string, out Gun_Type _temp))
            return _temp;
        Debug.LogError("Can't find Gun_Type Enum:" + _string);
        return Gun_Type.gun_Rifle;
    }

    public override void Setup()
    {
        Gun_Type _enum = GetEnum();
        if (SaveData.ownedGun.Contains(_enum))
            ownedAmt = 1;
        else
        {
            if (cost.unlockAtStart)
            {
                ownedAmt = 1;
                SaveData.ownedGun.Add(_enum);
            }
            else
                ownedAmt = 0;
        }
    }
    public override void GenerateTexture(Camera _camera, Vector3 pos, Vector3 rot, Texture2D onEmpty = null, GameObject _model = null,
        string filePath = "Assets/Art/Sprites/Guns/")
    {
        base.GenerateTexture(_camera, pos, rot, onEmpty, prefab.gameObject, filePath);
    }
    public override void Purchase()
    {
        base.Purchase();
        Gun_Type _enum = GetEnum();
        if (!SaveData.ownedGun.Contains(_enum))
            SaveData.ownedGun.Add(_enum);
        PlayerManager.main.DebugGunList();
        PlayerManager.main.Setup_Radial();
    }

    public void Damage_Objective(int _damage = -1)
    {
        PlayerController _PC;
        if (IsPlayer(out _PC))
        {
            if (_damage < 0) _damage = Mathf.FloorToInt(fireVariables.damage);
            switch (_id)
            {
                case "gun_Rifle": _PC.Update_Objectives(Objective_Type.Damage_Rifle, _damage); break;
                case "gun_Rod": _PC.Update_Objectives(Objective_Type.Damage_Rod, _damage); break;
                case "gun_Rocket": _PC.Update_Objectives(Objective_Type.Damage_RPG, _damage); break;
                default: break;
            }
        }
    }
}
