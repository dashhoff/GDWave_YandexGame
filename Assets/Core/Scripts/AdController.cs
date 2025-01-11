using UnityEngine;
using YG;

public class AdController : MonoBehaviour
{
    public static AdController Instance;

    [SerializeField] private int _rewardMoney = 10;

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
    private void OnDisable()
    {
        YandexGame.RewardVideoEvent -= Rewarded;
    }

    public void FullScreenAd()
    {
        YandexGame.FullscreenShow();
    }

    private void Rewarded(int id)
    {
        switch (id)
        {
            case 1:
                GameManager.Instance.AddMoney(_rewardMoney);
                UIController.Instance.UpdateMoneyText();
                break;
        }
    }

    public void OpenRewardAd(int id)
    {
        YandexGame.RewVideoShow(id);
    }
}


