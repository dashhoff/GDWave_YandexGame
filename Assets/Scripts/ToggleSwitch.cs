using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ToggleSwitch : MonoBehaviour
{
    [Space(10f)]
    [SerializeField] private bool _enabled;
    [SerializeField, Tooltip("������ ���� '�������', ���� ''")] private string _settingName;

    [Space(10f)]
    [Header("������ �� �����������")]
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Image _handleImage;

    [Space(10f)]
    [Header("������ �� ����� ��� ������")]
    [SerializeField] private Vector2 _onHandlePoint;
    [SerializeField] private Vector2 _offHandlePoint;

    [Space(10f)]
    [Header("��������� ������� ��� �����������")]
    [SerializeField] private float _durationToToggle = 0.5f;

    [Space(10f)]
    [Header("��������� �����")]
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
        if (_settingName == "�������")
        {
            if (GameSettings.Instance.EffectsEnabled)
                _enabled = true;
            else
                _enabled = false;
        }
    }

    private void SetInfo()
    {
        if (_settingName == "�������")
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
