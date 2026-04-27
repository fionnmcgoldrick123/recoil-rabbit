using UnityEngine;

public class GunView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer gunRenderer;
    [SerializeField] private Transform playerBody;
    [SerializeField] private Rigidbody2D playerRb;

    [Header("Aiming")]
    [SerializeField] private float horizontalDeadzone = 0.2f;
    [SerializeField] private float centerDeadzone = 0.5f;

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
        if (Time.timeScale == 0) return;

        Vector2 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 gunPivot = transform.position;
        Vector2 rawDir = mouseWorld - gunPivot;

        // Skip update if mouse is too close to the pivot to avoid erratic flipping
        if (rawDir.magnitude < centerDeadzone)
            return;

        Vector2 dirToMouse = rawDir.normalized;

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
