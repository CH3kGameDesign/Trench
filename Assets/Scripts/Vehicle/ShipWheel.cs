using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEditor.Experimental.GraphView.GraphView;

public class ShipWheel : Interactable
{
    private Ship s_Ship;
    public bool DEBUG_dropShip = true;
    public Themes.themeEnum DEBUG_themeToLoad = Themes.themeEnum.ship;

    private void Start()
    {
        if (!DEBUG_dropShip)
        {
            ShipSet();
        }
    }

    void ShipSet()
    {
        if (s_Ship == null)
        {
            try
            {
                s_Ship = GetComponentInParent<Ship>();
            }
            catch (Exception)
            {
                Debug.LogError("LayoutHolder doesn't contain Ship Script");
                return;
            }
        }
    }
    public override void OnInteract(BaseController _player)
    {
        if (DEBUG_dropShip) { DEBUG_DropShip(); return; }

        ShipSet();
        s_Ship.OnInteract(_player);

        base.OnInteract(_player);
    }
    void DEBUG_DropShip()
    {
        SaveData.themeCurrent = DEBUG_themeToLoad;
        SceneManager.LoadScene(0);
    }
}
