using UnityEngine;

public class GunView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer gunRenderer;
    [SerializeField] private Transform playerBody;
    [SerializeField] private Rigidbody2D playerRb;
    [SerializeField] private Animator playerAnimator;

    [Header("Aiming")]
    [SerializeField] private float horizontalDeadzone = 0.2f;
    [SerializeField] private float centerDeadzone = 0.5f;

    [Header("Idle Breathing Animation")]
    [SerializeField] private float breathingAmount = 0.3f;
    [SerializeField] private float breathingSpeed = 2f;

    private Camera mainCamera;
    private bool lastFacingLeft = false;
    private Vector3 restingLocalPosition;
    private bool isBreathing = false;
    private float breathingTimer = 0f;

    private void Awake()
    {
        mainCamera = Camera.main;

        if (gunRenderer == null)
            gunRenderer = GetComponent<SpriteRenderer>();

        if (playerBody == null)
            playerBody = transform.parent;

        if (playerRb == null && playerBody != null)
            playerRb = playerBody.GetComponent<Rigidbody2D>();

        if (playerAnimator == null && playerBody != null)
            playerAnimator = playerBody.GetComponent<Animator>();

        // Store the resting local position (pointing up)
        restingLocalPosition = transform.localPosition;
    }

    private void Update()
    {
        if (Time.timeScale == 0) return;

        Vector2 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 gunPivot = transform.position;
        Vector2 rawDir = mouseWorld - gunPivot;

        // Skip update if mouse is too close to the pivot to avoid erratic flipping
        if (rawDir.magnitude < centerDeadzone)
            return;

        Vector2 dirToMouse = rawDir.normalized;

        float angle = Mathf.Atan2(dirToMouse.y, dirToMouse.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        bool facingLeft = Mathf.Abs(dirToMouse.x) > horizontalDeadzone ? dirToMouse.x < 0 : lastFacingLeft;
        lastFacingLeft = facingLeft;

        if (playerBody != null)
        {
            Vector3 playerScale = playerBody.localScale;
            playerScale.x = facingLeft ? -Mathf.Abs(playerScale.x) : Mathf.Abs(playerScale.x);
            playerBody.localScale = playerScale;

            Vector3 gunScale = transform.localScale;
            gunScale.x = playerScale.x < 0 ? -1f : 1f;
            transform.localScale = gunScale;
        }

        gunRenderer.flipY = facingLeft;

        // Update idle breathing animation
        UpdateIdleBreathing();
    }

    private void UpdateIdleBreathing()
    {
        // Check if player is in idle state (Speed = 0 and IsGrounded = true)
        bool shouldBreathe = false;
        if (playerAnimator != null)
        {
            float speed = playerAnimator.GetFloat("Speed");
            bool isGrounded = playerAnimator.GetBool("IsGrounded");
            shouldBreathe = speed < 0.1f && isGrounded;
        }

        if (shouldBreathe && !isBreathing)
        {
            // Just entered idle state
            isBreathing = true;
            breathingTimer = 0f;
        }
        else if (!shouldBreathe && isBreathing)
        {
            // Left idle state - snap back to resting position
            isBreathing = false;
            ResetGunPosition();
        }

        // Apply breathing animation if idle
        if (isBreathing)
        {
            breathingTimer += Time.deltaTime * breathingSpeed;
            
            // Calculate breathing offset using sine wave
            // This creates a smooth up and down motion
            float breathingOffset = Mathf.Sin(breathingTimer * Mathf.PI) * breathingAmount;
            
            // Apply the offset to the local position (Y axis in world space)
            // The breathing happens perpendicular to the current aim direction
            Vector3 currentLocalPos = transform.localPosition;
            Vector3 upDirection = Vector3.up;
            
            // Get the current rotation's up direction in local space
            Vector3 rotatedUpDirection = transform.rotation * Vector3.up;
            
            transform.localPosition = restingLocalPosition + rotatedUpDirection * breathingOffset;
        }
    }

    public void ResetGunPosition()
    {
        // Snap back to resting position
        transform.localPosition = restingLocalPosition;
    }
}
