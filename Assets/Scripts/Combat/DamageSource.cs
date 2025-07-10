using PurrNet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageSource : NetworkBehaviour
{
    // Start is called before the first frame update
    public virtual void Start()
    {
        
    }
    protected override void OnSpawned()
    {
        base.OnSpawned();
        if (!isController)
            this.enabled = false;
    }
}
