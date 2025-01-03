using UnityEngine;
using DG.Tweening;

public class DORotate : MonoBehaviour
{
    [SerializeField] private Vector3 _rotationAngle = new Vector3(0.0f, 0.0f, 90.0f);
    [SerializeField] private float _rotationDuration = 1.0f;
    [SerializeField] private float _delayAfterRotation = 0f;

    [SerializeField] private bool _autoStart = true;
    [SerializeField] private bool _loop = true;

    private void Start()
    {
        if (_autoStart)
            StartRotation();
    }

    private void StartRotation()
    {
        DOTween.Sequence()
            .Append(transform.DORotate(transform.rotation.eulerAngles + _rotationAngle, _rotationDuration, RotateMode.LocalAxisAdd).SetEase(Ease.Linear))
            .AppendInterval(_delayAfterRotation)
            .SetLoops(_loop ? -1 : 0, LoopType.Restart);
    }
}
