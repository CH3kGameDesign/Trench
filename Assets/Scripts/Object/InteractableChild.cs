using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableChild : Interactable
{
    public Interactable I_parent;
    public override Interactable GetInteractable()
    {
        return I_parent;
    }
}
