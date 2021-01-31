using UnityEngine;
using UnityEngine.SceneManagement;

public class NextScene : MonoBehaviour
{
    public string nextLevel;
    public void LoadNextScene()
    {
        SceneManager.LoadScene(nextLevel);
    }
}
