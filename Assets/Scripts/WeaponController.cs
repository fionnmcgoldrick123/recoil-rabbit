using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Gun Stats")]
    [SerializeField] private GunStats revolverStats;
    [SerializeField] private GunStats shotgunStats;

    [Header("References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private Rigidbody2D playerRb;
    [SerializeField] private GunView gunView;
    [SerializeField] private CameraShake cameraShake;

    [Header("Camera Shake")]
    [SerializeField] private float revolverShakeIntensity = 0.015f;
    [SerializeField] private float revolverShakeDuration = 0.05f;
    [SerializeField] private float shotgunShakeIntensity = 0.03f;
    [SerializeField] private float shotgunShakeDuration = 0.07f;

    [Header("UI (Optional)")]
    [SerializeField] private TMPro.TextMeshProUGUI shotgunAmmoText;

    private float revolverCooldown;
    private float shotgunCooldown;
    private int shotgunAmmo;

    private Camera mainCamera;
    private PlayerController playerController;

    private void Awake()
    {
        mainCamera = Camera.main;
        shotgunAmmo = shotgunStats != null ? shotgunStats.startingAmmo : 0;

        if (firePoint == null)
            Debug.LogError("FirePoint not assigned on WeaponController!");
        if (playerRb == null)
            playerRb = GetComponentInParent<Rigidbody2D>();
        if (gunView == null)
            gunView = GetComponentInChildren<GunView>();
        if (cameraShake == null)
            cameraShake = FindFirstObjectByType<CameraShake>();

        playerController = GetComponentInParent<PlayerController>();
    }

    private void Update()
    {
        if (playerController != null && playerController.IsDead)
            return;

        revolverCooldown -= Time.deltaTime;
        shotgunCooldown -= Time.deltaTime;

        if (Input.GetMouseButtonDown(0))
        {
            TryFireRevolver();
        }

        if (Input.GetMouseButtonDown(1))
        {
            TryFireShotgun();
        }

        UpdateAmmoUI();
    }

    private void TryFireRevolver()
    {
        if (revolverCooldown > 0 || revolverStats == null) return;
        if (firePoint == null) return;

        revolverCooldown = 1f / revolverStats.fireRate;
        Vector2 direction = GetMouseDirection();
        SpawnBullets(revolverStats, direction, isRevolver: true);

        if (cameraShake != null)
            cameraShake.Shake(revolverShakeIntensity, revolverShakeDuration);
    }

    private void TryFireShotgun()
    {
        if (shotgunCooldown > 0 || shotgunStats == null) return;
        if (firePoint == null) return;
        if (shotgunAmmo <= 0) return;

        shotgunCooldown = 1f / shotgunStats.fireRate;
        shotgunAmmo--;

        Vector2 direction = GetMouseDirection();
        SpawnBullets(shotgunStats, direction, isRevolver: false);

        // Propel player in the opposite direction of the shot.
        // We set the velocity component along the recoil axis directly so the
        // push is always the same magnitude regardless of current player velocity.
        if (playerRb != null && shotgunStats.playerRecoilForce > 0)
        {
            Vector2 recoilDir = -direction.normalized;
            Vector2 currentVel = playerRb.linearVelocity;

            // Project current velocity onto the recoil direction.
            float currentAlongRecoil = Vector2.Dot(currentVel, recoilDir);

            // Only override if the player isn't already moving faster than the recoil force in that direction.
            float desiredSpeed = shotgunStats.playerRecoilForce;
            float newSpeed = Mathf.Max(currentAlongRecoil, desiredSpeed);

            // Replace just the component along the recoil axis, preserve the perpendicular component.
            Vector2 perp = currentVel - currentAlongRecoil * recoilDir;
            playerRb.linearVelocity = perp + recoilDir * newSpeed;

            PlayerController pc = playerRb.GetComponent<PlayerController>();
            if (pc != null)
                pc.SetBhopProtected();
        }

        if (cameraShake != null)
            cameraShake.Shake(shotgunShakeIntensity, shotgunShakeDuration);
    }

    private void SpawnBullets(GunStats stats, Vector2 direction, bool isRevolver)
    {
        if (stats.bulletPrefab == null)
        {
            Debug.LogError($"Bullet prefab not assigned for {stats.name}!");
            return;
        }

        for (int i = 0; i < stats.pelletsPerShot; i++)
        {
            Vector2 bulletDirection = direction;

            // Always apply spread angle for shotgun pellets
            if (stats.spreadAngle > 0)
            {
                float angle = Random.Range(-stats.spreadAngle * 0.5f, stats.spreadAngle * 0.5f);
                bulletDirection = Quaternion.Euler(0, 0, angle) * direction;
            }

            GameObject bulletGo = Instantiate(stats.bulletPrefab, firePoint.position, Quaternion.identity);
            Bullet bullet = bulletGo.GetComponent<Bullet>();

            if (bullet == null)
            {
                Debug.LogError("Bullet prefab missing Bullet component!");
                Destroy(bulletGo);
                continue;
            }

            float rotation = Mathf.Atan2(bulletDirection.y, bulletDirection.x) * Mathf.Rad2Deg;
            bulletGo.transform.rotation = Quaternion.Euler(0, 0, rotation);

            bullet.Init(bulletDirection, stats.bulletSpeed, stats.bulletRange, stats.damage, isRevolver, this);
        }
    }

    private Vector2 GetMouseDirection()
    {
        Vector2 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = (mouseWorld - (Vector2)firePoint.position);
        return dir.sqrMagnitude > 0.001f ? dir.normalized : Vector2.right;
    }

    public void OnRevolverKill()
    {
        shotgunAmmo++;
    }

    private void UpdateAmmoUI()
    {
        if (shotgunAmmoText != null)
        {
            shotgunAmmoText.text = shotgunAmmo.ToString();
        }
    }

    public int ShotgunAmmo => shotgunAmmo;
}
