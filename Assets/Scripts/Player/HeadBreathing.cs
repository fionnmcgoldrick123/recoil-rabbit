using UnityEngine;

public class HeadBreathing : MonoBehaviour
{
    [SerializeField] private BreathingAnimator breathingAnimator;
    
    [Header("Availability Bob")]
    [SerializeField] private float bobDownDistance = 0.15f;
    [SerializeField] private float bobDuration = 0.3f;

    private Vector3 restingLocalPosition;
    private float bobTimer = 0f;
    private bool isBobbing = false;

    private void Awake()
    {
        if (breathingAnimator == null)
        {
            
            Transform parent = transform.parent;
            if (parent != null)
                breathingAnimator = parent.GetComponent<BreathingAnimator>();
        }

        
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
        float totalOffset = 0f;
        
        // Handle availability bob animation
        if (isBobbing)
        {
            bobTimer += Time.deltaTime;
            if (bobTimer >= bobDuration)
            {
                isBobbing = false;
                bobTimer = 0f;
            }
            else
            {
                // Smooth bob down and back up
                float t = bobTimer / bobDuration;
                float bobCurve = Mathf.Sin(t * Mathf.PI); // 0 -> 1 -> 0
                totalOffset = -bobDownDistance * bobCurve;
            }
        }
        
        // Add breathing animation on top
        if (breathingAnimator.IsBreathing)
        {
            float breathingOffset = breathingAnimator.GetBreathingOffset();
            totalOffset += breathingOffset;
        }
        
        transform.localPosition = restingLocalPosition + Vector3.up * totalOffset;
    }
    
    public void TriggerAvailabilityBob()
    {
        isBobbing = true;
        bobTimer = 0f;
    }

    public void ResetHeadPosition()
    {
        
        transform.localPosition = restingLocalPosition;
        isBobbing = false;
        bobTimer = 0f;
    }
}
