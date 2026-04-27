using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

    [Header("Follow")]
    [SerializeField] private float smoothSpeed = 8f;

    [Header("Shake Boost")]
    [SerializeField] private float shakeMultiplier = 1.5f;

    private CameraShake cameraShake;

    private void Awake()
    {
        cameraShake = GetComponent<CameraShake>();
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = target.position + offset;

        Vector3 smoothPosition = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );

        Vector3 shakeOffset = cameraShake != null
            ? cameraShake.ShakeOffset * shakeMultiplier
            : Vector3.zero;

        transform.position = smoothPosition + shakeOffset;
    }
}