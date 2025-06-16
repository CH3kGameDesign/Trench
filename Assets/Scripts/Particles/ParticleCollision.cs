using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ParticleCollision : MonoBehaviour
{
    public Decal_Handler PF_createOnCollision;
    private ParticleSystem part;
    private List<ParticleCollisionEvent> collisionEvents;
    public int I_maxAmt = 1;
    private int i_curAmt = 0;

    void Start()
    {
        part = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
    }

    public void OnParticleCollision(GameObject other)
    {
        int numCollisionEvents = part.GetCollisionEvents(other, collisionEvents);

        int i = 0;

        while (i < numCollisionEvents && i_curAmt < I_maxAmt)
        {
            Decal_Handler DC = Instantiate(PF_createOnCollision, other.transform);
            LevelGen_Holder.Instance.AddDecal(DC);
            DC.transform.position = collisionEvents[i].intersection;
            DC.transform.forward = -collisionEvents[i].normal;
            DC.transform.localScale = transform.localScale;
            i++;
            i_curAmt++;
        }
    }
}
