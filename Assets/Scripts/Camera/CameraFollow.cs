using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Smoothing")]
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private Vector3 offset = Vector3.zero;

    [Header("Directional Offset")]
    [SerializeField] private bool useDirectionalOffset = false;
    [SerializeField] private float directionOffsetX = 2f;
    [SerializeField] private float directionOffsetY = 2f;
    [SerializeField] private float velocityThreshold = 0.1f;
    [SerializeField] private float directionOffsetSmoothSpeed = 3f;

    [Header("Bounds (Optional)")]
    [SerializeField] private bool useBounds = false;
    [SerializeField] private Vector2 minBounds;
    [SerializeField] private Vector2 maxBounds;

    private Rigidbody2D targetRb;
    private Vector2 currentDirectionalOffset = Vector2.zero;

    private void Start()
    {
        if (target != null)
        {
            targetRb = target.GetComponent<Rigidbody2D>();
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;

        
        if (useDirectionalOffset && targetRb != null)
        {
            Vector2 velocity = targetRb.linearVelocity;

            Vector2 targetDirectionalOffset = Vector2.zero;

            
            if (velocity.magnitude > velocityThreshold)
            {
                Vector2 direction = velocity.normalized;

                
                targetDirectionalOffset = new Vector2(direction.x * directionOffsetX, direction.y * directionOffsetY);
            }

            
            currentDirectionalOffset = Vector2.Lerp(currentDirectionalOffset, targetDirectionalOffset, directionOffsetSmoothSpeed * Time.deltaTime);

            
            desiredPosition.x += currentDirectionalOffset.x;
            desiredPosition.y += currentDirectionalOffset.y;
        }

        desiredPosition.z = transform.position.z;

        if (useBounds)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minBounds.x, maxBounds.x);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minBounds.y, maxBounds.y);
        }

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}
