using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

[CreateAssetMenu(menuName = "Trench/AssetLists/Gun", fileName = "New Default Gun")]
public class GunClass : ScriptableObject
{
    public string name = "";
    public string id = "";
    [Space(10)]
    public GunPrefab prefab;
    public Bullet bullet;
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
    public Animator A_charModel;

    private GunPrefab G_gunModel = null;

    [HideInInspector]
    public bool b_playerGun = false;

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
    public virtual GunClass Clone()
    {
        GunClass _temp = Clone(CreateInstance<GunClass>());
        return _temp;
    }
    public virtual GunClass Clone(GunClass _temp)
    {
        _temp.name = name;
        _temp.id = id;

        _temp.prefab = prefab;
        _temp.bullet = bullet;
        _temp.sprite = sprite;

        _temp.fireVariables = fireVariables;
        _temp.meleeVariables = meleeVariables;
        _temp.clipVariables = clipVariables;

        _temp.audioClips = audioClips;

        _temp.clipAmmo = clipVariables.clipSize;
        _temp.reserveAmmo = clipVariables.clipSize * (clipVariables.clipAmount - 1);
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
        b_playerGun = true;
    }
    public virtual void Setup(AgentController _agent)
    {
        AC_agent = _agent;
        baseController = _agent;
        b_playerGun = false;
        A_charModel = _agent.A_model;
    }

    public virtual void OnFire()
    {
        if (f_fireTimer <= 0 && clipAmmo > 0)
        {
            Fire(fireVariables);
        }
    }
    public void Fire(fireClass _fireVariables)
    {
        i_burstRemaining = Mathf.Max(_fireVariables.burstAmount, 1);
        f_fireTimer = _fireVariables.fireRate;
        f_recoilCam = _fireVariables.recoilCam;
        f_recoilUpwards = _fireVariables.recoilUpwards;
        if (b_playerGun)
            PM_player.reticle.RotateReticle(f_fireTimer);
    }
    public virtual void OnMelee(bool _isSprinting = false)
    {
        if (f_fireTimer <= 0)
        {
            f_fireTimer = meleeVariables.fireRate;
            PM_player.reticle.UpdateRoundCount(this);
            PM_player.reticle.RotateReticle(f_fireTimer);
            A_charModel.Play("Melee_Rifle", 1);
            baseController.AH_agentAudioHolder.Play(AgentAudioHolder.type.melee);
        }
    }
    public virtual void OnReload()
    {
        if (f_fireTimer <= 0 && clipAmmo < clipVariables.clipSize)
        {
            f_fireTimer = clipVariables.reloadSpeed;
            if (b_playerGun)
                PM_player.reticle.ReloadReticle(f_fireTimer, this);
            clipAmmo = clipVariables.clipSize;
            A_charModel.Play("Reload_Rifle", 1);
            baseController.AH_agentAudioHolder.Play(AgentAudioHolder.type.reload);
        }
    }
    public virtual void OnAim(bool _aiming)
    {
        b_aiming = _aiming;
    }
    public virtual void OnEquip(BaseController baseController)
    {
        if (b_playerGun)
        {
            PM_player.reticle.UpdateRoundCount(this);
            PM_player.RT_lockOnPoint.gameObject.SetActive(false);
        }
        if (prefab != null)
        {
            G_gunModel = Instantiate(prefab, baseController.T_gunHook);
            G_gunModel.transform.localPosition = Vector3.zero;
            G_gunModel.transform.localEulerAngles = Vector3.zero;
        }
        A_charModel.Play("Equip_Rifle", 1);
        baseController.AH_agentAudioHolder.Play(AgentAudioHolder.type.equip);
    }
    public virtual void OnUnEquip()
    {
        if (G_gunModel != null)
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

            G_gunModel.Shoot();

            Vector3 _tarPos;
            Transform _barrel;
            if (b_playerGun)
            {
                _barrel = PM_player.T_barrelHook;
                Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 100, LM_GunRay))
                    _tarPos = hit.point;
                else
                    _tarPos = T_camHolder.GetChild(0).position + (T_camHolder.GetChild(0).forward * 100);
            }
            else
            {
                _barrel = AC_agent.T_barrelHook;
                _tarPos = AC_agent.V3_attackLocation;
            }
            Bullet GO = Instantiate(bullet, _barrel.position, _barrel.rotation);
            GO.transform.LookAt(_tarPos);
            if (b_playerGun)
            {
                GO.OnCreate(fireVariables.damage, PM_player, this);
                PM_player.reticle.UpdateRoundCount(this);

                T_camHolder.GetChild(0).position -= T_camHolder.forward * f_recoilCam;
                PM_player.v3_camDir.x -= f_recoilUpwards;
            }
            else
            {
                GO.OnCreate(fireVariables.damage, AC_agent, this);
            }
            OnBullet(GO);
        }
        if (f_burstTimer > 0) f_burstTimer -= Time.deltaTime;
        if (f_fireTimer > 0) f_fireTimer -= Time.deltaTime;
    }

    public virtual void OnBullet(Bullet _bullet)
    {
        A_charModel.Play("Fire_Rifle", 1);
        baseController.AH_agentAudioHolder.Play(AgentAudioHolder.type.fire);
    }

    public virtual void OnHit(Bullet _bullet)
    {

    }
}
