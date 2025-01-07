using System.Collections;
using UnityEngine;

public class EntryPoint : MonoBehaviour
{
    [SerializeField] private GameManager GameManager;
    [SerializeField] private GameSettings GameSettings;

    [SerializeField] private Shop Shop;

    [SerializeField] private UIController UIController;
    [SerializeField] private SoundSlider SoundSlider;
    [SerializeField] private ToggleSwitch[] ToggleSwitches;

    private void Start()
    {
        StartCoroutine(StartGameCoroutine());
    }

    private IEnumerator StartGameCoroutine()
    {
        if (GameSettings != null) GameSettings.Init();
        if (GameManager != null) GameManager.Init();

        yield return new WaitForSecondsRealtime(0.1f);

        if (UIController != null) UIController.Init();
        if (SoundSlider != null) SoundSlider.Init();

        for (int i = 0; i < ToggleSwitches.Length; i++)
        {
            if (ToggleSwitches[i] != null)
                ToggleSwitches[i].Init();
        }

        if (Shop != null) Shop.Init();
    }
}
