using UnityEngine;
using UnityEngine.UI;

public class SoundSlider : MonoBehaviour
{
    [SerializeField] private Slider _slider;

    private void Start()
    {
        SetValue();
    }

    public void SaveValue()
    {
        GameSettings.SoundValue = _slider.value;
    }

    public void SetValue()
    {
        _slider.value = GameSettings.SoundValue;
    }
}
