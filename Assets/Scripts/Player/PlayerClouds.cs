using UnityEngine;

public class PlayerClouds : MonoBehaviour
{
    [Header("Land")]
    [SerializeField] private GameObject landPrefab;
    [SerializeField] private Vector2 landFloorOffset = new Vector2(0f, -0.4f);

    [Header("Jump")]
    [SerializeField] private GameObject jumpPrefab;
    [SerializeField] private Vector2 jumpFloorOffset = new Vector2(0f, -0.4f);

    [Header("Wall Jump")]
    [SerializeField] private GameObject wallJumpPrefab;
    // x = distance from player toward the wall, y = vertical offset
    [SerializeField] private Vector2 wallJumpWallOffset = new Vector2(0.3f, 0f);

    [Header("Wall Slide")]
    [SerializeField] private GameObject wallSlidePrefab;
    // x = distance from player toward the wall; y magnitude is used and sign flips with vertical velocity
    [SerializeField] private Vector2 wallSlideWallOffset = new Vector2(0.3f, 0.4f);
    [SerializeField] private float wallSlideSpawnInterval = 0.08f;

    [Header("Super Bhop")]
    [SerializeField] private GameObject superBhopPrefab;
    [SerializeField] private Vector2 superBhopFloorOffset = new Vector2(0f, -0.4f);

    [Header("Hyper Dash (Floor)")]
    [SerializeField] private GameObject hyperDashPrefab;
    [SerializeField] private Vector2 hyperDashFloorOffset = new Vector2(0f, -0.4f);

    [Header("Wall Hyper")]
    [SerializeField] private GameObject wallHyperPrefab;
    // x = distance from player toward the wall, y = vertical offset
    [SerializeField] private Vector2 wallHyperWallOffset = new Vector2(0.3f, 0f);

    private PlayerController player;
    private float wallSlideTimer;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        player.OnLand      += HandleLand;
        player.OnJump      += HandleJump;
        player.OnWallJump  += HandleWallJump;
        player.OnSuperBhop += HandleSuperBhop;
        player.OnHyperDash += HandleHyperDash;
        player.OnWallHyper += HandleWallHyper;
    }

    private void OnDisable()
    {
        player.OnLand      -= HandleLand;
        player.OnJump      -= HandleJump;
        player.OnWallJump  -= HandleWallJump;
        player.OnSuperBhop -= HandleSuperBhop;
        player.OnHyperDash -= HandleHyperDash;
        player.OnWallHyper -= HandleWallHyper;
    }

    private void Update()
    {
        TickWallSlide();
    }

    // ── Event handlers ──────────────────────────────────────────────────────

    private void HandleLand()            => SpawnAtFloor(landPrefab,      landFloorOffset);
    private void HandleJump()            => SpawnAtFloor(jumpPrefab,      jumpFloorOffset);
    private void HandleSuperBhop()       => SpawnAtFloor(superBhopPrefab, superBhopFloorOffset);
    private void HandleHyperDash(int _)  => SpawnAtFloor(hyperDashPrefab, hyperDashFloorOffset);
    private void HandleWallJump(int dir) => SpawnAtWall(wallJumpPrefab,   wallJumpWallOffset, dir);
    private void HandleWallHyper(int dir)=> SpawnAtWall(wallHyperPrefab,  wallHyperWallOffset, dir);

    // ── Wall slide (polled) ──────────────────────────────────────────────────

    private void TickWallSlide()
    {
        if (!player.IsWallSliding)
        {
            wallSlideTimer = 0f;
            return;
        }

        wallSlideTimer -= Time.deltaTime;
        if (wallSlideTimer <= 0f)
        {
            wallSlideTimer = wallSlideSpawnInterval;
            SpawnWallSlideParticle();
        }
    }

    private void SpawnWallSlideParticle()
    {
        if (wallSlidePrefab == null) return;

        int   wallDir = player.WallDirection;
        float velY    = player.Velocity.y;

        // x offset toward the wall; y offset opposes movement direction (sparks trail behind)
        float xOff = wallSlideWallOffset.x * wallDir;
        float yOff = wallSlideWallOffset.y * (velY != 0f ? -Mathf.Sign(velY) : 1f);

        SpawnAtWallRaw(wallSlidePrefab, new Vector2(xOff, yOff), wallDir);
    }

    // ── Spawn helpers ────────────────────────────────────────────────────────

    private void SpawnAtFloor(GameObject prefab, Vector2 offset)
    {
        if (prefab == null) return;
        Vector3 pos = transform.position + new Vector3(offset.x, offset.y, 0f);
        GameObject go = Instantiate(prefab, pos, Quaternion.identity);
        ParticleSystem ps = go.GetComponent<ParticleSystem>();
        if (ps != null) ps.Play();
    }

    /// <summary>
    /// Spawns a wall-relative effect. offset.x is the base distance toward the wall;
    /// it is automatically mirrored for left/right walls. The spawned object is also
    /// flipped on the X axis when on the left wall so the effect faces correctly.
    /// </summary>
    private void SpawnAtWall(GameObject prefab, Vector2 offset, int wallDir)
    {
        if (prefab == null) return;
        float xOff = offset.x * wallDir;
        SpawnAtWallRaw(prefab, new Vector2(xOff, offset.y), wallDir);
    }

    private void SpawnAtWallRaw(GameObject prefab, Vector2 finalOffset, int wallDir)
    {
        Vector3    pos = transform.position + new Vector3(finalOffset.x, finalOffset.y, 0f);
        GameObject go  = Instantiate(prefab, pos, Quaternion.identity);

        // Mirror the effect horizontally when the wall is on the left side
        if (wallDir < 0)
        {
            Vector3 s = go.transform.localScale;
            go.transform.localScale = new Vector3(-s.x, s.y, s.z);
        }

        ParticleSystem ps = go.GetComponent<ParticleSystem>();
        if (ps != null) ps.Play();
    }
}

