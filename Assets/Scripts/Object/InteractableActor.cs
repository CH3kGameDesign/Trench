using UnityEngine;

public class InteractableActor : InteractablePointer
{
    public RagdollManager RM_ragdollManager;
    public CharacterID CharID = CharacterID.DEBUG_Placeholder;
    private ConversationManager.characterClass Character;

    public void Start()
    {
        Character = ConversationManager.Instance.GetCharacter(CharID);
        S_interactName = Character.name;
        if (RM_ragdollManager == null)
            RM_ragdollManager = Instantiate(Character.PF_ragdoll,transform);
        Character.Armor.Equip(RM_ragdollManager);
    }
}
