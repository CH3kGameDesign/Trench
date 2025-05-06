using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static PlayerController;

public class BaseController : MonoBehaviour
{
    public NavMeshAgent NMA;
    public Transform T_model;
    public Transform T_gunHook;
    public Transform T_barrelHook;
    public Rigidbody RB;
    public RagdollManager RM_ragdoll;
    [HideInInspector] public Vehicle V_curVehicle = null;

    [HideInInspector] public GunClass gun_Equipped;
    [HideInInspector] public GunClass[] gun_EquippedList = new GunClass[3];

    public static gameStateEnum GameState = gameStateEnum.active;
    public enum gameStateEnum { inactive, active, dialogue, vehicle, ragdoll }
    public virtual void Start()
    {
        
    }

    public virtual void Update()
    {
        
    }

    public virtual void OnHit(GunManager.bulletClass _bullet)
    {
        
    }

    public virtual void GameState_Change(gameStateEnum _state)
    {
        GameState = _state;
    }
    public virtual void Pickup_Treasure(Treasure _treasure)
    {

    }
    public virtual void Update_Objectives()
    {

    }
    public virtual void Update_Objectives(Objective_Type _type, int _amt)
    {

    }
}
