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

    [Header("Hyper")]
    [SerializeField] private float hyperSpeed = 30f;
    [SerializeField] private float hyperJumpForce = 10f;
    [SerializeField] private float hyperWindowTime = 0.25f;
    [SerializeField] private float hyperComboMultiplier = 1.3f;
    [SerializeField] private float hyperHorizontalMultiplier = 1.1f;
    [SerializeField] private float hyperComboLandGraceTime = 0.08f;
    [SerializeField] private int maxHyperComboLevel = 3;

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
    [SerializeField] private float wallContactHyperWindow = 0.15f;
    [SerializeField] private LayerMask wallLayer;

    [Header("Wall Hyper")]
    [SerializeField] private float wallHyperUpSpeed = 25f;
    [SerializeField] private float wallHyperDownSpeed = 25f;
    [SerializeField] private float wallHyperSideSpeed = 10f;
    [SerializeField] private float wallHyperWindowTime = 0.25f;
    [SerializeField] private float wallHyperVertComboMult = 1.3f;
    [SerializeField] private float wallHyperHorizComboMult = 1.1f;

    [Header("Super Bhop")]
    [SerializeField] private float superBhopUpwardForce = 30f;
    [SerializeField] private float superBhopHorizontalForce = 3f;
    [SerializeField] private float superBhopWindowTime = 0.25f;
    [SerializeField] private float superBhopHorizontalThreshold = 0.3f;
    [SerializeField] private float superBhopUpwardThreshold = 0.7f;

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
    [SerializeField] private GameObject headObject;
    [Header("Debug Gizmos")]
    [SerializeField] private bool showHyperAngleGizmos = true;
    [SerializeField] private float gizmoHyperAngleThreshold = 0.3f;
    [SerializeField] private bool showSuperBhopGizmo = true;
    [SerializeField] private float gizmoSuperBhopHorizontalThreshold = 0.3f;
    [SerializeField] private float gizmoSuperBhopUpwardThreshold = 0.7f;
    [SerializeField] private float gizmoLineLength = 4f;

    public UnityEvent OnPlayerDeath = new UnityEvent();

    // Particle events — consumed by PlayerClouds
    public event System.Action OnLand;
    public event System.Action OnJump;
    public event System.Action<int> OnWallJump;   // wallDir: which wall was pushed off
    public event System.Action OnSuperBhop;
    public event System.Action<int> OnHyperDash;  // dir: hyper direction (1 or -1)
    public event System.Action<int> OnWallHyper;  // wallDir: which wall was pushed off

    private Rigidbody2D rb;
    private Animator animator;
    private bool isGrounded;
    private bool wasGrounded;

    public bool IsGrounded => isGrounded;
    public bool WasGrounded => wasGrounded;
    public bool IsWallSliding => isWallSliding;
    public bool IsDead => isDead;
    public float HorizontalSpeed => rb != null ? Mathf.Abs(rb.linearVelocity.x) : 0f;
    public Vector2 Velocity => rb != null ? rb.linearVelocity : Vector2.zero;
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
    private float wallContactWindowTimer;
    private bool wallHyperDisabledThisContact;
    private float deathImmunityTimer;
    private float speedMultiplier = 1f;
    private float gravityMultiplier = 1f;
    private float hyperWindowTimer;
    private float hyperDirection;
    private int hyperComboCount = 0;
    private float hyperComboLandTimer = 0f;
    private float wallHyperShotWindowTimer;
    private float wallHyperVerticalDir;
    private float superBhopShotWindowTimer;
    private float superBhopHorizontalDir;
    private bool skipFallClampThisFrame;

    public int WallDirection => wallDirection;
    public bool CanPerformSuperBhop => superBhopShotWindowTimer > 0f;
    public bool CanPerformWallHyper => wallHyperShotWindowTimer > 0f;

    public void SetBhopProtected() { bhopProtected = true; }

    public void ResetHyperCombo()
    {
        hyperComboCount = 0;
    }

    public void ClearJumpCut()
    {
        jumpCut = false;
        jumpCutSuppressedUntilGrounded = true;
    }

    public void TriggerHyperWindow(float horizontalDir)
    {
        hyperWindowTimer = hyperWindowTime;
        hyperDirection = Mathf.Sign(horizontalDir);
    }

    public void TriggerWallHyperShot(float verticalDir)
    {
        wallHyperShotWindowTimer = wallHyperWindowTime;
        wallHyperVerticalDir = Mathf.Sign(verticalDir);
    }

    public void TriggerSuperBhopShot(float horizontalDir)
    {
        superBhopShotWindowTimer = superBhopWindowTime;
        superBhopHorizontalDir = Mathf.Sign(horizontalDir);
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

        
        if (headObject != null)
            headObject.SetActive(true);
    }

    private void Update()
    {
        if (isDead || isInCannon) return;

        if (deathImmunityTimer > 0f)
            deathImmunityTimer -= Time.deltaTime;

        if (hyperWindowTimer > 0f)
            hyperWindowTimer -= Time.deltaTime;
        if (wallHyperShotWindowTimer > 0f)
            wallHyperShotWindowTimer -= Time.deltaTime;
        if (superBhopShotWindowTimer > 0f)
            superBhopShotWindowTimer -= Time.deltaTime;

        wasGrounded = isGrounded;
        wasWallSlidingLastFrame = isWallSliding;
        UpdateGroundedState();
        UpdateWallState();
        HandleJumpInput();
    }

    private void FixedUpdate()
    {
        if (isDead || isInCannon) return;

        if (wallJumpLockTimer > 0f)
            wallJumpLockTimer -= Time.fixedDeltaTime;

        ApplyMovement();
        ApplyGravityScale();
        ApplyWallSlide();
        TryJump();
        TryWallJump();

        if (!skipFallClampThisFrame)
        {
            float clampedY = gravityMultiplier > 0
                ? Mathf.Max(rb.linearVelocity.y, -maxFallSpeed)
                : Mathf.Min(rb.linearVelocity.y, maxFallSpeed);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, clampedY);
        }
        skipFallClampThisFrame = false;
    }

    private void UpdateGroundedState()
    {
        Vector2 groundCheckDirection = gravityMultiplier > 0 ? Vector2.down : Vector2.up;
        isGrounded = Physics2D.Raycast(transform.position, groundCheckDirection, groundCheckDistance, groundLayer);

        if (isGrounded)
        {
            coyoteTimer = coyoteTime;
            if (!wasGrounded)
            {
                isJumping = false;
                jumpCut = false;
                jumpCutSuppressedUntilGrounded = false;
                hyperComboLandTimer = hyperComboLandGraceTime;
                wallHyperShotWindowTimer = 0f;
                OnLand?.Invoke();
            }
            else
            {
                hyperComboLandTimer -= Time.deltaTime;
                if (hyperComboLandTimer <= 0f)
                    hyperComboCount = 0;
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

            
            wallContactWindowTimer += Time.deltaTime;
            if (wallContactWindowTimer > wallContactHyperWindow)
            {
                
                wallHyperDisabledThisContact = true;
                hyperComboCount = 0;
            }
        }
        else
        {
            wallJumpBufferTimer -= Time.deltaTime;
            wallContactWindowTimer = 0f;
            wallHyperDisabledThisContact = false;
            isWallSliding = false;
            wallSlideMomentum = 0f;
        }

        
        float moveX = Input.GetAxisRaw("Horizontal");
        if (wallDirection != 0 && moveX != 0 && Mathf.Sign(moveX) == wallDirection)
        {
            isWallSliding = true;
            
            
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
        float velocityCheck = rb.linearVelocity.y * gravityMultiplier;
        
        if (jumpCut && velocityCheck > 0)
        {
            rb.gravityScale = jumpCutGravityScale * gravityMultiplier;
        }
        else if (velocityCheck < 0)
        {
            rb.gravityScale = fallGravityScale * gravityMultiplier;
        }
        else
        {
            rb.gravityScale = baseGravityScale * gravityMultiplier;
        }
    }

    private void ApplyWallSlide()
    {
        
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

        
        wallSlideMomentum = Mathf.Max(0f, wallSlideMomentum - wallMomentumDecay * Time.fixedDeltaTime);

        
        if (wallSlideMomentum > 0f)
        {
            float momentumDir = gravityMultiplier > 0 ? wallSlideMomentum : -wallSlideMomentum;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, momentumDir);
        }
        else
        {
            
            float velocityCheck = rb.linearVelocity.y * gravityMultiplier;
            if (velocityCheck < 0)
            {
                float newY = Mathf.Lerp(rb.linearVelocity.y, 0f, wallSlideFriction * Time.fixedDeltaTime);
                newY = gravityMultiplier > 0 
                    ? Mathf.Max(newY, -wallSlideMaxFallSpeed)
                    : Mathf.Min(newY, wallSlideMaxFallSpeed);
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, newY);
            }
        }
    }

    private void TryJump()
    {
        if (jumpBufferTimer <= 0 || coyoteTimer <= 0)
            return;

        
        int nearWallDir = wallDirection != 0 ? wallDirection : (wallJumpBufferTimer > 0f ? bufferedWallDirection : 0);
        if (wallHyperShotWindowTimer > 0f && nearWallDir != 0)
            return;

        isJumping = true;
        jumpCut = false;
        jumpBufferTimer = 0;
        coyoteTimer = 0;
        bhopProtected = true;

        if (superBhopShotWindowTimer > 0f)
        {
            
            superBhopShotWindowTimer = 0f;
            float hVel = Mathf.Clamp(rb.linearVelocity.x, -superBhopHorizontalForce, superBhopHorizontalForce);
            rb.linearVelocity = new Vector2(hVel, superBhopUpwardForce * gravityMultiplier);
            jumpCutSuppressedUntilGrounded = true; 
            hyperComboCount = Mathf.Min(hyperComboCount + 1, maxHyperComboLevel - 1);
            OnSuperBhop?.Invoke();
        }
        else if (hyperWindowTimer > 0f)
        {
            
            hyperWindowTimer = 0f;
            int clampedCombo = Mathf.Min(hyperComboCount, maxHyperComboLevel - 1);
            float comboMult = Mathf.Pow(hyperComboMultiplier, clampedCombo);
            float hComboMult = Mathf.Pow(hyperHorizontalMultiplier, clampedCombo);
            rb.linearVelocity = new Vector2(hyperDirection * hyperSpeed * hComboMult, hyperJumpForce * comboMult * gravityMultiplier);
            hyperComboCount = Mathf.Min(hyperComboCount + 1, maxHyperComboLevel - 1);
            OnHyperDash?.Invoke((int)hyperDirection);
        }
        else
        {
            if (Mathf.Abs(rb.linearVelocity.x) > maxSpeed)
                bhopProtected = true;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * gravityMultiplier);
            if (hyperComboLandTimer <= 0f)
                hyperComboCount = 0;
            OnJump?.Invoke();
        }

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayJump();
    }

    private void TryWallJump()
    {
        int jumpWallDirection = wallDirection != 0 ? wallDirection : (wallJumpBufferTimer > 0f ? bufferedWallDirection : 0);

        if (jumpBufferTimer <= 0 || jumpWallDirection == 0 || isGrounded)
            return;

        
        if (wallHyperShotWindowTimer > 0f && !wallHyperDisabledThisContact)
        {
            wallHyperShotWindowTimer = 0f;
            isJumping = true;
            jumpCut = false;
            jumpBufferTimer = 0;
            wallJumpBufferTimer = 0f;
            wallJumpLockTimer = wallJumpLockTime;
            isWallSliding = false;
            wallSlideMomentum = 0f;
            bhopProtected = true;

            int clampedCombo = Mathf.Min(hyperComboCount, maxHyperComboLevel - 1);
            float comboMult = Mathf.Pow(wallHyperVertComboMult, clampedCombo);
            float hComboMult = Mathf.Pow(wallHyperHorizComboMult, clampedCombo);
            float vertMagnitude = wallHyperVerticalDir > 0 ? wallHyperUpSpeed : wallHyperDownSpeed;
            float hSpeed = -jumpWallDirection * wallHyperSideSpeed * hComboMult;
            float vSpeed = wallHyperVerticalDir * vertMagnitude * comboMult * gravityMultiplier;
            rb.linearVelocity = new Vector2(hSpeed, vSpeed);
            skipFallClampThisFrame = true; 
            hyperComboCount = Mathf.Min(hyperComboCount + 1, maxHyperComboLevel - 1);
            OnWallHyper?.Invoke(jumpWallDirection);

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayJump();
            return;
        }

        isJumping = true;
        jumpCut = false;
        jumpBufferTimer = 0;
        wallJumpBufferTimer = 0f;
        wallJumpLockTimer = wallJumpLockTime;
        isWallSliding = false;
        wallSlideMomentum = 0f;
        bhopProtected = true;

        float sideVelocity = -jumpWallDirection * wallJumpSideForce;
        rb.linearVelocity = new Vector2(sideVelocity, wallJumpUpForce * gravityMultiplier);
        OnWallJump?.Invoke(jumpWallDirection);

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

    private bool isInCannon = false;

    public bool IsInCannon => isInCannon;

    public void EnterCannon()
    {
        if (isDead) return;

        isInCannon = true;

        
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        
        foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>())
        {
            
            if (gunObject != null && sr.transform.IsChildOf(gunObject.transform))
                continue;
            sr.enabled = false;
        }

        
        if (gunObject != null)
            gunObject.SetActive(false);

        // Zoom out camera
        CameraZoom cameraZoom = FindFirstObjectByType<CameraZoom>();
        if (cameraZoom != null)
            cameraZoom.ZoomOutForCannon();
    }

    public void LaunchFromCannon(Vector2 direction, float power)
    {
        LaunchFromCannon(direction, power, transform.position);
    }

    public void LaunchFromCannon(Vector2 direction, float power, Vector3 launchPosition)
    {
        if (!isInCannon) return;

        transform.position = launchPosition;
        isInCannon = false;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = baseGravityScale * gravityMultiplier;

        foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>())
        {
            if (gunObject != null && sr.transform.IsChildOf(gunObject.transform))
                continue;
            sr.enabled = true;
        }

        if (gunObject != null)
            gunObject.SetActive(true);

        isJumping = false;
        jumpCut = false;
        jumpCutSuppressedUntilGrounded = false;
        jumpBufferTimer = 0f;
        coyoteTimer = 0f;
        skipFallClampThisFrame = true;

        rb.linearVelocity = direction.normalized * power;

        // Zoom in camera
        CameraZoom cameraZoom = FindFirstObjectByType<CameraZoom>();
        if (cameraZoom != null)
            cameraZoom.ZoomInFromCannon();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        
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
        if (headObject != null)
            headObject.SetActive(false);
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

    public void StartGravityFlip(float duration)
    {
        StartCoroutine(GravityFlipCoroutine(duration));
    }

    private System.Collections.IEnumerator GravityFlipCoroutine(float duration)
    {
        gravityMultiplier = -1f;
        
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 5f);

        
        SpriteRenderer playerSprite = GetComponentInChildren<SpriteRenderer>();
        if (playerSprite != null)
            playerSprite.flipY = true;

        
        if (headObject != null)
        {
            SpriteRenderer headSprite = headObject.GetComponentInChildren<SpriteRenderer>();
            if (headSprite != null)
                headSprite.flipY = true;
        }

        
        if (gunObject != null)
        {
            SpriteRenderer gunSprite = gunObject.GetComponentInChildren<SpriteRenderer>();
            if (gunSprite != null)
                gunSprite.flipY = true;

            
            Vector3 gunLocalPos = gunObject.transform.localPosition;
            gunObject.transform.localPosition = new Vector3(gunLocalPos.x, -gunLocalPos.y, gunLocalPos.z);
        }

        yield return new WaitForSeconds(duration);

        gravityMultiplier = 1f;

        
        if (playerSprite != null)
            playerSprite.flipY = false;

        
        if (headObject != null)
        {
            SpriteRenderer headSprite = headObject.GetComponentInChildren<SpriteRenderer>();
            if (headSprite != null)
                headSprite.flipY = false;
        }

        
        if (gunObject != null)
        {
            SpriteRenderer gunSprite = gunObject.GetComponentInChildren<SpriteRenderer>();
            if (gunSprite != null)
                gunSprite.flipY = false;

            
            Vector3 gunLocalPos = gunObject.transform.localPosition;
            gunObject.transform.localPosition = new Vector3(gunLocalPos.x, -gunLocalPos.y, gunLocalPos.z);
        }
    }

    [Header("Shell Pickup Pop")]
    [SerializeField] private float pickupPopScale = 1.2f;
    [SerializeField] private float pickupPopDuration = 0.12f;

    private Coroutine pickupPopRoutine;

    public void TriggerPickupPop()
    {
        if (pickupPopRoutine != null)
            StopCoroutine(pickupPopRoutine);
        pickupPopRoutine = StartCoroutine(PickupPopCoroutine());
    }

    private System.Collections.IEnumerator PickupPopCoroutine()
    {
        Vector3 baseScale = Vector3.one;
        Vector3 popScale = Vector3.one * pickupPopScale;
        float half = pickupPopDuration * 0.5f;
        float elapsed = 0f;

        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(baseScale, popScale, elapsed / half);
            yield return null;
        }

        transform.localScale = popScale;
        elapsed = 0f;

        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(popScale, baseScale, elapsed / half);
            yield return null;
        }

        transform.localScale = baseScale;
        pickupPopRoutine = null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * wallCheckDistance);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.left  * wallCheckDistance);

        if (showHyperAngleGizmos)
        {
            float thr = gizmoHyperAngleThreshold;
            if (thr > 0f && thr < 1f)
            {
                
                
                
                float lo = Mathf.Asin(thr); 
                float hi = Mathf.Acos(thr); 

                float[] starts = { lo,                  Mathf.PI - hi, Mathf.PI + lo, -hi  };
                float[] ends   = { hi, Mathf.PI - lo,   Mathf.PI + hi, -lo           };

                Vector3 origin = transform.position;
                const int arcSegments = 20;

                for (int z = 0; z < 4; z++)
                {
                    float startAngle = starts[z];
                    float endAngle   = ends[z];

                    
                    Gizmos.color = new Color(1f, 0.45f, 0f, 1f);
                    Gizmos.DrawLine(origin, origin + new Vector3(Mathf.Cos(startAngle), Mathf.Sin(startAngle), 0f) * gizmoLineLength);
                    Gizmos.DrawLine(origin, origin + new Vector3(Mathf.Cos(endAngle),   Mathf.Sin(endAngle),   0f) * gizmoLineLength);

                    
                    Gizmos.color = new Color(0.1f, 1f, 0.35f, 0.8f);
                    Vector3 prev = origin + new Vector3(Mathf.Cos(startAngle), Mathf.Sin(startAngle), 0f) * gizmoLineLength;
                    for (int s = 1; s <= arcSegments; s++)
                    {
                        float a    = Mathf.Lerp(startAngle, endAngle, (float)s / arcSegments);
                        Vector3 next = origin + new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * gizmoLineLength;
                        Gizmos.DrawLine(prev, next);
                        prev = next;
                    }
                }
            }
        }

        if (showSuperBhopGizmo)
        {
            float hThr = gizmoSuperBhopHorizontalThreshold;
            float vThr = gizmoSuperBhopUpwardThreshold;
            if (hThr > 0f && hThr < 1f && vThr > 0f && vThr < 1f)
            {
                
                
                
                float rightBoundary = Mathf.Acos(hThr);  
                float leftBoundary = Mathf.Acos(-hThr);  

                Vector3 origin = transform.position;
                const int arcSegments = 20;

                
                Gizmos.color = new Color(0f, 1f, 1f, 1f);
                Gizmos.DrawLine(origin, origin + new Vector3(Mathf.Cos(rightBoundary), Mathf.Sin(rightBoundary), 0f) * gizmoLineLength);
                Gizmos.DrawLine(origin, origin + new Vector3(Mathf.Cos(leftBoundary), Mathf.Sin(leftBoundary), 0f) * gizmoLineLength);

                
                Gizmos.color = new Color(1f, 1f, 0f, 0.9f);
                Vector3 prev = origin + new Vector3(Mathf.Cos(rightBoundary), Mathf.Sin(rightBoundary), 0f) * gizmoLineLength;
                for (int s = 1; s <= arcSegments; s++)
                {
                    float a = Mathf.Lerp(rightBoundary, leftBoundary, (float)s / arcSegments);
                    Vector3 next = origin + new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * gizmoLineLength;
                    Gizmos.DrawLine(prev, next);
                    prev = next;
                }

                
                Gizmos.color = new Color(1f, 1f, 0f, 0.6f);
                Gizmos.DrawLine(origin, origin + Vector3.up * gizmoLineLength);
            }
        }
    }
}
