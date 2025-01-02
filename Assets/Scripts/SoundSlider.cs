using UnityEngine;
using UnityEngine.UI;

public class SoundSlider : MonoBehaviour
{
    [SerializeField] private Slider _slider;

    public void Init()
    {
        SetValue();
    }

    public void SaveValue()
    {
        GameSettings.Instance.SoundValue = _slider.value;

        GameSettings.Instance.Save();
    }

    public void SetValue()
    {
        _slider.value = GameSettings.Instance.SoundValue;
    }
}
