using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    [Header("Run")]
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float groundAcceleration = 70f;
    [SerializeField] private float groundDeceleration = 30f;
    [SerializeField] private float airAcceleration = 55f;
    [SerializeField] private float airDeceleration = 10f;

    [Header("Momentum")]
    [SerializeField] private float overSpeedDeceleration = 20f;
    [SerializeField] private float maxOverSpeed = 25f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.12f;

    [Header("Gravity")]
    [SerializeField] private float baseGravityScale = 3f;
    [SerializeField] private float fallGravityScale = 6f;
    [SerializeField] private float jumpCutGravityScale = 12f;
    [SerializeField] private float maxFallSpeed = 20f;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 0.05f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Health")]
    [SerializeField] private bool isDead = false;

    public UnityEvent OnPlayerDeath = new UnityEvent();

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool wasGrounded;
    private float coyoteTimer;
    private float jumpBufferTimer;
    private bool jumpCut;
    private bool isJumping;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = baseGravityScale;
    }

    private void Update()
    {
        if (isDead) return;

        wasGrounded = isGrounded;
        UpdateGroundedState();
        HandleJumpInput();
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        ApplyMovement();
        ApplyGravityScale();
        TryJump();

        float clampedY = Mathf.Max(rb.linearVelocity.y, -maxFallSpeed);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, clampedY);
    }

    private void UpdateGroundedState()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);

        if (isGrounded)
        {
            coyoteTimer = coyoteTime;
            if (!wasGrounded)
            {
                isJumping = false;
                jumpCut = false;
            }
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }
    }

    private void HandleJumpInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferTimer = jumpBufferTime;
        }
        else
        {
            jumpBufferTimer -= Time.deltaTime;
        }

        if (Input.GetKeyUp(KeyCode.Space) && isJumping && rb.linearVelocity.y > 0)
        {
            jumpCut = true;
        }
    }

    private void ApplyMovement()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float inputSpeed = moveX * maxSpeed;
        float currentX = rb.linearVelocity.x;
        float absCurrentX = Mathf.Abs(currentX);
        bool hasInput = Mathf.Abs(moveX) > 0.01f;
        bool overSpeed = absCurrentX > maxSpeed;

        float newX;

        if (overSpeed)
        {
            // Player has momentum above max speed (e.g. from shotgun blast)
            // Gently bleed off the overspeed — don't snap to max
            float direction = Mathf.Sign(currentX);

            if (hasInput && Mathf.Sign(moveX) == direction)
            {
                // Holding direction with momentum — slow bleed
                newX = Mathf.MoveTowards(currentX, direction * maxSpeed, overSpeedDeceleration * Time.fixedDeltaTime);
            }
            else if (hasInput)
            {
                // Input opposite to momentum — decelerate faster but still carry speed
                newX = Mathf.MoveTowards(currentX, inputSpeed, (overSpeedDeceleration * 2f) * Time.fixedDeltaTime);
            }
            else
            {
                // No input — bleed off naturally
                newX = Mathf.MoveTowards(currentX, 0f, overSpeedDeceleration * Time.fixedDeltaTime);
            }
        }
        else
        {
            // Normal movement — standard accel/decel
            float accel;
            if (isGrounded)
                accel = hasInput ? groundAcceleration : groundDeceleration;
            else
                accel = hasInput ? airAcceleration : airDeceleration;

            newX = Mathf.MoveTowards(currentX, inputSpeed, accel * Time.fixedDeltaTime);
        }

        // Clamp to max overspeed ceiling to prevent infinite acceleration
        newX = Mathf.Clamp(newX, -maxOverSpeed, maxOverSpeed);

        rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
    }

    private void ApplyGravityScale()
    {
        if (jumpCut && rb.linearVelocity.y > 0)
        {
            rb.gravityScale = jumpCutGravityScale;
        }
        else if (rb.linearVelocity.y < 0)
        {
            rb.gravityScale = fallGravityScale;
        }
        else
        {
            rb.gravityScale = baseGravityScale;
        }
    }

    private void TryJump()
    {
        if (jumpBufferTimer > 0 && coyoteTimer > 0)
        {
            isJumping = true;
            jumpCut = false;
            jumpBufferTimer = 0;
            coyoteTimer = 0;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        if (other.GetComponent<Enemy>() != null)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        rb.isKinematic = true;
        OnPlayerDeath?.Invoke();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    }
}
