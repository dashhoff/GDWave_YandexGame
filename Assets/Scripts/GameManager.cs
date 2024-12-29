
using System.Collections;
using UnityEngine;
using YG;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

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
        }
        else
            StartCoroutine(StartGame());
    }

    private void OnEnable()
    {
        YandexGame.GetDataEvent += SetParameters;
    }

    private void OnDisable()
    {
        YandexGame.GetDataEvent -= SetParameters;
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

                break;
            }

            yield return new WaitForSecondsRealtime(0.5f);
        }
    }
}
