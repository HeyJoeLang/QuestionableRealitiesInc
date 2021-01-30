using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingWall : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("lb_bird"))
        {
            other.gameObject.SendMessage("Flee");
        }
    }
}
