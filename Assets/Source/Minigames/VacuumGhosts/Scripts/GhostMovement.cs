using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostMovement : MonoBehaviour
{
    public GameObject ExplosionParticles;
    Vector3 target;
    public float speed;
    private void Start()
    {

        target = GameObject.Find("SuperHero").transform.position;
        transform.LookAt(target);
    }
    void Update()
    {
            transform.position = Vector3.MoveTowards(transform.position, target,  speed  * Time.deltaTime);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("Butler"))
        {
            Destroy(Instantiate(ExplosionParticles, transform.position, Quaternion.identity), 2f);
            Destroy(this.gameObject);
        }
        if(other.transform.CompareTag("SuperHero"))
        {
            FindObjectOfType<GameManager_VacuumGhosts>().HitHero();
            Destroy(this.gameObject);
        }
    }
}
