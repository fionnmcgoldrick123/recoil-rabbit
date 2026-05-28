using UnityEngine;

/// <summary>
/// Manages idle breathing animation for player body parts.
/// Both gun and head reference this to stay in sync.
/// </summary>
public class BreathingAnimator : MonoBehaviour
{
    [Header("Idle Breathing Animation")]
    [SerializeField] private float breathingAmount = 0.3f;
    [SerializeField] private float breathingSpeed = 2f;

    [SerializeField] private Animator playerAnimator;

    private bool isBreathing = false;
    private float breathingTimer = 0f;

    private void Awake()
    {
        if (playerAnimator == null)
            playerAnimator = GetComponent<Animator>();
    }

    private void Update()
    {
        UpdateBreathingState();
    }

    private void UpdateBreathingState()
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
            // Left idle state
            isBreathing = false;
        }

        // Update breathing timer
        if (isBreathing)
        {
            breathingTimer += Time.deltaTime * breathingSpeed;
        }
    }

    /// <summary>
    /// Gets the current breathing offset. Use this to animate body parts.
    /// Returns 0 if not breathing.
    /// </summary>
    public float GetBreathingOffset()
    {
        if (!isBreathing)
            return 0f;

        // Calculate breathing offset using sine wave (0 to 1 to 0)
        // This creates a smooth up and down motion
        return Mathf.Sin(breathingTimer * Mathf.PI) * breathingAmount;
    }

    public bool IsBreathing => isBreathing;
}
