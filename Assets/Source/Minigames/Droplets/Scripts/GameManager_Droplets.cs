using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.UI;

public class GameManager_Droplets : MonoBehaviour
{
    float timeTillWin = 60;
    float startTime;
    public int hitsLeft = 3;
    public GameObject winCanvas, failCanvas, instructions;
    ClockTimer clockTimer;
    public Text hitsLeftText;

    private void Start()
    {
        startTime = Time.time;
        Destroy(instructions, 1);
        clockTimer = FindObjectOfType<ClockTimer>().GetComponent<ClockTimer>();
    }
    private void FixedUpdate()
    {
        float progress = ((Time.time - startTime) / timeTillWin);
        if(progress >= 1)
        {
            Win();
        }
        else
        {
            clockTimer.UpdateProgress(progress);
        }
        hitsLeftText.text = string.Format("{0}",hitsLeft);
    }
    public void HitHero()
    {
        hitsLeft--;
        if (hitsLeft == 0)
        {
            Fail();
        }
    }
    void StopDroplets()
    {
        DropletSpawner[] dropletSpawner = FindObjectsOfType<DropletSpawner>();
        for (int i = 0; i < dropletSpawner.Length; i++)
        {
            Destroy(dropletSpawner[i].gameObject);
        }

        WaterDroplet[] droplets = FindObjectsOfType<WaterDroplet>();
        for (int i = 0; i < droplets.Length; i++)
        {
            Destroy(droplets[i].gameObject);
        }
    }
    public void Fail()
    {
        StopDroplets();
        failCanvas.SetActive(true);
    }
    public void Win()
    {
        StopDroplets();
        winCanvas.SetActive(true);
    }
}
