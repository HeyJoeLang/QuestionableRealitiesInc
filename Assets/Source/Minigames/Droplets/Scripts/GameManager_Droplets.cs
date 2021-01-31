using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.UI;

public class GameManager_Droplets : MonoBehaviour
{
    float timeTillWin = 60;
    float startTime;
    int hitsLeft;
    public int totalHits = 5;
    public GameObject winCanvas, failCanvas, overviewCanvas;
    public Text timeLeftText;
    bool isUpdatingProgress = false;
    public GameObject DropletSpawner;

    public ProgressBarPro focusMeter, timeMeter;

    private void Start()
    {
        overviewCanvas.GetComponent<Animator>().SetTrigger("FadeFromBlack");
        startTime = Time.time;
    }
    public void StartGame()
    {
        DropletSpawner.SetActive(true);
        hitsLeft = totalHits;
        startTime = Time.time;
        isUpdatingProgress = true;
    }
    private void FixedUpdate()
    {
        if(isUpdatingProgress)
        {
            float progress = ((Time.time - startTime) / timeTillWin);
            if (progress >= 1)
            {
                Win();
            }
            else
            {
                timeMeter.Value = 1 - progress;
                timeLeftText.text = string.Format("{0}", (int)(timeTillWin - (Time.time - startTime)));
            }
        }
    }
    public void HitHero()
    {
        hitsLeft--;
        float hits = (float)hitsLeft / (float)totalHits;
        focusMeter.Value = hits;
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
        isUpdatingProgress = false;
        StopDroplets();
        failCanvas.SetActive(true);
    }
    public void Win()
    {
        isUpdatingProgress = false;
        StopDroplets();
        winCanvas.SetActive(true);
    }
}
