
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadSceneId(int id)
    {
        Time.timeScale = 1.0f;

        SceneManager.LoadScene(id);
    }

    public void SkipLevel()
    {
        Time.timeScale = 1.0f;

        GameManager.Instance.Location++;
        GameManager.Instance.Save();

        if (GameManager.Instance.Location >= 16)
        {
            GameManager.Instance.Location = 4;
            GameManager.Instance.Save();
        }

        SceneManager.LoadScene(GameManager.Instance.Location + 1);
    }

    public void LoadNextScene()
    {
        Time.timeScale = 1.0f;


        if (GameManager.Instance.Location >= 16)
        {
            GameManager.Instance.Location = 4;
            GameManager.Instance.Save();
        }

        SceneManager.LoadScene(GameManager.Instance.Location + 1);
    }

    public void LoadCurrentLevel()
    {
        Time.timeScale = 1.0f;

        SceneManager.LoadScene(GameManager.Instance.Location + 1);
    }

    public void RestartScene()
    {
        Time.timeScale = 1.0f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
