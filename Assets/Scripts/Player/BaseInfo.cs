using PurrNet;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BaseInfo : NetworkBehaviour
{
    [SerializeField] BaseController controller;
    public float F_curHealth { get; private set; } = 100;
    public float F_maxHealth = 100;

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

    protected override void OnSpawned()
    {
        base.OnSpawned();
        isPlayer = controller is PlayerController && isOwner;
        if (isOwner)
            F_curHealth = F_maxHealth;
        if (isPlayer)
        {
            Equip(SaveData.equippedArmor);
            Equip(SaveData.equippedGuns[SaveData.i_equippedGunNum.x]);
        }
        else
        {
            EquipArmor();
            EquipGun();
        }
    }

    [ObserversRpc]
    public void Hurt(float _amt)
    {
        F_curHealth -= _amt;
    }
    [ObserversRpc]
    public void Heal(float _amt)
    {
        F_curHealth = Mathf.Max(F_curHealth, 0);
        F_curHealth = Mathf.Min(F_curHealth + _amt, F_maxHealth);
    }
    [ObserversRpc]
    public void Equip(Gun_Type _gun)
    {
        equippedGun = _gun;
        EquipGun();
    }
    [ObserversRpc]
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
