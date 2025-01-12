
namespace YG
{
    [System.Serializable]
    public class SavesYG
    {
        // "Технические сохранения" для работы плагина (Не удалять)
        public int idSave;
        public bool isFirstSession = true;
        public string language = "ru";
        public bool promptDone;

        // Ваши сохранения

        public bool SkipAd = false;

        public int Money = 10;
        public int BestScore = 0;

        public bool[] OpenSkins = {true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false};
        public int[] IntOpenSkins = { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        public int PlayerSkinId = 0;

        public float SoundValue = 1;
        public float MusicValue = 1;
        public bool EffectsEnabled = true;

        // Поля (сохранения) можно удалять и создавать новые. При обновлении игры сохранения ломаться не должны

        // Вы можете выполнить какие то действия при загрузке сохранений
        public SavesYG()
        {
            OpenSkins[0] = true;
        }
    }
}
