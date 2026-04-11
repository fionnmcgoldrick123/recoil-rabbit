using UnityEngine;

public class AnimationEventRelay : MonoBehaviour
{
    private PlayerController playerController;

    private void Awake()
    {
        playerController = GetComponentInParent<PlayerController>();

        if (playerController == null)
            Debug.LogError("[AnimationEventRelay] PlayerController not found in parent hierarchy!");
    }

    private void onDeathAnimationComplete()
    {
        if (playerController != null)
            playerController.OnDeathAnimationComplete();
    }
    
}
