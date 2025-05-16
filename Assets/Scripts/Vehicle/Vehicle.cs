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

    public enum seatTypeEnum { driver, passenger, none};
    [System.Serializable]
    public class seatClass
    {
        public seatTypeEnum seatType = seatTypeEnum.driver;
        public Transform T_seatPos;
        public Transform T_exitPos;
        [HideInInspector] public BaseController BC_agent;
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

    public virtual void OnUpdate_Driver(BaseController _player)
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
            Seats[_seat].BC_agent.V_curVehicle = this;
            Seats[_seat].BC_agent.GameState_Change(BaseController.gameStateEnum.vehicle);
            Seat_AttachPlayer(Seats[_seat]);
        }
    }

    public virtual void Seat_AttachPlayer(seatClass _seat)
    {
        BaseController _player = _seat.BC_agent;
        _player.RB.isKinematic = true;
        _player.RB.gameObject.layer = 8;
        _player.RM_ragdoll.SetLayer(8);
        _player.RB.transform.parent = _seat.T_seatPos;
        _player.RB.transform.position = _seat.T_seatPos.position;
        _player.RB.transform.rotation = _seat.T_seatPos.rotation;
        _player.NMA.enabled = false;
    }

    public virtual void Seat_DetachPlayer(seatClass _seat)
    {
        BaseController _player = _seat.BC_agent;
        _player.RB.transform.parent = _player.transform;
        _player.RB.transform.position = _seat.T_exitPos.position;
        _player.RB.transform.rotation = _seat.T_seatPos.rotation;
        _player.RB.isKinematic = false;
        _player.RB.gameObject.layer = 3;
        _player.RM_ragdoll.SetLayer(11);
        _player.NMA.enabled = true;
    }

    public virtual void OnExit(int _num)
    {
        SeatInUse.Remove(_num);
        Seat_DetachPlayer(Seats[_num]);
        Seats[_num].BC_agent.GameState_Change(BaseController.gameStateEnum.active);
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
}
