using PurrNet;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class BaseInfo : NetworkBehaviour
{
    [SerializeField] BaseController controller;
    public SyncVar<float> F_curHealth { get; private set; } = new(100, 0, false);
    public float F_maxHealth = 100;
    public SyncVar<bool> b_alive { get; private set; } = new(true, 0, false);

    public Gun_Type equippedGun;

    public Armor_Type[] equippedArmor = new Armor_Type[]
    {
        Armor_Type.Helmet_Basic,
        Armor_Type.Chest_Basic,
        Armor_Type.Arm_Basic,
        Armor_Type.Arm_Basic,
        Armor_Type.Leg_Basic,
        Armor_Type.Material_Black
    };

    private bool isPlayer = false;

    private void Awake()
    {
        controller = GetComponent<BaseController>();
    }

    public PlayerController GetController()
    {
        if (controller is PlayerController)
            return (PlayerController)controller;
        return null;
    }

    protected override void OnSpawned()
    {
        base.OnSpawned();
        if (controller is PlayerController)
            PlayerManager.Instance.AddPlayer(this);
        if (isController)
        {
            F_curHealth.onChanged += HealthUpdate;
            b_alive.onChanged += AliveUpdate;
            SetHealth(F_maxHealth);
            if (controller is PlayerController)
            {
                equippedArmor = SaveData.equippedArmor;
                equippedGun = SaveData.equippedGuns[SaveData.i_equippedGunNum.x];
            }
            EquipArmor();
            EquipGun();
        }
    }
    protected override void OnDestroy()
    {
        PlayerManager.Instance.RemovePlayer(this);
        F_curHealth.onChanged -= HealthUpdate;
        base.OnDestroy();
    }

    [ServerRpc]
    public void Hurt(float _amt)
    {
        if (!b_alive)
            return;
        F_curHealth.value -= _amt;
        if (F_curHealth <= 0)
        {
            b_alive.value = false;
            controller.OnDeath_Server();
        }
    }
    [ServerRpc]
    void SetHealth(float _amt) { F_curHealth.value = _amt; }
    [ServerRpc]
    public void Heal(float _amt)
    {
        F_curHealth.value = Mathf.Max(F_curHealth, 0);
        F_curHealth.value = Mathf.Min(F_curHealth + _amt, F_maxHealth);
    }
    [ServerRpc]
    public void Revive()
    {
        if (b_alive)
            return;
        b_alive.value = true;
        Heal(F_maxHealth / 2);
    }
    [ServerRpc]
    public void Restart()
    {
        foreach (var item in PlayerManager.Instance.Players)
        {
            if (item.b_alive)
                return;
        }
        LevelGen_Holder.LoadTheme(SaveData.themeCurrent);
    }

    void HealthUpdate(float _amt)
    {
        controller.HealthUpdate();
    }
    void AliveUpdate(bool _alive)
    {
        if (_alive)
            controller.Revive();
        else
            controller.OnDeath();
    }

    public void Equip(Gun_Type _gun)
    {
        equippedGun = _gun;
        EquipGun();
    }

    public void Equip(Armor_Type[] _armor)
    {
        equippedArmor = _armor;
        EquipArmor();
    }

    void EquipArmor()
    {
        ArmorManager.EquipArmor_Static(controller.RM_ragdoll, equippedArmor);
    }

    void EquipGun()
    {

    }
}
