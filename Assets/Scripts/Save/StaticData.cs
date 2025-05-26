using UnityEngine;

public class StaticData : MonoBehaviour
{
    public Resource _Resource;
    public Objective _Objective;
    public Consumable _Consumable;
    public ConversationManager _ConversationManager;
    public SpaceManager _SpaceManager;
    public ArmorManager _ArmorManager;

    public void Awake()
    {
        if (Resource.Instance == null) Resource.Instance = _Resource;
        if (Objective.Instance == null) Objective.Instance = _Objective;
        if (Consumable.Instance == null) Consumable.Instance = _Consumable;
        if (ConversationManager.Instance == null) ConversationManager.Instance = _ConversationManager;
        if (SpaceManager.Instance == null) SpaceManager.Instance = _SpaceManager;
        if (ArmorManager.Instance == null) ArmorManager.Instance = _ArmorManager;
    }
}
