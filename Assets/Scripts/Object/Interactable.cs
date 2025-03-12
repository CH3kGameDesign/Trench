using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public string S_interactName = "<N/A>";

    public virtual Interactable GetInteractable()
    {
        return this;
    }

    public virtual void OnInteract(PlayerController _player)
    {

    }
    public virtual void OnUpdate(PlayerController _player)
    {

    }

    public virtual void Update()
    {

    }
}
