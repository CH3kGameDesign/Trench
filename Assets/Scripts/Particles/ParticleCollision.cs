using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ParticleCollision : MonoBehaviour
{
    public GameObject PF_createOnCollision;
    private ParticleSystem part;
    private List<ParticleCollisionEvent> collisionEvents;

    void Start()
    {
        part = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
    }

    public void OnParticleCollision(GameObject other)
    {
        int numCollisionEvents = part.GetCollisionEvents(other, collisionEvents);

        int i = 0;

        while (i < numCollisionEvents)
        {
            Transform T = Instantiate(PF_createOnCollision, other.transform).transform;
            T.position = collisionEvents[i].intersection;
            T.forward = -collisionEvents[i].normal;
            T.localScale = transform.localScale;
            i++;
        }
    }
}
