using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Run")]
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float groundAcceleration = 70f;
    [SerializeField] private float groundDeceleration = 30f;
    [SerializeField] private float airAcceleration = 55f;
    [SerializeField] private float airDeceleration = 10f;

    [Header("Momentum")]
    [SerializeField] private float overSpeedDeceleration = 8f;
    [SerializeField] private float overSpeedGroundedBleed = 2f;
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

    [Header("References")]
    [SerializeField] private GameObject gunObject;

    public UnityEvent OnPlayerDeath = new UnityEvent();

    private Rigidbody2D rb;
    private Animator animator;
    private bool isGrounded;
    private bool wasGrounded;

    public bool IsGrounded => isGrounded;
    public bool IsDead => isDead;
    public float HorizontalSpeed => rb != null ? Mathf.Abs(rb.linearVelocity.x) : 0f;
    public float MaxOverSpeed => maxOverSpeed;
    private float coyoteTimer;
    private float jumpBufferTimer;
    private bool jumpCut;
    private bool isJumping;
    private bool bhopProtected;

    public void SetBhopProtected() { bhopProtected = true; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        rb.gravityScale = baseGravityScale;

        if (gunObject == null)
        {
            WeaponController weaponController = GetComponentInChildren<WeaponController>();
            if (weaponController != null)
                gunObject = weaponController.gameObject;
        }
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
        if (bhopProtected)
        {
            bhopProtected = false;
            return;
        }

        float moveX = Input.GetAxisRaw("Horizontal");
        float inputSpeed = moveX * maxSpeed;
        float currentX = rb.linearVelocity.x;
        float absCurrentX = Mathf.Abs(currentX);
        bool hasInput = Mathf.Abs(moveX) > 0.01f;
        bool overSpeed = absCurrentX > maxSpeed;

        float newX;

        if (overSpeed)
        {
            float direction = Mathf.Sign(currentX);
            bool sameDir = hasInput && Mathf.Sign(moveX) == direction;
            bool opposite = hasInput && !sameDir;

            if (!isGrounded)
            {
                if (opposite)
                    newX = Mathf.MoveTowards(currentX, inputSpeed, overSpeedDeceleration * 2f * Time.fixedDeltaTime);
                else
                    newX = currentX;
            }
            else
            {
                if (sameDir)
                    newX = Mathf.MoveTowards(currentX, direction * maxSpeed, overSpeedGroundedBleed * Time.fixedDeltaTime);
                else if (opposite)
                    newX = Mathf.MoveTowards(currentX, inputSpeed, overSpeedDeceleration * 2f * Time.fixedDeltaTime);
                else
                    newX = Mathf.MoveTowards(currentX, 0f, overSpeedDeceleration * Time.fixedDeltaTime);
            }
        }
        else
        {
            float accel;
            if (isGrounded)
                accel = hasInput ? groundAcceleration : groundDeceleration;
            else
                accel = hasInput ? airAcceleration : airDeceleration;

            newX = Mathf.MoveTowards(currentX, inputSpeed, accel * Time.fixedDeltaTime);
        }

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
            if (Mathf.Abs(rb.linearVelocity.x) > maxSpeed)
                bhopProtected = true;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    public void LaunchFromSpring(float springForce)
    {
        if (isDead)
            return;

        isJumping = false;
        jumpCut = false;
        jumpBufferTimer = 0f;
        coyoteTimer = 0f;
        rb.gravityScale = baseGravityScale;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, springForce);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        if (other.GetComponent<Enemy>() != null || other.GetComponent<FlyingEnemy>() != null)
        {
            isGrounded = false;
            Die();
        }

        if (other.CompareTag("Hazard"))
        {
            isGrounded = false;
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        isJumping = false;
        jumpCut = false;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        if (gunObject != null)
            gunObject.SetActive(false);
        if (animator != null)
        {
            animator.SetBool("IsGrounded", true);
            animator.SetFloat("Speed", 0f);
            animator.ResetTrigger("Died");
            animator.SetTrigger("Died");
        }
        OnPlayerDeath?.Invoke();
    }

    public void OnDeathAnimationComplete()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    }
}
