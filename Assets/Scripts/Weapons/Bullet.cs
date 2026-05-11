using UnityEngine;
using System.Collections.Generic;

public class Bullet : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;
    
    private int damage;
    private float range;
    private bool isRevolverBullet;
    private Vector2 origin;

    private WeaponController owner;
    private Animator animator;
    private Rigidbody2D rb;
    private bool hasHit = false;

    public void Init(Vector2 direction, float speed, float range, int damage, bool isRevolverBullet, WeaponController owner)
    {
        this.damage = damage;
        this.range = range;
        this.isRevolverBullet = isRevolverBullet;
        this.owner = owner;
        this.origin = transform.position;

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        rb.linearVelocity = direction.normalized * speed;

        // Check if firepoint was inside tileset - if so, destroy immediately
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            List<Collider2D> overlappingColliders = new List<Collider2D>();
            ContactFilter2D filter = new ContactFilter2D { layerMask = groundLayer, useLayerMask = true };
            Physics2D.OverlapCollider(collider, filter, overlappingColliders);
            if (overlappingColliders.Count > 0)
            {
                PlayHitAnimationAndDestroy();
            }
        }
    }

    private void Update()
    {
        if (Vector2.Distance(origin, transform.position) >= range)
        {
            PlayHitAnimationAndDestroy();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;

        // Check for ground/tileset collision first - destroy immediately
        if (((1 << other.gameObject.layer) & groundLayer) != 0)
        {
            hasHit = true;
            PlayHitAnimationAndDestroy();
            return;
        }

        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            hasHit = true;
            bool killed = enemy.TakeDamage(damage);

            if (killed)
            {
                owner?.OnRevolverKill();
            }

            PlayHitAnimationAndDestroy();
            return;
        }

        if (!other.CompareTag("Player") && other.GetComponent<Bullet>() == null)
        {
            hasHit = true;
            PlayHitAnimationAndDestroy();
        }
    }

    private void PlayHitAnimationAndDestroy()
    {
        rb.linearVelocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;
        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OnHitAnimationComplete()
    {
        Destroy(gameObject);
    }
}
