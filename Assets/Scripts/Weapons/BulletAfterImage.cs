using UnityEngine;
using System.Collections;

public class BulletAfterImage : MonoBehaviour
{
    [Header("After Image")]
    // 
    [SerializeField] private float speedThreshold = 5f;
    [SerializeField] private float spawnInterval = 0.05f;
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private float minFadeDurationMultiplier = 0.3f;
    [SerializeField] private Color afterImageColor = new Color(1f, 0.5f, 0.5f, 0.4f);

    private SpriteRenderer sourceRenderer;
    private Rigidbody2D rb;
    private float spawnTimer;
    private bool isActive = true;
    private PaletteSwapperManager paletteManager;

    private void Awake()
    {
        sourceRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        paletteManager = FindFirstObjectByType<PaletteSwapperManager>();
    }

    private void Update()
    {
        if (!isActive || sourceRenderer == null || rb == null)
            return;

        // Use velocity magnitude
        float speed = rb.linearVelocity.magnitude;

        if (speed <= speedThreshold)
        {
            spawnTimer = 0f;
            return;
        }

        float interval = spawnInterval;
        spawnTimer -= Time.deltaTime;
        
        if (spawnTimer <= 0f)
        {
            spawnTimer = interval;
            SpawnAfterImage();
        }
    }

    private void SpawnAfterImage()
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

        // Use palette manager's afterimage color if available, otherwise use serialized value
        Color currentAfterImageColor = (paletteManager != null) 
            ? paletteManager.GetCurrentAfterImageColor() 
            : afterImageColor;

        ghostRenderer.color = currentAfterImageColor;

        StartCoroutine(FadeAndDestroy(ghostRenderer, fadeDuration));
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

        ghostRenderer.color = new Color(1f, 1f, 1f, 0.6f);
        StartCoroutine(FadeAndDestroy(ghostRenderer, fadeDuration));
    }

    public void Deactivate()
    {
        isActive = false;
    }
}
