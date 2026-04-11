using UnityEngine;

public class FlyingEnemy : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private bool moveHorizontal = true;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float maxDistance = 5f;

    [Header("Detection")]
    [SerializeField] private int maxHealth = 3;

    private Vector3 startPosition;
    private int currentHealth;
    private Animator animator;
    private Collider2D enemyCollider;
    private SpriteRenderer spriteRenderer;
    private Vector3 moveDirection = Vector3.right;

    private void Awake()
    {
        startPosition = transform.position;
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        enemyCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        Vector3 targetPos = transform.position + moveDirection * moveSpeed * Time.deltaTime;

        if (moveHorizontal)
        {
            float xDiff = targetPos.x - startPosition.x;
            targetPos.x = startPosition.x + Mathf.Clamp(xDiff, -maxDistance, maxDistance);

            if (Mathf.Abs(moveDirection.x) > 0.01f)
            {
                if (spriteRenderer != null)
                    spriteRenderer.flipX = moveDirection.x < 0;
            }

            if (Mathf.Abs(xDiff) >= maxDistance)
                moveDirection.x *= -1f;
        }
        else
        {
            float yDiff = targetPos.y - startPosition.y;
            targetPos.y = startPosition.y + Mathf.Clamp(yDiff, -maxDistance, maxDistance);

            if (Mathf.Abs(yDiff) >= maxDistance)
                moveDirection.y *= -1f;
        }

        transform.position = targetPos;
    }

    public bool TakeDamage(int amount)
    {
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
        // Disable collision immediately
        if (enemyCollider != null)
            enemyCollider.enabled = false;

        // Play death animation if animator exists
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        
        if (moveHorizontal)
        {
            Gizmos.DrawLine(transform.position - Vector3.right * maxDistance, transform.position + Vector3.right * maxDistance);
        }
        else
        {
            Gizmos.DrawLine(transform.position - Vector3.up * maxDistance, transform.position + Vector3.up * maxDistance);
        }
    }
}
