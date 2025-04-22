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

    public virtual void OnInteract(BaseController _player)
    {

    }
    public virtual void OnUpdate(BaseController _player)
    {

    }

    public virtual void Update()
    {

    }
}
