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

    public void Awake()
    {
        _Resource.Setup();
        _Objective.Setup();
        _Consumable.Setup();
        _ConversationManager.Setup();
        _SpaceManager.Setup();
        _ArmorManager.Setup();
        _GunManager.Setup();
    }
}
