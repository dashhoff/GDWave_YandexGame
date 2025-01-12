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
    public int[] IntOpenSkins;

    public float SoundValue;
    public float MusicValue;

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

        for (int i = 0; i < IntOpenSkins.Length; i++)
        {
            IntOpenSkins[i] = YandexGame.savesData.IntOpenSkins[i];
        }

        SoundValue = YandexGame.savesData.SoundValue;
        MusicValue = YandexGame.savesData.MusicValue;
        EffectsEnabled = YandexGame.savesData.EffectsEnabled;

        Debug.Log("Money: " + Money);
        Debug.Log("BestScore: " + BestScore);
        Debug.Log("SoundValue: " + SoundValue);
        Debug.Log("MusicValue: " + MusicValue);
        Debug.Log("EffectsEnabled: " + EffectsEnabled);
    }

    [Button]
    public void Save()
    {
        YandexGame.savesData.SkipAd = SkipAd;

        YandexGame.savesData.Money = Money;
        YandexGame.savesData.BestScore = BestScore;

        YandexGame.savesData.PlayerSkinId = PlayerSkinId;

        for (int i = 0; i < IntOpenSkins.Length; i++)
        {
            YandexGame.savesData.IntOpenSkins[i] = IntOpenSkins[i];
        }

        YandexGame.savesData.SoundValue = SoundValue;
        YandexGame.savesData.MusicValue =MusicValue;
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

        for (int i = 0; i < IntOpenSkins.Length; i++)
        {
            IntOpenSkins[i] = 0;
        }
        IntOpenSkins[0] = 1;

        SoundValue = 1;
        MusicValue = 1;
        EffectsEnabled = true;

        Save();
    }
}
