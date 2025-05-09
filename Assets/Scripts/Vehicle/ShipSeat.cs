using UnityEngine;

public class ShipSeat : MonoBehaviour
{
    public Vehicle.seatClass seat;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        try
        {
            Ship _ship = GetComponentInParent<Ship>();
            _ship.Seats.Add(seat);
            if (seat.seatType == Vehicle.seatTypeEnum.driver)
                _ship.T_pilotSeat = transform;
        }
        catch (System.Exception)
        {
            Debug.LogError("LayoutHolder doesn't contain Ship Script");
            throw;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
