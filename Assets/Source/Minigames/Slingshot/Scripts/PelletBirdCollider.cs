using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PelletBirdCollider : MonoBehaviour
{

    GameManager_Slingshot manager;
    private void Start()
    {
        manager = FindObjectOfType<GameManager_Slingshot>().GetComponent<GameManager_Slingshot>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("lb_bird"))
        {
            other.gameObject.SendMessage("Flee");
            Destroy(transform.parent.gameObject);
            other.enabled = false;
            //Destroy(other.gameObject, 1f);
            manager.HitBird(other.transform.position);
        }
        if(other.gameObject.CompareTag("Ground"))
        {
            Destroy(transform.parent.gameObject);
        }
    }
}
