using UnityEngine;

public class ObjectMovement : MonoBehaviour
{
    [SerializeField] private GameObject _target;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private Vector3 _offset;

    private Vector3 _targetPosition = new Vector3(0, 0, -10);

    private void Awake()
    {
        if (_target == null)
            _target = gameObject;
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, _targetPosition, _moveSpeed * Time.deltaTime);
    }

    public void MoveToPoint(Transform point)
    {
        _targetPosition = point.position + _offset;
    }
}
