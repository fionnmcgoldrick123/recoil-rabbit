using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private float shakeAmount = 0.1f;
    [SerializeField] private float shakeDuration = 0.1f;
    [SerializeField] private float shakeFrequency = 35f;

    private Vector3 originalPosition;
    private float shakeTimer;
    private float currentDuration;
    private float seedX;
    private float seedY;

    private void Start()
    {
        originalPosition = transform.localPosition;
    }

    private void LateUpdate()
    {
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;

            float progress = currentDuration > 0f ? 1f - (shakeTimer / currentDuration) : 1f;
            float envelope = 1f - progress;
            envelope *= envelope;

            float time = Time.time * shakeFrequency;
            float x = (Mathf.PerlinNoise(seedX, time) - 0.5f) * 2f * shakeAmount * envelope;
            float y = (Mathf.PerlinNoise(seedY, time) - 0.5f) * 2f * shakeAmount * envelope;

            transform.localPosition = originalPosition + new Vector3(x, y, 0);

            if (shakeTimer <= 0)
            {
                transform.localPosition = originalPosition;
            }
        }
    }

    public void Shake(float intensity = 1f, float duration = -1f)
    {
        originalPosition = transform.localPosition;
        shakeAmount = intensity;
        currentDuration = duration < 0 ? shakeDuration : duration;
        shakeTimer = currentDuration;
        seedX = Random.Range(0f, 1000f);
        seedY = Random.Range(0f, 1000f);
    }
}
