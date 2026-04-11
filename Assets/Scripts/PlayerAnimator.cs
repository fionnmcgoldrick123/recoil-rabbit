using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Header("Player Controller")]
    [SerializeField] private PlayerController playerController;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (playerController == null)
            playerController = GetComponent<PlayerController>();

        if (animator == null)
            Debug.LogError("[PlayerAnimator] Animator not found on this GameObject!");
        if (rb == null)
            Debug.LogError("[PlayerAnimator] Rigidbody2D not found on this GameObject!");
        if (playerController == null)
            Debug.LogError("[PlayerAnimator] PlayerController not found!");
    }

    private void Update()
    {
        if (animator == null || rb == null || playerController == null)
            return;

        UpdateAnimationParameters();
    }

    private void UpdateAnimationParameters()
    {
        // Speed parameter — driven by horizontal velocity magnitude
        float speed = Mathf.Abs(rb.linearVelocity.x);
        animator.SetFloat("Speed", speed);

        // IsGrounded parameter — from PlayerController's raycast check
        bool isGrounded = playerController.IsGrounded;
        animator.SetBool("IsGrounded", isGrounded);
    }
}

