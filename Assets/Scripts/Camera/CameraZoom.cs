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

    [Header("Cannon Zoom")]
    [SerializeField] private float cannonZoomOutSize = 7f;
    [SerializeField] private float cannonZoomDuration = 0.4f;

    private Camera cam;
    private float currentVelocity;
    private float targetSize;
    private Coroutine cannonZoomRoutine;
    private bool isCannonZooming = false;

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

        // Skip normal zoom calculations while cannon zoom is active
        if (isCannonZooming) return;

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

    public void ZoomOutForCannon()
    {
        isCannonZooming = true;
        if (cannonZoomRoutine != null)
            StopCoroutine(cannonZoomRoutine);
        cannonZoomRoutine = StartCoroutine(CannonZoomCoroutine(cannonZoomOutSize, cannonZoomDuration, isZoomingIn: false));
    }

    public void ZoomInFromCannon()
    {
        if (cannonZoomRoutine != null)
            StopCoroutine(cannonZoomRoutine);
        cannonZoomRoutine = StartCoroutine(CannonZoomCoroutine(baseSize, cannonZoomDuration, isZoomingIn: true));
    }

    private System.Collections.IEnumerator CannonZoomCoroutine(float targetSizeZoom, float duration, bool isZoomingIn)
    {
        if (cam == null) yield break;

        float startSize = cam.orthographicSize;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            cam.orthographicSize = Mathf.Lerp(startSize, targetSizeZoom, t);
            yield return null;
        }

        cam.orthographicSize = targetSizeZoom;
        cannonZoomRoutine = null;
        
        // Only re-enable normal zoom after zooming IN (back to normal)
        if (isZoomingIn)
            isCannonZooming = false;
    }
}
