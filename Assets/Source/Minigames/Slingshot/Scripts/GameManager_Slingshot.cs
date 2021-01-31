using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager_Slingshot : MonoBehaviour
{
    float timeTillWin = 60;
    float startTime;
    int hitsLeft;
    public int totalHits = 5;
    public GameObject winCanvas, failCanvas, overviewCanvas;
    public Text timeLeftText;
    bool isUpdatingProgress = false;
    lb_BirdController birdController;
    public ProgressBarPro focusMeter, timeMeter;
    void Start()
    {
        overviewCanvas.GetComponent<Animator>().SetTrigger("FadeFromBlack");
        startTime = Time.time;
        birdController = FindObjectOfType<lb_BirdController>().GetComponent<lb_BirdController>();
        birdController.Pause();
    }
    public void StartGame()
    {
        birdController.AllUnPause();
        hitsLeft = totalHits;
        startTime = Time.time;
        isUpdatingProgress = true;
    }
    void Win()
    {
        isUpdatingProgress = false;
        StopAllBirds();
        winCanvas.SetActive(true);
    }
    void Fail()
    {
        isUpdatingProgress = false;
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
        hitsLeft--; 
        float hits = (float)hitsLeft / (float)totalHits;
        focusMeter.Value = 1-hits;
        birdController.FeatherEmit(position);
        if (hitsLeft == 0)
        {
            Win();
        }
    }
    private void FixedUpdate()
    {
        if (isUpdatingProgress)
        {
            float progress = ((Time.time - startTime) / timeTillWin); 
            if (progress >= 1)
            {
                Fail();
            }
            else
            {
                timeMeter.Value = 1 - progress;
                timeLeftText.text = string.Format("{0}", (int)(timeTillWin - (Time.time - startTime)));
            }
        }
    }
}
