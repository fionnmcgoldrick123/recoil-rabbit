using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerAfterImage : MonoBehaviour
{
    [Header("After Image")]
    [Tooltip("Used for color/fade calculations based on horizontal speed.")]
    [SerializeField] private float speedThreshold = 10f;
    [Tooltip("Minimum HORIZONTAL speed required to start spawning afterimages. This prevents afterimages from spawning on jumps alone.")]
    [SerializeField] private float minSpawnSpeed = 12f;
    [SerializeField] private float spawnInterval = 0.05f;
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private float minFadeDurationMultiplier = 0.3f;
    [SerializeField] private Color afterImageColor = new Color(0.5f, 0.75f, 1f, 0.4f);

    [Header("Shotgun Flash")]
    [SerializeField] private Color flashAfterImageColor = new Color(1f, 1f, 1f, 0.8f);
    [SerializeField] private float flashFadeOutDuration = 0.5f;

    [Header("References")]
    [SerializeField] private SpriteRenderer bodyRenderer;
    [SerializeField] private SpriteRenderer headRenderer;

    private PlayerController playerController;
    private Rigidbody2D rb;
    private float spawnTimer;
    private float flashBlend = 0f;
    private PaletteSwapperManager paletteManager;
    private Material silhouetteMaterial;
    private List<SpriteRenderer> spriteRenderers;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();
        paletteManager = FindFirstObjectByType<PaletteSwapperManager>();

        spriteRenderers = new List<SpriteRenderer>();

        // Auto-find renderers if not assigned
        if (bodyRenderer == null)
            bodyRenderer = GetComponentInChildren<SpriteRenderer>();

        if (bodyRenderer != null)
            spriteRenderers.Add(bodyRenderer);

        if (headRenderer != null)
            spriteRenderers.Add(headRenderer);

        Shader sil = Shader.Find("Custom/SpriteSilhouette");
        if (sil != null)
            silhouetteMaterial = new Material(sil);
    }

    private void OnDestroy()
    {
        if (silhouetteMaterial != null)
            Destroy(silhouetteMaterial);
    }

    private void Update()
    {
        if (playerController == null || playerController.IsDead || spriteRenderers.Count == 0 || rb == null)
            return;

        if (flashBlend > 0f)
            flashBlend = Mathf.MoveTowards(flashBlend, 0f, Time.deltaTime / flashFadeOutDuration);

        // Use only horizontal speed - this prevents afterimages from spawning on jumps alone
        float horizontalSpeed = Mathf.Abs(rb.linearVelocity.x);

        // Only spawn afterimages if horizontal speed is well above the minimum spawn threshold
        if (horizontalSpeed < minSpawnSpeed)
        {
            spawnTimer = 0f;
            return;
        }

        float speedFactor = Mathf.InverseLerp(speedThreshold, playerController.MaxOverSpeed, horizontalSpeed);
        float interval = Mathf.Lerp(spawnInterval, spawnInterval * 0.2f, speedFactor);

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            spawnTimer = interval;
            SpawnAfterImage(speedFactor);
        }
    }

    // Creates a ghost at the given sprite's transform with the silhouette material applied.
    private SpriteRenderer CreateGhost(string name, SpriteRenderer sourceRenderer)
    {
        GameObject ghost = new GameObject(name);
        ghost.transform.position = sourceRenderer.transform.position;
        ghost.transform.rotation = sourceRenderer.transform.rotation;
        ghost.transform.localScale = sourceRenderer.transform.lossyScale;

        SpriteRenderer gr = ghost.AddComponent<SpriteRenderer>();
        gr.sprite           = sourceRenderer.sprite;
        gr.flipX            = sourceRenderer.flipX;
        gr.flipY            = sourceRenderer.flipY;
        gr.sortingLayerID   = sourceRenderer.sortingLayerID;
        gr.sortingOrder     = sourceRenderer.sortingOrder - 1;

        if (silhouetteMaterial != null)
            gr.sharedMaterial = silhouetteMaterial;

        return gr;
    }

    private void SpawnAfterImage(float speedFactor)
    {
        // Spawn afterimages for all sprite renderers (body and head)
        foreach (SpriteRenderer renderer in spriteRenderers)
        {
            if (renderer == null) continue;

            SpriteRenderer ghostRenderer = CreateGhost("AfterImage", renderer);

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
    }

    private IEnumerator FadeAndDestroy(SpriteRenderer ghostRenderer, float duration)
    {
        float elapsed = 0f;
        Color startColor  = ghostRenderer.color;

        while (elapsed < duration)
        {
            if (ghostRenderer == null) yield break;

            elapsed += Time.deltaTime;
            float t     = elapsed / duration;
            float alpha = Mathf.Lerp(startColor.a, 0f, t);
            ghostRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        if (ghostRenderer != null)
            Destroy(ghostRenderer.gameObject);
    }

    public void SpawnWhiteAfterImage(float fadeDuration = 0.3f)
    {
        // Spawn white afterimages for all sprite renderers (body and head)
        foreach (SpriteRenderer renderer in spriteRenderers)
        {
            if (renderer == null) continue;

            SpriteRenderer ghostRenderer = CreateGhost("WhiteAfterImage", renderer);
            flashBlend = 1f;
            ghostRenderer.color = flashAfterImageColor;
            StartCoroutine(FadeAndDestroy(ghostRenderer, fadeDuration));
        }
    }
}
