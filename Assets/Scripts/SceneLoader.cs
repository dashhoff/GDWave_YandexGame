using UnityEngine;
using UnityEngine.SceneManagement;
using YG;

public class SceneLoader : MonoBehaviour
{
    public void LoadSceneId(int id)
    {
        Time.timeScale = 1.0f;

        YandexGame.FullscreenShow();

        SceneManager.LoadScene(id);
    }

    public void FadeLoadSceneId(int id)
    {
        Time.timeScale = 1.0f;

        YandexGame.FullscreenShow();

        SceneManager.LoadScene(id);
    }

    public void RestartScene()
    {
        Time.timeScale = 1.0f;

        YandexGame.FullscreenShow();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
