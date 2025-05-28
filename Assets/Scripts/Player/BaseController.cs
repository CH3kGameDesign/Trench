using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BaseController : MonoBehaviour
{
    public NavMeshAgent NMA;
    public Transform T_model;
    public Transform T_gunHook;
    public Transform T_barrelHook;
    public Transform T_aimPoint;
    public Rigidbody RB;
    public RagdollManager RM_ragdoll;
    public AgentAudioHolder AH_agentAudioHolder;
    [HideInInspector] public Vehicle V_curVehicle = null;
    [HideInInspector] public Transform T_surface = null;
    private Vector3 v3_surfacePos;
    public LayerMask LM_TerrainRay;
    [Header("Combat Variables")]
    public float F_maxHealth = 100;
    [HideInInspector] public float F_curHealth = 100;

    [HideInInspector] public GunClass gun_Equipped;
    [HideInInspector] public GunClass[] gun_EquippedList = new GunClass[3];

    [HideInInspector] public bool b_alive = true;
    [HideInInspector] public bool b_grounded = false;

    public static gameStateEnum GameState = gameStateEnum.active;
    public enum gameStateEnum { inactive, active, dialogue, vehicle, ragdoll, dialogueResponse, menu }
    public virtual void Start()
    {

    }

    public virtual void Update()
    {

    }

    public virtual void FixedUpdate()
    {
        UpdateParentPosition();
    }

    bool onSurface = false;
    public void UpdateParentPosition()
    {
        if (T_surface != null && V_curVehicle == null)
        {
            if (onSurface)
            {
                NMA.Move(T_surface.position - v3_surfacePos);
                RB.transform.position += T_surface.position - v3_surfacePos;
            }
            T_surface_Update();
        }
        else
            onSurface = false;
    }
    public void T_surface_Update()
    {
        v3_surfacePos = T_surface.position;
        onSurface = true;
    }
    public void T_surface_Update(Transform _temp)
    {
        T_surface = _temp;
        v3_surfacePos = _temp.position;
        onSurface = true;
    }

    public virtual void OnHit(GunManager.bulletClass _bullet)
    {
        
    }

    public virtual void OnHeal(float _amt)
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
    public virtual void Update_Objectives(Objective_Type _type, Resource_Type _resource, int _amt)
    {

    }
}
