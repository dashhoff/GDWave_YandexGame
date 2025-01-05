using YG;
using NaughtyAttributes;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance;

    public bool SkipAd;

    public int Money;

    public int BestScore;

    public int PlayerSkinId;

    public float SoundValue;
    public bool EffectsEnabled;

    private void Start()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void Init()
    {
        SetParameters();
    }

    public void SetParameters()
    {
        SkipAd = YandexGame.savesData.SkipAd;

        Money = YandexGame.savesData.Money;
        BestScore = YandexGame.savesData.BestScore;

        PlayerSkinId = YandexGame.savesData.PlayerSkinId;

        SoundValue = YandexGame.savesData.SoundValue;
        EffectsEnabled = YandexGame.savesData.EffectsEnabled;

        Debug.Log("Money: " + Money);
        Debug.Log("BestScore: " + BestScore);
        Debug.Log("SoundValue: " + SoundValue);
        Debug.Log("EffectsEnabled: " + EffectsEnabled);
    }

    [Button]
    public void Save()
    {
        YandexGame.savesData.SkipAd = SkipAd;

        YandexGame.savesData.Money = Money;
        YandexGame.savesData.BestScore = BestScore;

        YandexGame.savesData.PlayerSkinId = PlayerSkinId;

        YandexGame.savesData.SoundValue = SoundValue;
        YandexGame.savesData.EffectsEnabled = EffectsEnabled;

        YandexGame.SaveProgress();
    }

    [Button]
    public void ResetProgress()
    {
        SkipAd = false;

        Money = 0;
        BestScore = 0;

        PlayerSkinId = 0;

        SoundValue = 1;
        EffectsEnabled = true;

        Save();
    }
}
