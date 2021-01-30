using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterDroplet : MonoBehaviour
{
    ParticleSystem part;
    public List<ParticleCollisionEvent> collisionEvents;

    void Start()
    {
        part = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
    }

    void OnParticleCollision(GameObject other)
    {

        int numCollisionEvents = part.GetCollisionEvents(other, collisionEvents);
        for(int i = 0; i < numCollisionEvents; i++)
        {
            Destroy(this.gameObject,1);
        }

        if (other.CompareTag("SuperHero"))
        {
            FindObjectOfType<GameManager_Droplets>().GetComponent<GameManager_Droplets>().HitHero();
        }
    }
}