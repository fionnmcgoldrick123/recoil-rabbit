using UnityEngine;
using System.Collections;

public class PlayerAfterImage : MonoBehaviour
{
    [Header("After Image")]
    [SerializeField] private float speedThreshold = 10f;
    [SerializeField] private float spawnInterval = 0.05f;
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private float minFadeDurationMultiplier = 0.3f;
    [SerializeField] private Color afterImageColor = new Color(0.5f, 0.75f, 1f, 0.4f);

    [Header("Shotgun Flash")]
    [SerializeField] private Color flashAfterImageColor = new Color(1f, 1f, 1f, 0.8f);
    [SerializeField] private float flashFadeOutDuration = 0.5f;

    private PlayerController playerController;
    private SpriteRenderer sourceRenderer;
    private Rigidbody2D rb;
    private float spawnTimer;
    private float flashBlend = 0f;
    private PaletteSwapperManager paletteManager;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        sourceRenderer = GetComponentInChildren<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        paletteManager = FindFirstObjectByType<PaletteSwapperManager>();
    }

    private void Update()
    {
        if (playerController == null || playerController.IsDead || sourceRenderer == null || rb == null)
            return;

        if (flashBlend > 0f)
            flashBlend = Mathf.MoveTowards(flashBlend, 0f, Time.deltaTime / flashFadeOutDuration);

        float speed = rb.linearVelocity.magnitude;

        if (speed <= speedThreshold)
        {
            spawnTimer = 0f;
            return;
        }

        float speedFactor = Mathf.InverseLerp(speedThreshold, playerController.MaxOverSpeed, speed);
        float interval = Mathf.Lerp(spawnInterval, spawnInterval * 0.2f, speedFactor);

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            spawnTimer = interval;
            SpawnAfterImage(speedFactor);
        }
    }

    private void SpawnAfterImage(float speedFactor)
    {
        GameObject ghost = new GameObject("AfterImage");
        ghost.transform.position = sourceRenderer.transform.position;
        ghost.transform.rotation = sourceRenderer.transform.rotation;
        ghost.transform.localScale = sourceRenderer.transform.lossyScale;

        SpriteRenderer ghostRenderer = ghost.AddComponent<SpriteRenderer>();
        ghostRenderer.sprite = sourceRenderer.sprite;
        ghostRenderer.flipX = sourceRenderer.flipX;
        ghostRenderer.flipY = sourceRenderer.flipY;
        ghostRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
        ghostRenderer.sortingOrder = sourceRenderer.sortingOrder - 1;

        Color currentAfterImageColor = (paletteManager != null) 
            ? paletteManager.GetCurrentAfterImageColor() 
            : afterImageColor;

        Color c = currentAfterImageColor;
        c.a = Mathf.Lerp(0.2f, currentAfterImageColor.a, speedFactor);
        c = Color.Lerp(c, new Color(flashAfterImageColor.r, flashAfterImageColor.g, flashAfterImageColor.b, c.a), flashBlend);
        ghostRenderer.color = c;

        float scaledFadeDuration = Mathf.Lerp(fadeDuration, fadeDuration * minFadeDurationMultiplier, speedFactor);
        StartCoroutine(FadeAndDestroy(ghostRenderer, scaledFadeDuration));
    }

    private IEnumerator FadeAndDestroy(SpriteRenderer ghostRenderer, float duration)
    {
        float elapsed = 0f;
        Color startColor = ghostRenderer.color;

        while (elapsed < duration)
        {
            if (ghostRenderer == null)
                yield break;

            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, 0f, elapsed / duration);
            ghostRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        if (ghostRenderer != null)
            Destroy(ghostRenderer.gameObject);
    }

    public void SpawnWhiteAfterImage(float fadeDuration = 0.3f)
    {
        if (sourceRenderer == null)
            return;

        GameObject ghost = new GameObject("WhiteAfterImage");
        ghost.transform.position = sourceRenderer.transform.position;
        ghost.transform.rotation = sourceRenderer.transform.rotation;
        ghost.transform.localScale = sourceRenderer.transform.lossyScale;

        SpriteRenderer ghostRenderer = ghost.AddComponent<SpriteRenderer>();
        ghostRenderer.sprite = sourceRenderer.sprite;
        ghostRenderer.flipX = sourceRenderer.flipX;
        ghostRenderer.flipY = sourceRenderer.flipY;
        ghostRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
        ghostRenderer.sortingOrder = sourceRenderer.sortingOrder - 1;

        flashBlend = 1f;
        ghostRenderer.color = flashAfterImageColor;
        StartCoroutine(FadeAndDestroy(ghostRenderer, fadeDuration));
    }
}
