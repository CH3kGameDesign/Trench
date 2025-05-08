using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShipWheel : Interactable
{
    private Ship s_Ship;
    public bool DEBUG_dropShip = true;
    public override void OnInteract(BaseController _player)
    {
        if (DEBUG_dropShip) { DEBUG_DropShip(); return; }

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
        GetComponentInParent<Ship>().OnInteract(_player);

        base.OnInteract(_player);
    }
    void DEBUG_DropShip()
    {
        switch (SaveData.themeCurrent)
        {
            case Themes.themeEnum.spaceStation:
                SaveData.themeCurrent = Themes.themeEnum._default;
                break;
            default:
                SaveData.themeCurrent = Themes.themeEnum.spaceStation;
                break;
        }
        SceneManager.LoadScene(0);
    }
}
