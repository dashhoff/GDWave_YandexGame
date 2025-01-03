using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private int _score;

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
    }

    private void OnDisable()
    {
        EventManager.Defeated -= Defeat;
    }

    public void Init()
    {

    }

    public void LevelCompleted()
    {
        EventManager.OnCompleteLevel();
    }

    public void AddScore()
    {
        _score++;
    }

    public void AddMoney()
    {
        GameSettings.Instance.Money++;
    }

    public void Defeat()
    {
        if (_score > GameSettings.Instance.BestScore)
            GameSettings.Instance.BestScore = _score;

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
