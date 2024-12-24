using System;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;

    public static event Action CorrectedMerge;

    public static event Action FailedMerge;

    public static event Action Victoried;

    public static event Action Defeated;

    public static event Action AdStarted;

    public static event Action AdEndet;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public static void OnCorrectedMerge()
    {
        CorrectedMerge?.Invoke();
    }

    public static void OnFailedMerge()
    {
        FailedMerge?.Invoke();
    }

    public static void OnVictoried()
    {
        Victoried?.Invoke();
    }

    public static void OnDefeated()
    {
        Defeated?.Invoke();
    }

    public static void StartAd()
    {
        AdStarted?.Invoke();
    }

    public static void EndAd()
    {
        AdEndet?.Invoke();
    }
}
