using UnityEngine;
using DG.Tweening;

public class ObjectMovement : MonoBehaviour
{
    [SerializeField] private GameObject _target;
    [SerializeField] private float _moveDuration = 0.5f;
    [SerializeField] private Vector3 _offset;

    private void Awake()
    {
        if (_target == null)
            _target = gameObject;
    }

    public void MoveToPoint(Transform point)
    {
        Vector3 targetPosition = point.position + _offset;

        _target.transform
            .DOMove(targetPosition, _moveDuration)
            .SetEase(Ease.InOutBack);
    }
}
