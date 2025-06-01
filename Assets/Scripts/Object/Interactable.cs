using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public enumType I_type = enumType.interact;
    public string S_interactName = "<N/A>";
    public enum enumType { interact, talk, landing}
    public static string[] interactText =
    {
        "Press [0] to interact with [1]",
        "Press [0] to talk to [1]",
        "Press [0] to land at [1]",
    };
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
