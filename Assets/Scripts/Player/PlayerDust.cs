using System.Collections.Generic;
using UnityEngine;

public class PlayerDust : MonoBehaviour
{

    // ── Inner type ────────────────────────────────────────────────────
    private class Particle
    {
        public Vector2 position;
        public Vector2 velocity;
        public float   lifetime;
        public float   maxLifetime;
        public float   startSize;
        public float   gravity;
        public Color   startColor;
        public bool    active;
        public Transform  t;
        public SpriteRenderer sr;
    }

    // ── Pool / Rendering ──────────────────────────────────────────────
    [Header("Pool & Rendering")]
    [Tooltip("Total particles shared by every effect. Increase if effects get cut off.")]
    [SerializeField] private int poolSize = 150;
    [Tooltip("Optional sprite. Leave empty to use a generated 4x4 white square.")]
    [SerializeField] private Sprite particleSprite;
    [Tooltip("Sorting layer name for all particles.")]
    [SerializeField] private string sortingLayerName = "Default";
[Tooltip("Sorting order within that layer.")]
    [SerializeField] private int sortingOrder = 5;

    // ── Spawn Offsets ──────────────────────────────────────────────────
    [Header("Spawn Offsets")]
    [Tooltip("World-space offset from player pivot to feet (ground effects).")]
    [SerializeField] private Vector2 feetOffset = new Vector2(0f, -0.5f);
    [Tooltip("World-space horizontal distance from player pivot to wall contact point.")]
    [SerializeField] private float wallSideOffset = 0.25f;
    [Tooltip("World-space vertical range for wall spark spawn (random between -wallHeightRange and +wallHeightRange).")]
    [SerializeField] private float wallHeightRange = 0.2f;
    [Tooltip("Y offset from pivot when wall-sliding UPWARD (near feet).")]
    [SerializeField] private float wallSlideUpY = -0.4f;
    [Tooltip("Y offset from pivot when wall-sliding DOWNWARD (near or above head).")]
    [SerializeField] private float wallSlideDownY = 0.4f;

    // ── Run Dust ──────────────────────────────────────────────────────
    [Header("Run Dust")]
    [SerializeField] private float runEmitRate       = 12f;
    [SerializeField] private float runSpeedThreshold = 3f;
    [SerializeField] private float runLifetime       = 0.25f;
    [SerializeField] private float runSpread         = 0.15f;
    [SerializeField] private float runSpeedMin       = 0.5f;
    [SerializeField] private float runSpeedMax       = 2f;
    [SerializeField] private Color runColor          = new Color(0.85f, 0.80f, 0.70f, 0.7f);
    [SerializeField] private float runGravity        = 1.5f;
    [Tooltip("Upward kick speed range for run dust particles.")]
    [SerializeField] private float runUpSpeedMin     = 0.3f;
    [SerializeField] private float runUpSpeedMax     = 2.5f;

    // ── Land Dust ─────────────────────────────────────────────────────
    [Header("Land Dust")]
    [SerializeField] private int   landParticleCount  = 8;
    [SerializeField] private float landLifetime       = 0.35f;
    [SerializeField] private float landSpreadX        = 0.3f;
    [SerializeField] private float landSpeedMin       = 1.5f;
    [SerializeField] private float landSpeedMax       = 4f;
    [SerializeField] private Color landColor          = new Color(0.85f, 0.80f, 0.70f, 0.9f);
    [SerializeField] private float landGravity        = 2f;

    // ── Jump Puff ─────────────────────────────────────────────────────
    [Header("Jump Puff")]
    [SerializeField] private int   jumpParticleCount = 6;
    [SerializeField] private float jumpLifetime      = 0.3f;
    [SerializeField] private float jumpSpeedMin      = 1f;
    [SerializeField] private float jumpSpeedMax      = 3f;
    [SerializeField] private Color jumpColor         = new Color(0.85f, 0.80f, 0.70f, 0.7f);
    [SerializeField] private float jumpGravity       = 1f;

