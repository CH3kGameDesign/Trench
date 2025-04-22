using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static PlayerController;

public class BaseController : MonoBehaviour
{
    public NavMeshAgent NMA;
    public Transform T_model;
    public Rigidbody RB;
    public RagdollManager RM_ragdoll;
    [HideInInspector] public Vehicle V_curVehicle = null;

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
    public virtual void PickUp(Treasure _treasure)
    {

    }
}
