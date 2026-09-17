using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class LevelSelect : MonoBehaviour
{
    [Header("Prefabs")]
    public ButtonAdvanced PF_buttonPrefab;
    [Header("References")]
    public ButtonAdvanced BA_levelTavern;
    [Space(10)]
    public Location missionTavern;

    public missionInfoUIClass missionInfoUI;
    [System.Serializable]
    public class missionInfoUIClass
    {
        public RawImage I_locationImage;
        public Image I_missionImage;
        [Space(10)]
        public TextMeshProUGUI TM_locationName;
        public TextMeshProUGUI TM_missionName;
        public Image[] I_difficultyArray;
        [Space(10)]
        public TextMeshProUGUI TM_description;
        [Space(10)]
        public TextMeshProUGUI TM_reward;
        public RectTransform RT_resourceArray;
        [Space(10)]
        public Image PF_resource;

        public void Setup(Location _location, Mission _mission)
        {
            I_locationImage.texture = _location.T2_icon;
            TM_locationName.text = _location.S_name;
            if (_mission != null)
            {
                I_missionImage.sprite = _mission._sprite;
                TM_missionName.text = _mission._name;
                TM_description.text = _mission._description;
                //Switch to difficulty amount
                for (int i = 0; i < I_difficultyArray.Length; i++)
                    I_difficultyArray[i].gameObject.SetActive(i < 3);
                //Switch to variable amount
                TM_reward.text = "$100";
            }
            else
            {
                I_missionImage.sprite = null;
                TM_missionName.text = "";
                TM_description.text = "";
                //Switch to difficulty amount
                for (int i = 0; i < I_difficultyArray.Length; i++)
                    I_difficultyArray[i].gameObject.SetActive(false);
                //Switch to variable amount
                TM_reward.text = "";
            }
            //Add Resource Array functionality
        }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Setup();
    }

    // Update is called once per frame
    void Setup()
    {
        BA_levelTavern.Setup(SelectTavern, null, null, missionTavern.T2_icon, null, missionTavern.S_name);

        SelectTavern();
    }

    public void SelectTavern() { SelectLevel(missionTavern); }
    
    
    private Location _curLocation;
    private Mission _curMission;
    public void SelectLevel(Location _location)
    {
        _curLocation = _location;
        _curMission = _location.GetRandomMission();
        missionInfoUI.Setup(_curLocation, _curMission);
    }
    public void SelectLevel(Location _location, int _MissionNum)
    {
        _curLocation = _location;
        _curMission = _location.GetSpecificMission(_MissionNum);
        missionInfoUI.Setup(_curLocation, _curMission);
    }

    public void LoadLevel()
    {
        if (_curLocation == null)
            return;
        Themes.themeEnum _theme;
        switch (_curLocation.locationID)
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
        if (_curMission != null)
            LevelGen_Holder.LoadTheme(_theme, _curMission._id);
        else
            LevelGen_Holder.LoadTheme(_theme, -1);
    }
}