    // ── Wall Slide Sparks ─────────────────────────────────────────────
    [Header("Wall Slide Sparks")]
    [SerializeField] private float wallEmitRate  = 10f;
    [SerializeField] private float wallLifetime  = 0.2f;
    [SerializeField] private float wallSpeedMin  = 1f;
    [SerializeField] private float wallSpeedMax  = 3f;
    [SerializeField] private Color wallColor     = new Color(1f, 0.85f, 0.4f, 0.9f);
    [SerializeField] private float wallGravity   = 3f;

    // ── Particle size ────────────────────────────────────────────────
    private const float PIXEL_SIZE = 1.0f;  // 1x1 sprite displays as 1 pixel at its PPU
    private PlayerController playerController;
    private Rigidbody2D      rb;
    private List<Particle>   pool;
    private Transform        poolParent;
    private float            runEmitTimer;
    private float            wallEmitTimer;
    private bool             wasGroundedPrev;
    private float            prevVerticalVelocity;

    // ── Init ──────────────────────────────────────────────────────────
    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        rb               = GetComponent<Rigidbody2D>();

        // Clean up any pre-existing pools to avoid duplicates
        Transform existingPool = transform.Find("[DustPool]");
        if (existingPool != null)
            Destroy(existingPool.gameObject);

        // Create fresh pool
        poolParent = new GameObject("[DustPool]").transform;
        poolParent.SetParent(transform);
        poolParent.localPosition = Vector3.zero;

        Sprite sprite = particleSprite != null ? particleSprite : CreateFallbackSprite();

