using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ToggleSwitch : MonoBehaviour
{
    [Space(10f)]
    [SerializeField] private bool _enabled;
    [SerializeField, Tooltip("Задать либо 'Эффекты', либо ''")] private string _settingName;

    [Space(10f)]
    [Header("Ссылки на изображения")]
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Image _handleImage;

    [Space(10f)]
    [Header("Ссылки на точки для хэндла")]
    [SerializeField] private Vector2 _onHandlePoint;
    [SerializeField] private Vector2 _offHandlePoint;

    [Space(10f)]
    [Header("Настройка времени для переключния")]
    [SerializeField] private float _durationToToggle = 0.5f;

    [Space(10f)]
    [Header("Настройка цвета")]
    [SerializeField] private Color _enabledColor;
    [SerializeField] private Color _disabledColor;

    public void Switch()
    {
        _enabled = !_enabled;

        UpdateToggle();
    }

    public void Init()
    {
        GetInfo();

        UpdateToggle();
    }

    public void UpdateToggle()
    {
        if (_enabled)
        {
            MoveHandle(_onHandlePoint);

            SetColor(_enabledColor);

            SetInfo();
        }
        else
        {
            MoveHandle(_offHandlePoint);

            SetColor(_disabledColor);

            SetInfo();
        }
    }

    private void GetInfo()
    {
        if (_settingName == "Эффекты")
        {
            if (GameSettings.Instance.EffectsEnabled)
                _enabled = true;
            else
                _enabled = false;
        }
    }

    private void SetInfo()
    {
        if (_settingName == "Эффекты")
        {
            if (_enabled)
                GameSettings.Instance.EffectsEnabled = true;
            else
                GameSettings.Instance.EffectsEnabled = false;

            GameSettings.Instance.Save();
        }
    }

    private void MoveHandle(Vector2 endPoint)
    {
        DOTween.Sequence()
            .Append(_handleImage.rectTransform.DOAnchorPos(endPoint, _durationToToggle));
    }

    private void SetColor(Color endColor)
    {
        DOTween.Sequence()
            .Append(_backgroundImage.DOColor(endColor, _durationToToggle));
    }
}
