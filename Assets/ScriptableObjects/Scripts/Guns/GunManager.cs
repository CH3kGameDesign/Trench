using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Trench/AssetLists/GunManager", fileName = "New Gun Manager")]
public class GunManager : ScriptableObject
{
    public List<GunClass> list = new List<GunClass>();



    [System.Serializable]
    public class bulletClass
    {
        public bool B_player = false;
        public PlayerController con_Player;
        public AgentController con_Agent;
        public GunClass con_Gun;

        public float F_speed = 100f;
        public float F_radius = 0.1f;
        public float F_lifeTime = 30f;

        public float F_lockOnTurnSpeed = 60f;

        public GameObject PF_impactHit;

        public float F_damage = 0;

        public HitObject.damageTypeEnum D_damageType = HitObject.damageTypeEnum.bullet;

        public bulletClass Clone()
        {
            bulletClass _temp = new bulletClass();
            _temp.B_player = B_player;
            _temp.con_Player = con_Player;
            _temp.con_Agent = con_Agent;
            _temp.con_Gun = con_Gun;

            _temp.F_speed = F_speed;
            _temp.F_radius = F_radius;
            _temp.F_lifeTime = F_lifeTime;

            _temp.F_lockOnTurnSpeed = F_lockOnTurnSpeed;
            _temp.PF_impactHit = PF_impactHit;
            _temp.F_damage = F_damage;
            _temp.D_damageType = D_damageType;
            return _temp;
        }
    }

    public GunClass GetGunByID(string _id, PlayerController _player)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i]._id == _id)
                return list[i].Clone(_player);
        }
        Debug.LogError("Couldn't find gun of id: " + _id);
        return null;
    }
    public GunClass GetGunByInt(int _i, PlayerController _player)
    {
        if (_i >= list.Count)
        {
            Debug.LogError("Requested Int was out of range: " + _i);
            return null;
        }
        return list[_i].Clone(_player);
    }
    public GunClass GetGunByID(string _id, AgentController _agent)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i]._id == _id)
                return list[i].Clone(_agent);
        }
        Debug.LogError("Couldn't find gun of id: " + _id);
        return null;
    }
    public GunClass GetGunByInt(int _i, AgentController _agent)
    {
        if (_i >= list.Count)
        {
            Debug.LogError("Requested Int was out of range: " + _i);
            return null;
        }
        return list[_i].Clone(_agent);
    }
}
