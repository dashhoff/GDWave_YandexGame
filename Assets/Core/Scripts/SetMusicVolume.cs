using UnityEngine;

public class SetMusicVolume : MonoBehaviour
{
    [SerializeField] private AudioSource _sound;

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        _sound.volume = GameSettings.Instance.MusicValue;

        if (!_sound.isPlaying)
            _sound.Play();
    }

    public void Init()
    {
        if (_sound == null)
            _sound = GetComponent<AudioSource>();

        _sound.volume = GameSettings.Instance.MusicValue;
        _sound.Play();
    }
}
