using UnityEngine;

/// <summary>
/// Attach to the bubble parent object. Makes the bubble float around in a small area.
/// Highly configurable - can be completely static or have smooth floating movement.
/// </summary>
public class BubbleFloat : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private bool enableMovement = true;
    [Tooltip("How far the bubble drifts from its starting position (0 = static).")]
    [SerializeField] private float floatRange = 0.5f;
    [Tooltip("How fast the bubble bobs up and down.")]
    [SerializeField] private float floatSpeedVertical = 1f;
    [Tooltip("How fast the bubble drifts horizontally.")]
    [SerializeField] private float floatSpeedHorizontal = 0.8f;

    private Vector3 startPosition;
    private float verticalOffset;
    private float horizontalOffset;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        if (!enableMovement)
            return;

        // Create smooth sine-wave motion in X and Y
        verticalOffset = Mathf.Sin(Time.time * floatSpeedVertical) * floatRange;
        horizontalOffset = Mathf.Cos(Time.time * floatSpeedHorizontal * 0.7f) * floatRange;

        transform.position = startPosition + new Vector3(horizontalOffset, verticalOffset, 0f);
    }

    /// <summary>
    /// Call this to toggle movement on/off at runtime.
    /// </summary>
    public void SetMovementEnabled(bool enabled)
    {
        enableMovement = enabled;
        if (!enabled && transform.position != startPosition)
            transform.position = startPosition;
    }

    /// <summary>
    /// Reset to the original starting position.
    /// </summary>
    public void ResetPosition()
    {
        transform.position = startPosition;
    }
}
