using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pellet : MonoBehaviour
{
    public void Launch()
    {
        FindObjectOfType<SlingshotLauncher>().GetComponent<SlingshotLauncher>().Launch(this.gameObject);
    }
}
