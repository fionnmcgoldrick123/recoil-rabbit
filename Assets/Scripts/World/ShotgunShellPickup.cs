using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class ShotgunShellPickup : MonoBehaviour
{
    [Header("Launch")]
    [SerializeField] private float launchSpeed = 7f;
    [SerializeField] private float launchGravityScale = 3f;

    [Header("Attraction")]
    [SerializeField] private float noAttractionDuration = 0.35f;
    [SerializeField] private float attractionSpeed = 12f;
    [SerializeField] private float attractionAcceleration = 30f;
    [SerializeField] private float pickupRadius = 0.4f;

    private Rigidbody2D rb;
    private Collider2D col;
    private Transform playerTransform;
    private WeaponController weaponController;
    private PlayerController playerController;

    private bool attracting = false;
    private bool collected = false;
    private float noAttractionTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        col.isTrigger = true;

        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            playerTransform = player.transform;
            playerController = player;
            weaponController = player.GetComponentInChildren<WeaponController>();
        }
    }

    public void Launch(Vector2 direction, float speedOverride = -1f)
    {
        noAttractionTimer = noAttractionDuration;
        rb.gravityScale = launchGravityScale;
        float speed = speedOverride > 0f ? speedOverride : launchSpeed;
        rb.linearVelocity = direction.normalized * speed;
    }

    private void Update()
    {
        if (collected) return;

        if (!attracting)
        {
            noAttractionTimer -= Time.deltaTime;
            if (noAttractionTimer <= 0f)
            {
                attracting = true;
                rb.gravityScale = 0f;
            }
            return;
        }

        if (playerTransform == null) return;

        if (Vector2.Distance(transform.position, playerTransform.position) <= pickupRadius)
        {
            Collect();
            return;
        }

        Vector2 toPlayer = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
        float currentSpeed = rb.linearVelocity.magnitude;
        float newSpeed = Mathf.MoveTowards(currentSpeed, attractionSpeed, attractionAcceleration * Time.deltaTime);
        rb.linearVelocity = toPlayer * newSpeed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        if (attracting && other.GetComponent<PlayerController>() != null)
            Collect();
    }

    private void Collect()
    {
        if (collected) return;
        collected = true;

        if (weaponController != null)
            weaponController.AddShotgunAmmo(1);

        if (playerController != null)
            playerController.TriggerPickupPop();

        Destroy(gameObject);
    }
}
