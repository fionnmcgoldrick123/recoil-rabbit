using UnityEngine;
using System.Collections;

public class BulletAfterImage : MonoBehaviour
{
    [Header("After Image")]
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
    private Material silhouetteMaterial;

    private void Awake()
    {
        sourceRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        paletteManager = FindFirstObjectByType<PaletteSwapperManager>();

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
        if (!isActive || sourceRenderer == null || rb == null)
            return;

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

    private SpriteRenderer CreateGhost()
    {
        GameObject ghost = new GameObject("AfterImage");
        ghost.transform.position  = sourceRenderer.transform.position;
        ghost.transform.rotation  = sourceRenderer.transform.rotation;
        ghost.transform.localScale = sourceRenderer.transform.lossyScale;

        SpriteRenderer gr = ghost.AddComponent<SpriteRenderer>();
        gr.sprite         = sourceRenderer.sprite;
        gr.flipX          = sourceRenderer.flipX;
        gr.flipY          = sourceRenderer.flipY;
        gr.sortingLayerID = sourceRenderer.sortingLayerID;
        gr.sortingOrder   = sourceRenderer.sortingOrder - 1;

        if (silhouetteMaterial != null)
            gr.sharedMaterial = silhouetteMaterial;

        return gr;
    }

    private void SpawnAfterImage()
    {
        SpriteRenderer ghostRenderer = CreateGhost();

        Color currentAfterImageColor = (paletteManager != null)
            ? paletteManager.GetCurrentAfterImageColor()
            : afterImageColor;

        ghostRenderer.color = currentAfterImageColor;
        StartCoroutine(FadeAndDestroy(ghostRenderer, fadeDuration));
    }

    private IEnumerator FadeAndDestroy(SpriteRenderer ghostRenderer, float duration)
    {
        float elapsed   = 0f;
        Color startColor  = ghostRenderer.color;
        Vector3 startScale = ghostRenderer.transform.localScale;

        while (elapsed < duration)
        {
            if (ghostRenderer == null) yield break;

            elapsed += Time.deltaTime;
            float t     = elapsed / duration;
            float alpha = Mathf.Lerp(startColor.a, 0f, t);
            ghostRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            ghostRenderer.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            yield return null;
        }

        if (ghostRenderer != null)
            Destroy(ghostRenderer.gameObject);
    }

    public void SpawnWhiteAfterImage(float fadeDuration = 0.3f)
    {
        if (sourceRenderer == null) return;

        SpriteRenderer ghostRenderer = CreateGhost();
        ghostRenderer.color = new Color(1f, 1f, 1f, 0.6f);
        StartCoroutine(FadeAndDestroy(ghostRenderer, fadeDuration));
    }

    public void Deactivate()
    {
        isActive = false;
    }
}
