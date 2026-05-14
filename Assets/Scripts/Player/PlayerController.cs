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

    [Header("Wall Jump")]
    [SerializeField] private float wallJumpUpForce = 14f;
    [SerializeField] private float wallJumpSideForce = 10f;
    [SerializeField] private float wallCheckDistance = 0.3f;
    [SerializeField] private float wallJumpBufferTime = 0.1f;
    [SerializeField] private float wallJumpLockTime = 0.15f;
    [SerializeField] private LayerMask wallLayer;

    [Header("Wall Slide")]
    [SerializeField] private float wallSlideFriction = 0.5f;
    [SerializeField] private float wallSlideMaxFallSpeed = 3f;
    [SerializeField] private float wallMomentumMultiplier = 0.8f;
    [SerializeField] private float wallMomentumDecay = 3f;

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
    [SerializeField] private float deathImmunityDuration = 0.1f;

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
    private bool jumpCutSuppressedUntilGrounded;
    private bool isJumping;
    private bool bhopProtected;
    private int wallDirection;
    private int bufferedWallDirection;
    private float wallJumpBufferTimer;
    private float wallJumpLockTimer;
    private bool isWallSliding;
    private bool wasWallSlidingLastFrame;
    private float wallSlideMomentum;
    private float deathImmunityTimer;
    private float speedMultiplier = 1f;

    public void SetBhopProtected() { bhopProtected = true; }
    public void ClearJumpCut()
    {
        jumpCut = false;
        jumpCutSuppressedUntilGrounded = true;
    }

    public void ActivateDeathImmunity()
    {
        deathImmunityTimer = deathImmunityDuration;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.gravityScale = baseGravityScale;
        RunTimerManager.ResetTimer();

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

        if (deathImmunityTimer > 0f)
            deathImmunityTimer -= Time.deltaTime;

        wasGrounded = isGrounded;
        wasWallSlidingLastFrame = isWallSliding;
        UpdateGroundedState();
        UpdateWallState();
        HandleJumpInput();
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        if (wallJumpLockTimer > 0f)
            wallJumpLockTimer -= Time.fixedDeltaTime;

        ApplyMovement();
        ApplyGravityScale();
        ApplyWallSlide();
        TryJump();
        TryWallJump();

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
                jumpCutSuppressedUntilGrounded = false;
            }
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }
    }

    private void UpdateWallState()
    {
        if (isGrounded)
        {
            wallDirection = 0;
            bufferedWallDirection = 0;
            wallJumpBufferTimer = 0f;
            isWallSliding = false;
            wallSlideMomentum = 0f;
            return;
        }

        bool wallRight = Physics2D.Raycast(transform.position, Vector2.right, wallCheckDistance, wallLayer);
        bool wallLeft  = Physics2D.Raycast(transform.position, Vector2.left,  wallCheckDistance, wallLayer);

        if (wallRight)
            wallDirection = 1;
        else if (wallLeft)
            wallDirection = -1;
        else
            wallDirection = 0;

        if (wallDirection != 0)
        {
            bufferedWallDirection = wallDirection;
            wallJumpBufferTimer = wallJumpBufferTime;
        }
        else
        {
            wallJumpBufferTimer -= Time.deltaTime;
            isWallSliding = false;
            wallSlideMomentum = 0f;
        }

        // Check if player is pressing towards the wall
        float moveX = Input.GetAxisRaw("Horizontal");
        if (wallDirection != 0 && moveX != 0 && Mathf.Sign(moveX) == wallDirection)
        {
            isWallSliding = true;
            
            // Capture momentum on first wall contact
            if (!wasWallSlidingLastFrame && rb.linearVelocity.y > 0)
            {
                wallSlideMomentum = rb.linearVelocity.y * wallMomentumMultiplier;
            }
        }
        else
        {
            isWallSliding = false;
            wallSlideMomentum = 0f;
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

        if (Input.GetKeyUp(KeyCode.Space) && isJumping && rb.linearVelocity.y > 0 && !jumpCutSuppressedUntilGrounded)
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

        if (wallJumpLockTimer > 0f)
            return;

        float moveX = Input.GetAxisRaw("Horizontal");
        float inputSpeed = moveX * maxSpeed * speedMultiplier;
        float currentX = rb.linearVelocity.x;
        float absCurrentX = Mathf.Abs(currentX);
        bool hasInput = Mathf.Abs(moveX) > 0.01f;
        bool overSpeed = absCurrentX > maxSpeed * speedMultiplier;

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
                    newX = Mathf.MoveTowards(currentX, direction * maxSpeed * speedMultiplier, overSpeedGroundedBleed * Time.fixedDeltaTime);
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

    private void ApplyWallSlide()
    {
        // Don't apply wall slide friction during wall jump lock period
        if (wallJumpLockTimer > 0f)
        {
            wallSlideMomentum = 0f;
            return;
        }

        if (!isWallSliding || isGrounded)
        {
            wallSlideMomentum = 0f;
            return;
        }

        // Decay wall momentum over time
        wallSlideMomentum = Mathf.Max(0f, wallSlideMomentum - wallMomentumDecay * Time.fixedDeltaTime);

        // If we still have momentum, apply upward force
        if (wallSlideMomentum > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, wallSlideMomentum);
        }
        else
        {
            // When momentum is depleted, apply friction for downward slide
            if (rb.linearVelocity.y < 0)
            {
                float newY = Mathf.Lerp(rb.linearVelocity.y, 0f, wallSlideFriction * Time.fixedDeltaTime);
                newY = Mathf.Max(newY, -wallSlideMaxFallSpeed);
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, newY);
            }
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

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayJump();
        }
    }

    private void TryWallJump()
    {
        int jumpWallDirection = wallDirection != 0 ? wallDirection : (wallJumpBufferTimer > 0f ? bufferedWallDirection : 0);

        if (jumpBufferTimer <= 0 || jumpWallDirection == 0 || isGrounded)
            return;

        isJumping = true;
        jumpCut = false;
        jumpBufferTimer = 0;
        wallJumpBufferTimer = 0f;
        wallJumpLockTimer = wallJumpLockTime;
        isWallSliding = false;
        wallSlideMomentum = 0f;
        bhopProtected = true;

        float sideVelocity = -jumpWallDirection * wallJumpSideForce;
        rb.linearVelocity = new Vector2(sideVelocity, wallJumpUpForce);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayJump();
    }

    public void LaunchFromSpring(float springForce)
    {
        if (isDead)
            return;

        isJumping = false;
        jumpCut = false;
        jumpCutSuppressedUntilGrounded = false;
        jumpBufferTimer = 0f;
        coyoteTimer = 0f;
        rb.gravityScale = baseGravityScale;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, springForce);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        // Don't die if we have death immunity (just killed an enemy)
        if (deathImmunityTimer > 0f) return;

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
        jumpCutSuppressedUntilGrounded = false;
        RunTimerManager.PauseTimer();
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        if (gunObject != null)
        {
            WeaponController weaponController = gunObject.GetComponent<WeaponController>();
            if (weaponController != null)
                weaponController.HideShotgunHud();
        }
        if (gunObject != null)
            gunObject.SetActive(false);
        if (animator != null)
        {
            animator.SetBool("IsGrounded", true);
            animator.SetFloat("Speed", 0f);
            animator.ResetTrigger("Died");
            animator.SetTrigger("Died");
        }

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayDeath();

        OnPlayerDeath?.Invoke();
    }

    public void OnDeathAnimationComplete()
    {
        SceneTransition.ReloadActiveScene();
    }

    public void ApplySpeedBoost(float multiplier, float duration)
    {
        StartCoroutine(SpeedBoostCoroutine(multiplier, duration));
    }

    private System.Collections.IEnumerator SpeedBoostCoroutine(float multiplier, float duration)
    {
        speedMultiplier = multiplier;
        yield return new WaitForSeconds(duration);
        speedMultiplier = 1f;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * wallCheckDistance);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.left  * wallCheckDistance);
    }
}
