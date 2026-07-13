using System.Collections.Generic;
using PurrNet;
using UnityEngine;

public class BaseInfo : NetworkBehaviour
{
    [SerializeField] BaseController controller;
    public SyncVar<float> F_curHealth { get; private set; } = new(100, 0, false);
    public float F_maxHealth = 100;
    public SyncVar<bool> b_alive { get; private set; } = new(true, 0, false);

    public Gun_Type equippedGun;

    public SyncVar<Armor_Type> icon = new SyncVar<Armor_Type>(ownerAuth:true);

    public Armor_Type[] equippedArmor = new Armor_Type[]
    {
        Armor_Type.Helmet_Basic,
        Armor_Type.Chest_Basic,
        Armor_Type.Arm_Basic,
        Armor_Type.Arm_Basic,
        Armor_Type.Leg_Basic,
        Armor_Type.Material_Black
    };
    public SyncVar<GraffitiManager.graffitiClass> equippedGraffiti_Server_1 = new(null, 0, true);
    public SyncVar<GraffitiManager.graffitiClass> equippedGraffiti_Server_2 = new(null, 0, true);
    public SyncVar<GraffitiManager.graffitiClass> equippedGraffiti_Server_3 = new(null, 0, true);
    public SyncVar<GraffitiManager.graffitiClass> equippedGraffiti_Server_4 = new(null, 0, true);
    public SyncVar<GraffitiManager.graffitiClass> equippedGraffiti_Server_5 = new(null, 0, true);
    public SyncVar<GraffitiManager.graffitiClass> equippedGraffiti_Server_6 = new(null, 0, true);
    public SyncVar<GraffitiManager.graffitiClass> equippedGraffiti_Server_7 = new(null, 0, true);
    public SyncVar<GraffitiManager.graffitiClass> equippedGraffiti_Server_8 = new(null, 0, true);
    public List<GraffitiManager.graffitiClass> equippedGraffiti = new List<GraffitiManager.graffitiClass>();
    public SyncVar<List<graffitiLocation>> placedGraffiti = new(new List<graffitiLocation>(), 0, true);
    public List<Decal_Handler> placedGraffiti_Objects = new List<Decal_Handler>();
    [System.Serializable]
    public class graffitiLocation
    {
        public int _graffitiID = -1;
        public Vector3Int _V3ID;
        public Vector3 _pos;
        public Quaternion _rot;

        public graffitiLocation(int id, Vector3Int v3, Vector3 pos, Quaternion rot)
        {
            _graffitiID = id;
            _V3ID = v3;
            _pos = pos;
            _rot = rot;
        }
    }

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
                equippedArmor = SaveData.Data.equippedArmor;
                equippedGun = SaveData.Data.equippedGuns[SaveData.Data.i_equippedGunNum.x];
            }
            EquipArmor();
            EquipGun();
        }
            equippedGraffiti_Server_1.onChanged += GraffitiListUpdate_1;
            equippedGraffiti_Server_2.onChanged += GraffitiListUpdate_2;
            equippedGraffiti_Server_3.onChanged += GraffitiListUpdate_3;
            equippedGraffiti_Server_4.onChanged += GraffitiListUpdate_4;
            equippedGraffiti_Server_5.onChanged += GraffitiListUpdate_5;
            equippedGraffiti_Server_6.onChanged += GraffitiListUpdate_6;
            equippedGraffiti_Server_7.onChanged += GraffitiListUpdate_7;
            equippedGraffiti_Server_8.onChanged += GraffitiListUpdate_8;
            placedGraffiti.onChanged += PlacedGraffitiUpdate;
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
    public void AttackTarget(PlayerID? _PlayerID = null)
    {
        if (!b_alive)
            return;
        controller.AttackTarget(_PlayerID);
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
    [ServerRpc]
    public void Land(string _landingID)
    {
        SaveData.lastLandingSpot = _landingID;
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
    public void GraffitiListUpdate_Client(List<GraffitiManager.graffitiClass> _list)
    {
        if (_list.Count > 0) equippedGraffiti_Server_1.value = _list[0];
        if (_list.Count > 1) equippedGraffiti_Server_2.value = _list[1];
        if (_list.Count > 2) equippedGraffiti_Server_3.value = _list[2];
        if (_list.Count > 3) equippedGraffiti_Server_4.value = _list[3];
        if (_list.Count > 4) equippedGraffiti_Server_5.value = _list[4];
        if (_list.Count > 5) equippedGraffiti_Server_6.value = _list[5];
        if (_list.Count > 6) equippedGraffiti_Server_7.value = _list[6];
        if (_list.Count > 7) equippedGraffiti_Server_8.value = _list[7];
    }
    void GraffitiListUpdate_1(GraffitiManager.graffitiClass _temp) { GraffitiListUpdate(0); }
    void GraffitiListUpdate_2(GraffitiManager.graffitiClass _temp) { GraffitiListUpdate(1); }
    void GraffitiListUpdate_3(GraffitiManager.graffitiClass _temp) { GraffitiListUpdate(2); }
    void GraffitiListUpdate_4(GraffitiManager.graffitiClass _temp) { GraffitiListUpdate(3); }
    void GraffitiListUpdate_5(GraffitiManager.graffitiClass _temp) { GraffitiListUpdate(4); }
    void GraffitiListUpdate_6(GraffitiManager.graffitiClass _temp) { GraffitiListUpdate(5); }
    void GraffitiListUpdate_7(GraffitiManager.graffitiClass _temp) { GraffitiListUpdate(6); }
    void GraffitiListUpdate_8(GraffitiManager.graffitiClass _temp) { GraffitiListUpdate(7); }
    void GraffitiListUpdate(int _num)
    {
        if (isOwner)
            return;
        GraffitiManager.graffitiClass _temp;
        switch (_num)
        {
            case 0: _temp = equippedGraffiti_Server_1.value; break;
            case 1: _temp = equippedGraffiti_Server_2.value; break;
            case 2: _temp = equippedGraffiti_Server_3.value; break;
            case 3: _temp = equippedGraffiti_Server_4.value; break;
            case 4: _temp = equippedGraffiti_Server_5.value; break;
            case 5: _temp = equippedGraffiti_Server_6.value; break;
            case 6: _temp = equippedGraffiti_Server_7.value; break;
            case 7: _temp = equippedGraffiti_Server_8.value; break;
            default: return;
        }
        equippedGraffiti[_num] = _temp.Clone();
    }
    void PlacedGraffitiUpdate(List<graffitiLocation> _list)
    {
        if (isOwner)
            return;
        for (int i = placedGraffiti_Objects.Count; i < _list.Count; i++)
            controller.PlaceGraffiti_Server(_list[i]);
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
        icon.value = equippedArmor[0];
    }

    void EquipGun()
    {

    }
}
