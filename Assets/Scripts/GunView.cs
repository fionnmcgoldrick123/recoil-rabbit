using UnityEngine;

public class GunView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer gunRenderer;
    [SerializeField] private Transform playerBody;
    [SerializeField] private Rigidbody2D playerRb;

    [Header("Aiming")]
    [SerializeField] private float horizontalDeadzone = 0.2f;

    private Camera mainCamera;
    private bool lastFacingLeft = false;

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

        float angle = Mathf.Atan2(dirToMouse.y, dirToMouse.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        bool facingLeft = Mathf.Abs(dirToMouse.x) > horizontalDeadzone ? dirToMouse.x < 0 : lastFacingLeft;
        lastFacingLeft = facingLeft;

        if (playerBody != null)
        {
            Vector3 playerScale = playerBody.localScale;
            playerScale.x = facingLeft ? -Mathf.Abs(playerScale.x) : Mathf.Abs(playerScale.x);
            playerBody.localScale = playerScale;

            Vector3 gunScale = transform.localScale;
            gunScale.x = playerScale.x < 0 ? -1f : 1f;
            transform.localScale = gunScale;
        }

        gunRenderer.flipY = facingLeft;
    }
}
