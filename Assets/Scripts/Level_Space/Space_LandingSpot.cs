using System.Collections.Generic;
using PurrNet;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Space_LandingSpot : MonoBehaviour
{
    public string landingID;
    public string landingName;

    public Transform exitTransform;

    public SS_MessageObject PF_message;
    public Sprite S_sprite;
    private SS_MessageObject _activeMessage;
    [Space(10)]
    public List<Mission> missionList = new List<Mission>();
    private Mission selMission;

    public void OnTriggerEnter(Collider other)
    {
        Vehicle_SubCollider _col;
        if (other.TryGetComponent<Vehicle_SubCollider>(out _col))
            _col.LandingSpot_Enter(this);
    }
    public void OnTriggerExit(Collider other)
    {
        Vehicle_SubCollider _col;
        if (other.TryGetComponent<Vehicle_SubCollider>(out _col))
            _col.LandingSpot_Exit(this);
    }
    public void Land(PlayerController _player)
    {
        Themes.themeEnum _theme;
        switch (landingID)
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
        _player.info.Land(landingID);
        if (selMission != null)
            LevelGen_Holder.LoadTheme(_theme, selMission._id);
        else
            LevelGen_Holder.LoadTheme(_theme, -1);
    }

    protected void Start()
    {
        PlayerManager.Instance.CheckMain(SetupMessage);
        if (missionList.Count > 0)
            selMission = missionList.GetRandom().Clone();
    }
    void SetupMessage()
    {
        Canvas _canvas = PlayerManager.conversation.C_canvas;
        RectTransform _holder = PlayerManager.conversation.RT_messageHolder_Ship;
        _activeMessage = Instantiate(PF_message, _holder);
        _activeMessage.Setup(S_sprite, transform, _canvas, landingName, true);
    }
    protected void OnDisable()
    {
        //Destroy(_activeMessage.gameObject);
    }
}
