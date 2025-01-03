using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int Score;

    private void Awake()
    {
        //Application.targetFrameRate = 120;

        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        EventManager.Defeated += Defeat;

        EventManager.LevelCompleted += AddScore;
        EventManager.CoinCollected += AddMoney;
    }

    private void OnDisable()
    {
        EventManager.Defeated -= Defeat;

        EventManager.LevelCompleted -= AddScore;
        EventManager.CoinCollected -= AddMoney;
    }

    public void Init()
    {

    }

    public void AddScore()
    {
        Score++;

        UIController.Instance.UpdateScoreText();
    }

    public void AddMoney()
    {
        GameSettings.Instance.Money++;
        GameSettings.Instance.Save();

        UIController.Instance.UpdateMoneyText();
    }

    public void Defeat()
    {
        if (Score > GameSettings.Instance.BestScore)
            GameSettings.Instance.BestScore = Score;

        GameSettings.Instance.Save();
    }

    public void PauseOn()
    {
        Time.timeScale = 0;

        EventManager.OnPause();
    }

    public void PauseOff() 
    {
        Time.timeScale = 1f;

        EventManager.OffPause();
    }
}
