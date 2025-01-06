using System;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public static AudioController Instance;

    [SerializeField] private GameObject _soundPrefab;

    [SerializeField] private AudioClip _uiSound;

    [SerializeField] private AudioClip _equipSound;
    [SerializeField] private AudioClip _buySound;
    [SerializeField] private AudioClip _errorSound;

    [SerializeField] private AudioClip _levelCompletedSound;
    [SerializeField] private AudioClip _coinSound;

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
        EventManager.LevelCompleted += PlayLevelSound;

        EventManager.Defeated += PlayDefeatSound;

        EventManager.CoinCollected += CoinCollectedSound;
    }

    private void OnDisable()
    {
        EventManager.LevelCompleted -= PlayLevelSound;

        EventManager.Defeated -= PlayDefeatSound;

        EventManager.CoinCollected -= CoinCollectedSound;
    }

    public void PlayUISound()
    {
        GameObject sound = Instantiate(_soundPrefab);

        AudioSource audioSource = sound.GetComponent<AudioSource>();

        audioSource.clip = _uiSound;
        audioSource.volume = GameSettings.Instance.SoundValue;
        audioSource.Play();
    }

    public void PlayEquipSound()
    {
        GameObject sound = Instantiate(_soundPrefab);

        AudioSource audioSource = sound.GetComponent<AudioSource>();

        audioSource.clip = _equipSound;
        audioSource.volume = GameSettings.Instance.SoundValue;
        audioSource.Play();
    }

    public void PlayBuySound()
    {
        GameObject sound = Instantiate(_soundPrefab);

        AudioSource audioSource = sound.GetComponent<AudioSource>();

        audioSource.clip = _buySound;
        audioSource.volume = GameSettings.Instance.SoundValue;
        audioSource.Play();
    }

    public void PlayErrorSound()
    {
        GameObject sound = Instantiate(_soundPrefab);

        AudioSource audioSource = sound.GetComponent<AudioSource>();

        audioSource.clip = _errorSound;
        audioSource.volume = GameSettings.Instance.SoundValue;
        audioSource.Play();
    }

    public void PlayLevelSound()
    {
        GameObject sound = Instantiate(_soundPrefab);

        AudioSource audioSource = sound.GetComponent<AudioSource>();

        audioSource.transform.position = PlayerMovement.Instance.transform.position;
        audioSource.clip = _levelCompletedSound;
        audioSource.volume = GameSettings.Instance.SoundValue;
        audioSource.Play();
    }

    public void PlayDefeatSound()
    {
        GameObject sound = Instantiate(_soundPrefab);

        AudioSource audioSource = sound.GetComponent<AudioSource>();

        audioSource.transform.position = PlayerMovement.Instance.transform.position;
        audioSource.clip = _defeatSound;
        audioSource.volume = GameSettings.Instance.SoundValue;
        audioSource.Play();
    }

    public void CoinCollectedSound()
    {
        GameObject sound = Instantiate(_soundPrefab);

        AudioSource audioSource = sound.GetComponent<AudioSource>();

        audioSource.transform.position = PlayerMovement.Instance.transform.position;
        audioSource.clip = _coinSound;
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
            case "coin":
                audioSource.clip = _coinSound;
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
