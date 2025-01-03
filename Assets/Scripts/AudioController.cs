using System;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public static AudioController Instance;

    [SerializeField] private GameObject _soundPrefab;

    [SerializeField] private AudioClip _uiSound;
    [SerializeField] private AudioClip _errorSound;

    [SerializeField] private AudioClip _levelCompletedSound;
    [SerializeField] private AudioClip _moneyCollectedSound;

    [SerializeField] private AudioClip _defeatSound;

    private void Awake()
    {
        if(Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        EventManager.Defeated += PlaySound("defeat");
        EventManager.LevelCompleted += PlayLevelSound;
    }

    private void OnDisable()
    {
        EventManager.Defeated -= PlaySound("defeat");
        EventManager.LevelCompleted -= PlayLevelSound;

    }

    public void PlayLevelSound()
    {
        GameObject sound = Instantiate(_soundPrefab);

        AudioSource audioSource = sound.GetComponent<AudioSource>();
        audioSource.clip = _levelCompletedSound;
        audioSource.volume = GameSettings.Instance.SoundValue;
        audioSource.Play();
    }

    public void PlayUISound()
    {
        GameObject sound = Instantiate(_soundPrefab);

        AudioSource audioSource = sound.GetComponent<AudioSource>();
        audioSource.clip = _uiSound;
        audioSource.volume = GameSettings.Instance.SoundValue;
        audioSource.Play();
    }

    public Action PlaySound(string name)
    {
        GameObject sound = Instantiate(_soundPrefab);

        AudioSource audioSource = sound.GetComponent<AudioSource>();

        switch (name)
        {
            case "ui":
                audioSource.clip = _uiSound;
                break;
            case "level":
                audioSource.clip = _levelCompletedSound;
                break;
            case "money":
                audioSource.clip = _moneyCollectedSound;
                break;
            case "error":
                audioSource.clip = _errorSound;
                break;
            case "defeat":
                audioSource.clip = _defeatSound;
                break;
        }

        audioSource.Play();

        return null;
    }
}
