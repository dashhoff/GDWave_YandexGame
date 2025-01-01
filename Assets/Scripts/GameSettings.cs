using YG;
using NaughtyAttributes;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public static int Money;

    public static int BestScore;

    public static float SoundValue;
    public static bool EffectsEnabled;

    private void Start()
    {
        SetParameters();
    }

    public static void SetParameters()
    {
        Money = YandexGame.savesData.Money;
        BestScore = YandexGame.savesData.BestScore;

        SoundValue = YandexGame.savesData.SoundValue;
        EffectsEnabled = YandexGame.savesData.EffectsEnabled;

        Debug.Log("Money: " + Money);
        Debug.Log("BestScore: " + BestScore);
        Debug.Log("SoundValue: " + SoundValue);
        Debug.Log("EffectsEnabled: " + EffectsEnabled);
    }

    public static void Save()
    {
        YandexGame.savesData.Money = Money;
        YandexGame.savesData.BestScore = BestScore;

        YandexGame.savesData.SoundValue = SoundValue;
        YandexGame.savesData.EffectsEnabled = EffectsEnabled;

        YandexGame.SaveProgress();
    }

    [Button]
    public static void ResetProgress()
    {
        Money = 0;
        BestScore = 0;
        SoundValue = 1;
        EffectsEnabled = true;

        Save();
    }
}
