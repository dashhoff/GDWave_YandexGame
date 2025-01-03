using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class UIHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Vector3 _endScale = new Vector3(1.1f, 1.1f, 1.1f);
    [SerializeField] private float _durationToEndScale = 0.2f;
    [SerializeField] private float _durationToStartScale = 0.2f;

    private Vector3 _startScale;

    private void Start()
    {
        _startScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOScale(_endScale, _durationToEndScale).SetEase(Ease.OutBack).SetUpdate(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOScale(_startScale, _durationToStartScale).SetEase(Ease.InBack).SetUpdate(true);
    }
}
