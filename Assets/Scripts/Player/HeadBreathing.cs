using UnityEngine;

/// <summary>
/// Applies idle breathing animation to the player's head.
/// Syncs with BreathingAnimator to match gun breathing.
/// </summary>
public class HeadBreathing : MonoBehaviour
{
    [SerializeField] private BreathingAnimator breathingAnimator;

    private Vector3 restingLocalPosition;

    private void Awake()
    {
        if (breathingAnimator == null)
        {
            // Find BreathingAnimator on parent (player object)
            Transform parent = transform.parent;
            if (parent != null)
                breathingAnimator = parent.GetComponent<BreathingAnimator>();
        }

        // Store the resting local position
        restingLocalPosition = transform.localPosition;
    }

    private void Update()
    {
        if (breathingAnimator == null)
            return;

        UpdateHeadBreathing();
    }

    private void UpdateHeadBreathing()
    {
        if (!breathingAnimator.IsBreathing)
        {
            // Not breathing - snap to resting position
            transform.localPosition = restingLocalPosition;
            return;
        }

        // Apply breathing animation
        float breathingOffset = breathingAnimator.GetBreathingOffset();
        
        // Head moves up and down (Y axis)
        transform.localPosition = restingLocalPosition + Vector3.up * breathingOffset;
    }

    public void ResetHeadPosition()
    {
        // Snap back to resting position immediately when shooting
        transform.localPosition = restingLocalPosition;
    }
}
