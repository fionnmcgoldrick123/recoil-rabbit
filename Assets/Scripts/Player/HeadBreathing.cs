using UnityEngine;

public class HeadBreathing : MonoBehaviour
{
    [SerializeField] private BreathingAnimator breathingAnimator;

    private Vector3 restingLocalPosition;

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
        if (!breathingAnimator.IsBreathing)
        {
            
            transform.localPosition = restingLocalPosition;
            return;
        }

        
        float breathingOffset = breathingAnimator.GetBreathingOffset();
        
        
        transform.localPosition = restingLocalPosition + Vector3.up * breathingOffset;
    }

    public void ResetHeadPosition()
    {
        
        transform.localPosition = restingLocalPosition;
    }
}
