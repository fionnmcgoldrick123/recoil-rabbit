using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private float shakeAmount = 0.1f;
    [SerializeField] private float shakeDuration = 0.1f;

    private Vector3 originalPosition;
    private float shakeTimer;

    private void Start()
    {
        originalPosition = transform.localPosition;
    }

    private void Update()
    {
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;

            float x = Random.Range(-1f, 1f) * shakeAmount;
            float y = Random.Range(-1f, 1f) * shakeAmount;

            transform.localPosition = originalPosition + new Vector3(x, y, 0);

            if (shakeTimer <= 0)
            {
                transform.localPosition = originalPosition;
            }
        }
    }

    public void Shake(float intensity = 1f, float duration = -1f)
    {
        shakeAmount = intensity;
        shakeTimer = duration < 0 ? shakeDuration : duration;
    }
}
