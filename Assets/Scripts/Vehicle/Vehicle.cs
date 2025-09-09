using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 public class Vehicle : Interactable
{
    public virtual string Type() { return "Unassigned"; }

    public List<seatClass> Seats = new List<seatClass>();
    public Transform T_camHook;
    public Vector3 V3_camOffset = new Vector3(0, 0.5f, -6);
    [HideInInspector] public List<int> SeatInUse = new List<int>();

    [HideInInspector] public float f_curHealth = 1000;
    public int I_maxHealth = 1000;
    public enum seatTypeEnum { driver, passenger, none};
    [System.Serializable]
    public class seatClass
    {
        public seatTypeEnum seatType = seatTypeEnum.driver;
        public Transform T_seatPos;
        public Transform T_exitPos;
        [HideInInspector] public BaseController BC_agent;
        [HideInInspector] public BaseController.stateEnum prevState;
    }

    public virtual void Awake()
    {
        SetHealth();
    }

    void SetHealth()
    {
        f_curHealth = I_maxHealth;
    }
    public override void OnInteract(BaseController _player)
    {
        for (int i = 0; i < Seats.Count; i++)
        { 
            if (Seats[i].BC_agent == _player)
            {
                OnExit(i);
                return;
            }
        }
        OnEnter(_player);
    }
    public override void OnUpdate(BaseController _player)
    {
        seatClass _seat = GetSeat(_player);
        if (_seat == null) { Debug.LogError("'" + _player.name + "' isn't in a dedicated seat in '" + name + "', but is still attempting to Update in it"); return; }
        switch (_seat.seatType)
        {
            case seatTypeEnum.driver:
                OnUpdate_Driver(_player);
                break;
            case seatTypeEnum.passenger:
                OnUpdate_Passenger(_player);
                break;
            case seatTypeEnum.none:
                break;
            default:
                break;
        }
    }

    public override void OnFixedUpdate(BaseController _player)
    {
        seatClass _seat = GetSeat(_player);
        if (_seat == null) { Debug.LogError("'" + _player.name + "' isn't in a dedicated seat in '" + name + "', but is still attempting to Update in it"); return; }
        switch (_seat.seatType)
        {
            case seatTypeEnum.driver:
                OnFixedUpdate_Driver(_player);
                break;
            case seatTypeEnum.passenger:
                break;
            case seatTypeEnum.none:
                break;
            default:
                break;
        }
    }

    public virtual void OnUpdate_Driver(BaseController _player)
    {

    }

    public virtual void OnFixedUpdate_Driver(BaseController _player)
    {

    }

    public virtual void OnUpdate_Passenger(BaseController _player)
    {

    }

    public virtual void OnEnter(BaseController _player)
    {
        int _seat = Seat_GetOpen();
        if (_seat != -1)
        {
            SeatInUse.Add(_seat);
            Seats[_seat].BC_agent = _player;
            Seats[_seat].BC_agent.EnterExit_Vehicle(true, this);
            Seats[_seat].BC_agent.V_curVehicle = this;
            Seats[_seat].prevState = _player.state;
            Seats[_seat].BC_agent.ChangeState(BaseController.stateEnum.vehicle);
            Seat_AttachPlayer(Seats[_seat]);
        }
    }

    public virtual void Seat_AttachPlayer(seatClass _seat)
    {
        BaseController _player = _seat.BC_agent;
        _player.RB.isKinematic = true;
        _player.RB.gameObject.layer = 8;
        _player.RM_ragdoll.SetLayer(8);
        _player.AttachToBound(_seat.T_seatPos);
        _player.RB.transform.position = _seat.T_seatPos.position;
        _player.RB.transform.rotation = _seat.T_seatPos.rotation;
        _player.NMA.enabled = false;
    }

    public virtual void Seat_DetachPlayer(seatClass _seat)
    {
        BaseController _player = _seat.BC_agent;
        _player.AttachToBound();
        _player.RB.transform.position = _seat.T_exitPos.position;
        _player.RB.transform.rotation = _seat.T_seatPos.rotation;
        _player.RB.linearVelocity = Vector3.zero;
        _player.RB.angularVelocity = Vector3.zero;
        _player.GroundedUpdate(false);
        _player.RB.gameObject.layer = 3;
        _player.RM_ragdoll.SetLayer(11);
        _player.NMA.enabled = true;
    }

    public virtual void OnExit(int _num)
    {
        SeatInUse.Remove(_num);
        Seat_DetachPlayer(Seats[_num]);
        Seats[_num].BC_agent.EnterExit_Vehicle(false, this);
        Seats[_num].BC_agent.ChangeState(Seats[_num].prevState);
        Seats[_num].BC_agent.V_curVehicle = null;
        Seats[_num].BC_agent = null;
    }

    int Seat_GetOpen()
    {
        for (int i = 0; i < Seats.Count; i++)
        {
            if (Seats[i].BC_agent == null)
                return i;
        }
        return -1;
    }
    public virtual seatClass GetSeat(BaseController _player)
    {
        for (int i = 0; i < Seats.Count; i++)
        {
            if (Seats[i].BC_agent == _player)
                return Seats[i];
        }
        return null;
    }
    public virtual seatTypeEnum GetSeatType(BaseController _player)
    {
        for (int i = 0; i < Seats.Count; i++)
        {
            if (Seats[i].BC_agent == _player)
                return Seats[i].seatType;
        }
        return seatTypeEnum.none;
    }

    public bool hasDriver()
    {
        foreach (var seat in SeatInUse)
        {
            if (Seats[seat].seatType == seatTypeEnum.driver)
                return true;
        }
        return false;
    }
    public bool DriverIsAgent()
    {
        AgentController _AC;
        return DriverIsAgent(out _AC);
    }
    public bool DriverIsMain()
    {
        PlayerController _PC;
        return DriverIsMain(out _PC);
    }
    public bool DriverIsMain(out PlayerController _PC)
    {
        _PC = null;
        if (PlayerManager.main == null)
            return false;
        foreach (var seat in SeatInUse)
        {
            if (Seats[seat].seatType == seatTypeEnum.driver)
                if (Seats[seat].BC_agent == PlayerManager.main)
                {
                    _PC = PlayerManager.main;
                    return true;
                }
        }
        return false;
    }
    public bool DriverIsAgent(out AgentController _AC)
    {
        _AC = null;
        foreach (var seat in SeatInUse)
        {
            if (Seats[seat].seatType == seatTypeEnum.driver)
            {
                if (Seats[seat].BC_agent == null)
                    return false;
                if (Seats[seat].BC_agent is AgentController)
                {
                    _AC = Seats[seat].BC_agent as AgentController;
                    return true;
                }
                else
                    return false;
            }
        }
        return false;
    }
    public bool GetDriver(out BaseController _BC)
    {
        _BC = null;
        foreach (var seat in SeatInUse)
        {
            if (Seats[seat].seatType == seatTypeEnum.driver)
            {
                if (Seats[seat].BC_agent == null)
                    return false;
                _BC = Seats[seat].BC_agent;
                return true;
            }
        }
        return false;
    }

    public virtual void RotLoop(bool yLoop, float _adjust)
    {

    }

    public virtual Vector3 GetLocalVelocity()
    {
        return Vector3.zero;
    }
    public virtual Vector3 GetVelocity()
    {
        return Vector3.zero;
    }
    public virtual float GetWeaponSpeed()
    {
        return 0f;
    }
}
