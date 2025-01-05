using System.Collections;
using UnityEngine;

public class EntryPoint : MonoBehaviour
{
    [SerializeField] private GameManager GameManager;
    [SerializeField] private GameSettings GameSettings;

    [SerializeField] private UIController UIController;
    [SerializeField] private SoundSlider SoundSlider;
    [SerializeField] private ToggleSwitch[] ToggleSwitches;

    private void Awake()
    {
        StartCoroutine(StartGameCoroutine());
    }

    private IEnumerator StartGameCoroutine()
    {
        GameSettings.Init();
        GameManager.Init();

        yield return new WaitForSecondsRealtime(0.1f);

        UIController.Init();
        SoundSlider.Init();

        for (int i = 0; i < ToggleSwitches.Length; i++)
        {
            ToggleSwitches[i].Init();
        }
        

    }
}
