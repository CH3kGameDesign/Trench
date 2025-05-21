using UnityEngine;

public class Vehicle_SubCollider : MonoBehaviour
{
    public Ship _ship;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LandingSpot_Enter(Space_LandingSpot _spot)
    {
        if (!_ship.landingSpots.Contains(_spot))
        {
            _ship.landingSpots.Add(_spot);
            _ship.UpdateInteractText();
        }
    }
    public void LandingSpot_Exit(Space_LandingSpot _spot)
    {
        if (_ship.landingSpots.Contains(_spot))
        {
            _ship.landingSpots.Remove(_spot);
            _ship.UpdateInteractText();
        }
    }
}
