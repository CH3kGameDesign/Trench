using UnityEngine;
using System.Collections.Generic;

public class LevelSelect : MonoBehaviour
{
    [Header("Prefabs")]
    public ButtonAdvanced PF_buttonPrefab;
    [Header("References")]
    public ButtonAdvanced BA_level1;
    public ButtonAdvanced BA_level2;
    [Space(10)]
    public List<Location> missionList = new List<Location>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Setup();
    }

    // Update is called once per frame
    void Setup()
    {
        BA_level1.Setup(LoadLevel_1, null, null, missionList[0].T2_icon, null, missionList[0].S_name);
        BA_level2.Setup(LoadLevel_2, null, null, missionList[1].T2_icon, null, missionList[1].S_name);
    }

    public void LoadLevel_1() { LoadLevel(missionList[0]); }
    public void LoadLevel_2() { LoadLevel(missionList[1]); }
    
    public void LoadLevel(Location _location)
    {
        Themes.themeEnum _theme;
        switch (_location.locationID)
        {
            case "tavern":
                _theme = Themes.themeEnum.spaceStation;
                break;
            case "lidoStation":
                _theme = Themes.themeEnum._default;
                break;
            case "dollyStation":
                _theme = Themes.themeEnum._default;
                break;
            default:
                _theme = Themes.themeEnum.ship;
                break;
        }
        //_player.info.Land(landingID);
        Mission _temp = _location.GetRandomMission();
        if (_temp != null)
            LevelGen_Holder.LoadTheme(_theme, _temp._id);
        else
            LevelGen_Holder.LoadTheme(_theme, -1);
    }
}
