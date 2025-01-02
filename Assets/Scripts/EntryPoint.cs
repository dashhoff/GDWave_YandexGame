using NUnit.Framework;
using UnityEngine;

public class EntryPoint : MonoBehaviour
{
    [SerializeField] private GameManager GameManager;
    [SerializeField] private GameSettings GameSettings;

    [SerializeField] private UIController UIController;
    [SerializeField] private SoundSlider SoundSlider;
    [SerializeField] private ToggleSwitch[] ToggleSwitches;

    private void Start()
    {
        GameSettings.Init();
        GameManager.Init();

        UIController.Init();
        SoundSlider.Init();

        for (int i = 0; i < ToggleSwitches.Length; i++)
        {
            ToggleSwitches[i].Init();
        }

    }
}
