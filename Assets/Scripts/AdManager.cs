using UnityEngine;
using YG;

public class AdManager : MonoBehaviour
{
    public static AdManager Instance;

    [SerializeField] private int _moneyAfterVictory = 15;
    [SerializeField] private int _timeAfterDefeat = 60;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        YandexGame.RewardVideoEvent += Rewarded;
    }

    // Отписываемся от события открытия рекламы в OnDisable
    private void OnDisable()
    {
        YandexGame.RewardVideoEvent -= Rewarded;
    }

    private void Rewarded(int id)
    {
        switch (id)
        {
            case 1:
                AddMoney(_moneyAfterVictory);
                break;
        }
    }

    public void OpenRewardAd(int id)
    {
        YandexGame.RewVideoShow(id);
    }

    private void AddMoney(int value)
    {
    }
}


