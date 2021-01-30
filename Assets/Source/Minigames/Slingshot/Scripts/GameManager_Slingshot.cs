using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager_Slingshot : MonoBehaviour
{
    public float timeLeft = 30f;
    public int birdsLeft = 5;
    public GameObject winCanvas, failCanvas;//, startCanvas;
    public Text birdsLeftText;
    public Text timeLeftText;
    bool isGameActive = true;
    lb_BirdController birdController;
    void Start()
    {
        birdController = FindObjectOfType<lb_BirdController>().GetComponent<lb_BirdController>();
    }
    void Win()
    {
        isGameActive = false;
        StopAllBirds();
        winCanvas.SetActive(true);
    }
    void Fail()
    {
        isGameActive = false;
        StopAllBirds();
        failCanvas.SetActive(true);
    }
    public void StopAllBirds()
    {
        birdController.AllFlee();
        birdController.Pause();
    }
    public void HitBird(Vector3 position)
    {
        birdsLeft--;
        birdsLeftText.text = string.Format("{0}", birdsLeft);
        birdController.FeatherEmit(position);
        if (birdsLeft == 0)
        {
            Win();
        }
    }
    private void FixedUpdate()
    {

        if(isGameActive)
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0)
            {
                timeLeft = 0;
                Fail();
            }
            timeLeftText.text = string.Format("{0}", (int)timeLeft);
        }
    }
}
