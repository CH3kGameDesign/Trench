using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConversationEvent : MonoBehaviour
{
    public string eventID = "";
    public void ApplyEffect()
    {
        Conversation _conversation = Conversation.Instance;
        _conversation.StartDialogue(eventID);
    }
}
