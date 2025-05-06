using UnityEngine;
using UnityEngine.SceneManagement;

public class Ship : Vehicle
{
    public override void OnInteract(BaseController _player)
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
