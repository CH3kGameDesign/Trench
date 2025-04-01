using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 public class Vehicle : Interactable
{
    public virtual string Type() { return "Unassigned"; }

    public seatClass[] Seats = new seatClass[0];
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
        [HideInInspector] public PlayerController PC_Player;
    }

    public override void OnInteract(PlayerController _player)
    {
        for (int i = 0; i < Seats.Length; i++)
        { 
            if (Seats[i].PC_Player == _player)
            {
                OnExit(i);
                return;
            }
        }
        OnEnter(_player);
    }
    public override void OnUpdate(PlayerController _player)
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

    public virtual void OnUpdate_Driver(PlayerController _player)
    {

    }

    public virtual void OnUpdate_Passenger(PlayerController _player)
    {

    }

    public virtual void OnEnter(PlayerController _player)
    {
        int _seat = Seat_GetOpen();
        if (_seat != -1)
        {
            SeatInUse.Add(_seat);
            Seats[_seat].PC_Player = _player;
            Seats[_seat].PC_Player.V_curVehicle = this;
            Seats[_seat].PC_Player.GameState_Change(PlayerController.gameStateEnum.vehicle);
            Seat_AttachPlayer(Seats[_seat]);
        }
    }

    public virtual void Seat_AttachPlayer(seatClass _seat)
    {
        PlayerController _player = _seat.PC_Player;
        _player.RB_player.isKinematic = true;
        _player.RM_ragdoll.SetLayer(8);
        _player.RB_player.transform.parent = _seat.T_seatPos;
        _player.RB_player.transform.position = _seat.T_seatPos.position;
        _player.RB_player.transform.rotation = _seat.T_seatPos.rotation;
        _player.NMA_player.enabled = false;
    }

    public virtual void Seat_DetachPlayer(seatClass _seat)
    {
        PlayerController _player = _seat.PC_Player;
        _player.RB_player.transform.parent = _player.transform;
        _player.RB_player.transform.position = _seat.T_exitPos.position;
        _player.RB_player.transform.rotation = _seat.T_seatPos.rotation;
        _player.RB_player.isKinematic = false;
        _player.RM_ragdoll.SetLayer(3);
        _player.NMA_player.enabled = true;
    }

    public virtual void OnExit(int _num)
    {
        SeatInUse.Remove(_num);
        Seat_DetachPlayer(Seats[_num]);
        Seats[_num].PC_Player.GameState_Change(PlayerController.gameStateEnum.active);
        Seats[_num].PC_Player.V_curVehicle = null;
        Seats[_num].PC_Player = null;
    }

    int Seat_GetOpen()
    {
        for (int i = 0; i < Seats.Length; i++)
        {
            if (Seats[i].PC_Player == null)
                return i;
        }
        return -1;
    }
    public virtual seatClass GetSeat(PlayerController _player)
    {
        for (int i = 0; i < Seats.Length; i++)
        {
            if (Seats[i].PC_Player == _player)
                return Seats[i];
        }
        return null;
    }
    public virtual seatTypeEnum GetSeatType(PlayerController _player)
    {
        for (int i = 0; i < Seats.Length; i++)
        {
            if (Seats[i].PC_Player == _player)
                return Seats[i].seatType;
        }
        return seatTypeEnum.none;
    }
}
