using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlingshotLauncher : MonoBehaviour
{
    public GameObject pelletPrefab;
    void Start()
    {
        
    }
    void Update()
    {
        if(GameObject.FindGameObjectWithTag("Pellet") == null)
        {
            SpawnPellet();
        }
    }
    void SpawnPellet()
    {
        Instantiate(pelletPrefab, transform.position, Quaternion.identity);
    }
    public void Launch(GameObject pellet)
    {
        Destroy(pellet, 3f);
    }
}
