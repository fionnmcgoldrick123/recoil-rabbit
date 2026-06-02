using UnityEngine;
using System.Collections.Generic;

public class Crate : MonoBehaviour
{
    [Header("Shells")]
    [SerializeField] private GameObject shotgunShellPrefab;
    [SerializeField] private int shellCount = 3;

    [Header("Piece Physics")]
    [SerializeField] private float pieceExplosionForce = 6f;
    [Tooltip("Max spin speed of a broken piece in degrees per second. Higher = more chaotic tumbling.")]
    [SerializeField] private float pieceTorqueRange = 250f;
    [SerializeField] private float pieceGravityScale = 4f;
    [SerializeField] private float pieceDestroyDelay = 4f;
    [SerializeField] private float pieceFadeDuration = 0.5f;

    [Header("Shell Launch")]
    [Tooltip("How fast shells fly out of the crate on break.")]
    [SerializeField] private float shellLaunchSpeed = 7f;
    [Tooltip("0 = fully random direction, 1 = straight up. Prevents shells launching into the ground.")]
    [Range(0f, 1f)]
    [SerializeField] private float shellLaunchUpwardBias = 0.4f;

    private bool broken = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (broken) return;
        if (other.GetComponent<Bullet>() == null) return;

        Break();
    }

    private void Break()
    {
        broken = true;

        // Collect children first to avoid modifying the collection mid-iteration
        List<Transform> pieces = new List<Transform>();
        foreach (Transform child in transform)
            pieces.Add(child);

        foreach (Transform piece in pieces)
        {
            piece.SetParent(null);

            Rigidbody2D rb = piece.GetComponent<Rigidbody2D>();
            if (rb == null)
                rb = piece.gameObject.AddComponent<Rigidbody2D>();

            // Gravity is applied immediately — no delay
            rb.gravityScale = pieceGravityScale;

            Vector2 dir = (Vector2)(piece.position - transform.position);
            if (dir.sqrMagnitude < 0.001f)
                dir = Random.insideUnitCircle;
            dir = dir.normalized;

            float force = Random.Range(pieceExplosionForce * 0.6f, pieceExplosionForce * 1.4f);
            rb.linearVelocity = dir * force;
            rb.angularVelocity = Random.Range(-pieceTorqueRange, pieceTorqueRange);

            StartCoroutine(FadeOutAndDestroy(piece.gameObject, pieceDestroyDelay, pieceFadeDuration));
        }

        // Spawn shotgun shells
        if (shotgunShellPrefab != null)
        {
            for (int i = 0; i < shellCount; i++)
            {
                Vector2 spawnPos = (Vector2)transform.position + Random.insideUnitCircle * 0.15f;
                GameObject shell = Instantiate(shotgunShellPrefab, spawnPos, Quaternion.identity);
                ShotgunShellPickup pickup = shell.GetComponent<ShotgunShellPickup>();
                if (pickup != null)
                {
                    Vector2 dir = Random.insideUnitCircle.normalized;
                    dir.y = Mathf.Lerp(dir.y, 1f, shellLaunchUpwardBias);
                    pickup.Launch(dir.normalized, shellLaunchSpeed);
                }
            }
        }

        // Destroy the crate parent after all fades complete
        Destroy(gameObject, pieceDestroyDelay + pieceFadeDuration + 0.1f);
    }

    private System.Collections.IEnumerator FadeOutAndDestroy(GameObject obj, float waitTime, float fadeDuration)
    {
        yield return new WaitForSeconds(waitTime);

        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            float elapsed = 0f;
            Color originalColor = sr.color;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(originalColor.a, 0f, elapsed / fadeDuration);
                sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }
            sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        }

        Destroy(obj);
    }
}
