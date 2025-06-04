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
            default:
                _theme = Themes.themeEnum.ship;
                break;
        }
        SaveData.lastLandingSpot = landingID;
        LevelGen_Holder.LoadTheme(_theme);
    }

    void Start()
    {
        SetupMessage();
    }
    void SetupMessage()
    {
        Canvas _canvas = Conversation.Instance.C_canvas;
        RectTransform _holder = Conversation.Instance.RT_messageHolder;
        _activeMessage = Instantiate(PF_message, _holder);
        _activeMessage.Setup(S_sprite, transform, _canvas, true);
    }
    private void OnDestroy()
    {
        Destroy(_activeMessage.gameObject);
    }
}
