using System;
using UnityEngine;

public static class EventManager
{
    //public static EventManager Instance;

    public static event Action GameStarted;

    public static event Action GameStoped;

    public static event Action LevelSpawned;

    public static event Action Victoried;

    public static event Action Defeated;

    public static event Action AdStarted;

    public static event Action AdEndet;

    /*private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }*/
    public static void OnVictoried()
    {
        Victoried?.Invoke();
    }

    public static void OnDefeated()
    {
        Defeated?.Invoke();
    }

    public static void OnSpawnLevel()
    {
        LevelSpawned?.Invoke();
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
