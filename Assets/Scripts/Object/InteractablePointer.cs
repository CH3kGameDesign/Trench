using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractablePointer : Interactable
{
    public UnityEvent UE_event;
    public override void OnInteract(BaseController _player)
    {
        UE_event.Invoke();
        base.OnInteract(_player);
    }
}
