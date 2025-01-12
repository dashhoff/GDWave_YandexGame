using UnityEngine;
using UnityEngine.UI;

public class SoundSlider : MonoBehaviour
{
    [SerializeField] private Slider _soundSlider;
    [SerializeField] private Slider _musicSlider;

    public void Init()
    {
        SetSoundValue();
        SetMusicValue();
    }

    public void SaveSoundValue()
    {
        GameSettings.Instance.SoundValue = _soundSlider.value;

        GameSettings.Instance.Save();
    }

    public void SetSoundValue()
    {
        _soundSlider.value = GameSettings.Instance.SoundValue;
    }

    public void SaveMusicValue()
    {
        GameSettings.Instance.MusicValue = _musicSlider.value;

        GameSettings.Instance.Save();
    }

    public void SetMusicValue()
    {
        _musicSlider.value = GameSettings.Instance.MusicValue;
    }
}
