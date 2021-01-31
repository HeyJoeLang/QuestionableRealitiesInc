
using CinemaDirector;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager_VacuumGhosts : MonoBehaviour
{
    public GameObject failCanvas, winCanvas, overviewCanvas;
    bool isGameActive = false;
    public GameObject enemies;
    public Text timeLeftText;
    public ProgressBarPro focusMeter, timeMeter;
    float timeTillWin = 60;
    float startTime;
    int hitsLeft;
    public int totalHits = 5;
    public GameObject Jofree;
    private void Start()
    {
        startTime = Time.time;
        overviewCanvas.GetComponent<Animator>().SetTrigger("FadeFromBlack");
    }
    public void StartGame()
    {
        Jofree.GetComponent<Animator>().SetTrigger("walk");
        hitsLeft = totalHits;
        startTime = Time.time;
        enemies.SetActive(true);
        isGameActive = true;
        focusMeter.Value = 1;
    }
    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void Fail()
    {
        isGameActive = false;
        StopGhosts();
        failCanvas.SetActive(true);
    }
    void Win()
    {
        isGameActive = false;
        winCanvas.SetActive(true);
    }
    public void HitHero()
    {
        hitsLeft--;
        float hits = (float)hitsLeft / (float)totalHits;
        focusMeter.Value = hits;
        if(hitsLeft == 0)
        {
            Fail();
        }
    }
    void Update()
    {
        if(isGameActive)
        {
            float progress = ((Time.time - startTime) / timeTillWin);
            if (progress >= 1)
            {
                Win();
            }
            else
            {
                timeMeter.Value = 1-progress;
                timeLeftText.text = string.Format("{0}", (int)(timeTillWin - (Time.time - startTime)));
            }
        }
    }
    void StopGhosts()
    {
        GhostMovement[] enemies = FindObjectsOfType<GhostMovement>();
        for(int i = 0; i < enemies.Length; i++)
        {
            enemies[i].speed = 0;
        }
    }
}
