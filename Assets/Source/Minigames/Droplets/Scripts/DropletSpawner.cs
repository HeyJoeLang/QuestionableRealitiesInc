using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DropletSpawner : MonoBehaviour
{
    public GameObject spawnee;
    public bool stopSpawning = false;
    public float delayStartTime;
    public Vector2 timeRange;
    public float lifetime;
    public Vector3 rotation;
    void Start()
    {
        StartCoroutine("SpawnObject");
    }
    IEnumerator SpawnObject()
    {
        yield return new WaitForSeconds(delayStartTime);
        while(true)
        {
            if(!stopSpawning)
            {
                yield return new WaitForSeconds(Random.Range(timeRange.x, timeRange.y));
                GameObject droplet = Instantiate(spawnee, transform.position, Quaternion.Euler(rotation), transform);
                droplet.GetComponent<ParticleSystem>().Play();
                Destroy(droplet, lifetime);
            }
        }
    }
}
