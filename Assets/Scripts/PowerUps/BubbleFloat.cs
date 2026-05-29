using UnityEngine;

public class BubbleFloat : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private bool enableMovement = true;
    [SerializeField] private float floatRange = 0.5f;
    [SerializeField] private float floatSpeedVertical = 1f;
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

        
        verticalOffset = Mathf.Sin(Time.time * floatSpeedVertical) * floatRange;
        horizontalOffset = Mathf.Cos(Time.time * floatSpeedHorizontal * 0.7f) * floatRange;

        transform.position = startPosition + new Vector3(horizontalOffset, verticalOffset, 0f);
    }

    public void SetMovementEnabled(bool enabled)
    {
        enableMovement = enabled;
        if (!enabled && transform.position != startPosition)
            transform.position = startPosition;
    }

    public void ResetPosition()
    {
        transform.position = startPosition;
    }
}
