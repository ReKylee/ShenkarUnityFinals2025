using UnityEngine;

public class CameraFollowBounded : MonoBehaviour
{
    public Transform followTarget;
    public BoxCollider2D mapBounds;
    public float smoothSpeed = 0.5f;

    private Camera _cam;
    private float _camHalfHeight;
    private float _camHalfWidth;

    private Vector3 _velocity = Vector3.zero;

    private void Start()
    {
        _cam = GetComponent<Camera>();
        _camHalfHeight = _cam.orthographicSize;
        _camHalfWidth = _camHalfHeight * _cam.aspect;
    }

    private void FixedUpdate()
    {
        if (!followTarget || !mapBounds) return;

        Vector3 targetPos = followTarget.position;
        float clampedX = Mathf.Clamp(targetPos.x, mapBounds.bounds.min.x + _camHalfWidth,
            mapBounds.bounds.max.x - _camHalfWidth);

        float clampedY = Mathf.Clamp(targetPos.y, mapBounds.bounds.min.y + _camHalfHeight,
            mapBounds.bounds.max.y - _camHalfHeight);

        Vector3 desiredPos = new(clampedX, clampedY, transform.position.z);
        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref _velocity, smoothSpeed);
    }
}
