using UnityEngine;
using DG.Tweening;

public class PulseEffect : MonoBehaviour
{
    [SerializeField] private Vector3 _startScale = new Vector3(1.0f, 1.0f, 1.0f);
    [SerializeField] private float _startPhaseDuration = 0.5f;
    [SerializeField] private float _delayAfterStartPhase = 0.1f;

    [SerializeField] private Vector3 _endScale = new Vector3(1.2f, 1.2f, 1.2f);
    [SerializeField] private float _endPhaseDuration = 0.5f;
    [SerializeField] private float _delayAfterEndPhase = 0.1f;

    [SerializeField] private bool _autoStart = true;
    [SerializeField] private bool _loop = true;

    private void Start()
    {
        if (_autoStart)
            StartPulse();
    }

    private void StartPulse()
    {
        DOTween.Sequence()
            .Append(transform.DOScale(_endScale, _endPhaseDuration).SetEase(Ease.InOutSine))
            .AppendInterval(_delayAfterEndPhase)
            .Append(transform.DOScale(_startScale, _startPhaseDuration).SetEase(Ease.InOutSine))
            .AppendInterval(_delayAfterStartPhase)
            .SetLoops(_loop ? -1 : 0, LoopType.Restart);
    }
}
