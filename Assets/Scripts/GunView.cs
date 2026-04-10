using UnityEngine;

/// <summary>
/// Attach to the gun sprite GameObject (child of player).
/// The gun sprite should point RIGHT by default (along +X).
/// FirePoint should be a child of the gun, offset in +X at the barrel tip.
/// </summary>
public class GunView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer gunRenderer;
    [SerializeField] private Transform playerBody;
    [SerializeField] private Rigidbody2D playerRb;

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;

        if (gunRenderer == null)
            gunRenderer = GetComponent<SpriteRenderer>();

        if (playerBody == null)
            playerBody = transform.parent;

        if (playerRb == null && playerBody != null)
            playerRb = playerBody.GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        Vector2 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 gunPivot = transform.position;
        Vector2 dirToMouse = (mouseWorld - gunPivot).normalized;

        // Rotate gun to aim at mouse in world space
        float angle = Mathf.Atan2(dirToMouse.y, dirToMouse.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Determine facing direction: mouse wins, velocity is fallback
        bool facingLeft = dirToMouse.x < 0;

        // Flip player body using localScale so all children (except gun rotation) mirror correctly
        if (playerBody != null)
        {
            Vector3 scale = playerBody.localScale;
            scale.x = facingLeft ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
            playerBody.localScale = scale;
        }

        // Flip gun sprite Y so it doesn't appear upside-down when facing left
        // This compensates for the parent scale flip on the sprite visual only
        gunRenderer.flipY = facingLeft;
    }
}
