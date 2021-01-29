
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager_VacuumGhosts : MonoBehaviour
{
    public GameObject failCanvas, winCanvas;
    bool isGameActive = true;
    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void Fail()
    {
        StopGhosts();
        failCanvas.SetActive(true);
    }
    void Win()
    {
        winCanvas.SetActive(true);
    }
    void Update()
    {
        if(GameObject.FindGameObjectWithTag("Enemy") == null && isGameActive)
        {
            isGameActive = false;
            Win();
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
