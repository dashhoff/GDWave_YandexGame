using NaughtyAttributes;
using YG;

public static class GameSettings
{
    public static int Money;

    public static int BestScore;

    public static void Save()
    {
        YandexGame.savesData.Money = Money;
        YandexGame.savesData.BestScore = BestScore;

        YandexGame.SaveProgress();
    }

    [Button]
    public static void ResetProgress()
    {
        Money = 0;
        BestScore = 0;

        Save();
    }
}
