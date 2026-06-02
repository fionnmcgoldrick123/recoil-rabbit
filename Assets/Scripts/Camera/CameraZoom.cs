using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private PlayerController player;

    [Header("Orthographic Size")]
    [SerializeField] private float baseSize = 5f;
    [SerializeField] private float maxSize = 9f;

    [Header("Speed Mapping")]
    [SerializeField] private float minSpeed = 0f;
    [SerializeField] private float maxSpeed = 30f;

    [Header("Smoothing")]
    [SerializeField] private float zoomOutSpeed = 3f;
    [SerializeField] private float zoomInSpeed = 1.5f;
    [SerializeField] private float smoothing = 0.15f;

    private Camera cam;
    private float currentVelocity;
    private float targetSize;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        if (player == null)
            player = FindFirstObjectByType<PlayerController>();

        targetSize = baseSize;

        if (cam != null)
            cam.orthographicSize = baseSize;
    }

    private void LateUpdate()
    {
        if (cam == null || player == null) return;

        float speed = player.Velocity.magnitude;

        float t = Mathf.InverseLerp(minSpeed, maxSpeed, speed);
        targetSize = Mathf.Lerp(baseSize, maxSize, t);

        float lerpSpeed = cam.orthographicSize < targetSize ? zoomOutSpeed : zoomInSpeed;

        cam.orthographicSize = Mathf.SmoothDamp(
            cam.orthographicSize,
            targetSize,
            ref currentVelocity,
            smoothing,
            lerpSpeed
        );
    }
}
