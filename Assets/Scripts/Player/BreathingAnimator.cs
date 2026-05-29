using UnityEngine;

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
        
        bool shouldBreathe = false;
        if (playerAnimator != null)
        {
            float speed = playerAnimator.GetFloat("Speed");
            bool isGrounded = playerAnimator.GetBool("IsGrounded");
            shouldBreathe = speed < 0.1f && isGrounded;
        }

        if (shouldBreathe && !isBreathing)
        {
            
            isBreathing = true;
            breathingTimer = 0f;
        }
        else if (!shouldBreathe && isBreathing)
        {
            
            isBreathing = false;
        }

        
        if (isBreathing)
        {
            breathingTimer += Time.deltaTime * breathingSpeed;
        }
    }

    public float GetBreathingOffset()
    {
        if (!isBreathing)
            return 0f;

        
        
        return Mathf.Sin(breathingTimer * Mathf.PI) * breathingAmount;
    }

    public bool IsBreathing => isBreathing;
}
