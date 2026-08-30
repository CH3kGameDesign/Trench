using UnityEngine;

public class StaticData : MonoBehaviour
{
    public Resource _Resource;
    public Objective _Objective;
    public Consumable _Consumable;
    public ConversationManager _ConversationManager;
    public SpaceManager _SpaceManager;
    public ArmorManager _ArmorManager;
    public GunManager _GunManager;
    public Themes _Themes;

    public void Awake()
    {
        _Resource.Setup();
        _Objective.Setup();
        _Consumable.Setup();
        _ConversationManager.Setup();
        _SpaceManager.Setup();
        _ArmorManager.Setup();
        _Themes.Setup();
        SaveData.Load();
        //Requires Setup after Save Data Load
        _GunManager.Setup();
    }
}
