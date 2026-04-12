using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private float shootInterval = 2f;
    [SerializeField] private float projectileSpawnOffset = 0.6f;

    [SerializeField] private int maxHealth = 3;
    private int currentHealth;
    private Animator animator;
    private Collider2D enemyCollider;
    private SpriteRenderer spriteRenderer;
    private System.Collections.IEnumerator shootRoutine;
    private Transform playerTarget;
    private bool isDead;

    private void Awake()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        enemyCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
            playerTarget = playerObject.transform;

        if (projectilePrefab != null && shootInterval > 0f)
            StartCoroutine(ShootLoop());
    }

    private System.Collections.IEnumerator ShootLoop()
    {
        while (!isDead)
        {
            yield return new WaitForSeconds(shootInterval);

            if (!isDead)
                Shoot();
        }
    }

    private void Shoot()
    {
        if (projectilePrefab == null)
            return;

        Vector2 direction = GetShootDirection();
        Vector3 spawnPosition = transform.position + (Vector3)(direction * projectileSpawnOffset);

        GameObject projectileGo = Object.Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
    }

    private Vector2 GetShootDirection()
    {
        if (playerTarget != null)
            return ((Vector2)playerTarget.position - (Vector2)transform.position).normalized;

        return GetFacingDirection();
    }

    private Vector2 GetFacingDirection()
    {
        bool facingLeft = (spriteRenderer != null && spriteRenderer.flipX) || transform.localScale.x < 0f;
        return facingLeft ? Vector2.left : Vector2.right;
    }

    public bool TakeDamage(int amount)
    {
        if (isDead)
            return false;

        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Die();
            return true;
        }

        return false;
    }

    private void Die()
    {
        isDead = true;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayEnemyDeath();

        if (shootRoutine != null)
            StopCoroutine(shootRoutine);

        if (enemyCollider != null)
            enemyCollider.enabled = false;

        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }
        else
        {
            if (spriteRenderer != null)
                spriteRenderer.enabled = false;
            Destroy(gameObject);
        }
    }

    public void OnDeathAnimationComplete()
    {
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;
        Destroy(gameObject);
    }
}

