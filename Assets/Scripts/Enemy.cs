using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;
    private Animator animator;
    private Collider2D enemyCollider;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        enemyCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
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

