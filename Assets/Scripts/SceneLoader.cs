using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadSceneId(int id)
    {
        Time.timeScale = 1.0f;

        SceneManager.LoadScene(id);
    }

    public void FadeLoadSceneId(int id)
    {
        Time.timeScale = 1.0f;

        SceneManager.LoadScene(id);
    }

    public void RestartScene()
    {
        Time.timeScale = 1.0f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
