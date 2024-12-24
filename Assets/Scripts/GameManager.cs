
using NaughtyAttributes;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using YG;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Space(20)]
    public int CurrentLevel;
    public int Location;
    public int Money;

    private void Awake()
    {
        Application.targetFrameRate = 60;

        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        if (YandexGame.SDKEnabled)
        {
            SetParameters();
            if (UIController.Instance != null)
                UIController.Instance.UpdateLevelText();
        }
        else
            StartCoroutine(StartGame());
    }

    private void OnEnable()
    {
        YandexGame.GetDataEvent += LoadMenu;
        YandexGame.GetDataEvent += SetParameters;
    }

    private void OnDisable()
    {
        YandexGame.GetDataEvent -= LoadMenu;
        YandexGame.GetDataEvent -= SetParameters;
    }

    private void LoadMenu()
    {
        SceneManager.LoadScene(1);
    }

    private void SetParameters()
    {
    }

    private IEnumerator StartGame()
    {
        while (true)
        {
            if (YandexGame.SDKEnabled)
            {
                SetParameters();

                UIController.Instance.UpdateLevelText();

                LoadMenu();

                break;
            }

            yield return new WaitForSecondsRealtime(0.5f);
        }
    }

    public void Save()
    {
    }

    [Button]
    public void ResetProgress()
    {
        YandexGame.SaveProgress();

        UIController.Instance.UpdateLevelText();
    }
}
