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

    [Header("UI (Optional)")]
    [SerializeField] private TMPro.TextMeshProUGUI shotgunAmmoText;

    private float revolverCooldown;
    private float shotgunCooldown;
    private int shotgunAmmo;

    private Camera mainCamera;

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
    }

    private void Update()
    {
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

        // Propel player in the opposite direction of the shot
        if (playerRb != null && shotgunStats.playerRecoilForce > 0)
        {
            Vector2 recoil = -direction.normalized * shotgunStats.playerRecoilForce;
            playerRb.AddForce(recoil, ForceMode2D.Impulse);
        }
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

            // Apply spread for shotgun
            if (stats.pelletsPerShot > 1)
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
        // Direction from the firePoint (barrel tip) to the mouse cursor in world space
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
            shotgunAmmoText.text = $"Shells: {shotgunAmmo}";
        }
    }

    public int ShotgunAmmo => shotgunAmmo;
}