        pool = new List<Particle>(poolSize);
        for (int i = 0; i < poolSize; i++)
        {
            var go = new GameObject("p");
            go.transform.SetParent(poolParent);
            go.SetActive(false);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite        = sprite;
            sr.sortingLayerName = sortingLayerName;
            sr.sortingOrder  = sortingOrder;

            pool.Add(new Particle { t = go.transform, sr = sr, active = false });
        }
    }

    private void OnDestroy()
    {
        if (poolParent != null)
            Destroy(poolParent.gameObject);
    }

    // ── Update ────────────────────────────────────────────────────────
    private void Update()
    {
        if (playerController == null || rb == null) return;

        bool    grounded    = playerController.IsGrounded;
        bool    wallSliding = playerController.IsWallSliding;
        Vector2 vel         = playerController.Velocity;
        bool    dead        = playerController.IsDead;

        if (!dead)
        {
            // Landing
            if (grounded && !wasGroundedPrev)
                SpawnLandDust();

            // Jump
            if (!grounded && wasGroundedPrev && vel.y > 1f)
                SpawnJumpPuff();

            // Run dust
            if (grounded && Mathf.Abs(vel.x) >= runSpeedThreshold)
            {
                runEmitTimer -= Time.deltaTime;
                if (runEmitTimer <= 0f)
                {
                    runEmitTimer = 1f / runEmitRate;
                    SpawnRunDust(vel.x);
                }
            }
            else
            {
                runEmitTimer = 0f;
            }

            // Wall slide
            if (wallSliding)
            {
                wallEmitTimer -= Time.deltaTime;
                if (wallEmitTimer <= 0f)
                {
                    wallEmitTimer = 1f / wallEmitRate;
                    SpawnWallSlideSpark();
                }
            }
        }

        wasGroundedPrev      = grounded;
        prevVerticalVelocity = vel.y;

        TickParticles();
    }

    // ── Spawn helpers ─────────────────────────────────────────────────
    private void SpawnRunDust(float moveX)
    {
        float side   = -Mathf.Sign(moveX);
        Vector2 orig = (Vector2)transform.position + new Vector2(side * 0.2f + feetOffset.x, feetOffset.y);
        // Kick mostly upward; small backward drift; varied height so some stay low, some fly up
        float upSpeed = Random.Range(runUpSpeedMin, runUpSpeedMax);
        Vector2 v    = new Vector2(side * Random.Range(runSpeedMin, runSpeedMax) * 0.35f, upSpeed);
        v.x += Random.Range(-runSpread, runSpread);
        Emit(orig, v, runLifetime, PIXEL_SIZE, runColor, runGravity);
    }

    private void SpawnLandDust()
    {
        Vector2 origin = (Vector2)transform.position + feetOffset;
        for (int i = 0; i < landParticleCount; i++)
        {
            float side = (i % 2 == 0) ? 1f : -1f;
            float t    = (float)i / landParticleCount;
            Vector2 v  = new Vector2(
                side * Random.Range(landSpeedMin, landSpeedMax) * (0.5f + t * 0.5f),
                Random.Range(0.5f, 2f));
            v.x += Random.Range(-landSpreadX, landSpreadX);
            Emit(origin, v, landLifetime, PIXEL_SIZE, landColor, landGravity);
        }
    }

    private void SpawnJumpPuff()
    {
        Vector2 origin = (Vector2)transform.position + feetOffset;
        for (int i = 0; i < jumpParticleCount; i++)
        {
            float rad = Random.Range(0f, Mathf.PI * 2f);
            Vector2 v = new Vector2(
                Mathf.Cos(rad) * Random.Range(jumpSpeedMin, jumpSpeedMax),
                Mathf.Sin(rad) * Random.Range(jumpSpeedMin * 0.5f, jumpSpeedMax * 0.5f));
            Emit(origin, v, jumpLifetime, PIXEL_SIZE, jumpColor, jumpGravity);
        }
    }

    private void SpawnWallSlideSpark()
    {
        int     wd    = playerController.WallDirection;
        float   velY  = playerController.Velocity.y;
        // Sliding down → spawn near/above head; sliding up → spawn near feet
        float   baseY = velY < 0f ? wallSlideDownY : wallSlideUpY;
        Vector2 origin = (Vector2)transform.position + new Vector2(wd * wallSideOffset, baseY + Random.Range(-wallHeightRange, wallHeightRange));
        // Sparks fly away from wall; going down they fall, going up they drift down too
        Vector2 v      = new Vector2(-wd * Random.Range(wallSpeedMin, wallSpeedMax), -Random.Range(0.5f, 1.5f));
        Emit(origin, v, wallLifetime, PIXEL_SIZE, wallColor, wallGravity);
    }

    // ── Pool core ─────────────────────────────────────────────────────
    private void Emit(Vector2 pos, Vector2 vel, float lifetime, float size, Color color, float gravity)
    {
        Particle p = GetFree();
        if (p == null) return;

        p.position    = pos;
        p.velocity    = vel;
        p.lifetime    = lifetime;
        p.maxLifetime = lifetime;
        p.startSize   = size;
        p.gravity     = gravity;
        p.startColor  = color;
        p.active      = true;

        p.t.position       = pos;
        p.t.localScale     = Vector3.one * size;
        p.sr.color         = color;
        p.t.gameObject.SetActive(true);
    }

    private void TickParticles()
    {
        float dt = Time.deltaTime;
        foreach (var p in pool)
        {
            if (!p.active) continue;

            p.lifetime -= dt;
            if (p.lifetime <= 0f)
            {
                p.active = false;
                p.t.gameObject.SetActive(false);
                continue;
            }

            p.velocity.y -= p.gravity * dt;
            p.position   += p.velocity * dt;
            p.t.position  = p.position;

            float pct = p.lifetime / p.maxLifetime;
            p.sr.color       = p.startColor;
        p.t.localScale   = Vector3.one * p.startSize;
        }
    }

    private Particle GetFree()
    {
        foreach (var p in pool)
            if (!p.active) return p;
        return null;
    }

    // ── Fallback sprite (4×4 white square at 16 PPU) ──────────────────
    private static Sprite CreateFallbackSprite()
    {
        var tex = new Texture2D(4, 4, TextureFormat.ARGB32, false);
        tex.filterMode = FilterMode.Point;
        var pixels = new Color[16];
        for (int i = 0; i < 16; i++) pixels[i] = Color.white;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 16f);
    }
}


