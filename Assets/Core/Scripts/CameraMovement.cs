using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _offset = new Vector3(0, 0, -10);
    [SerializeField] private float _smoothSpeed = 0.125f;

    [SerializeField] private bool _YIsStatic = true;

    private Vector3 _velocity = Vector3.zero;

    public void FollowTarget()
    {
        if (_target == null)
        {
            Debug.LogWarning("Target not assigned!");
            return;
        }

        Vector3 targetPosition = _target.position + _offset;

        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref _velocity, _smoothSpeed);

        if (_YIsStatic)
            transform.position = new Vector3(smoothedPosition.x, 0, smoothedPosition.z);
        else
            transform.position = smoothedPosition;
    }

    private void LateUpdate()
    {
        FollowTarget();
    }
}
