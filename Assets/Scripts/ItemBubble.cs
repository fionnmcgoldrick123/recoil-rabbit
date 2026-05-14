using UnityEngine;

/// <summary>
/// Attach to a child of a bubble. Cycles through an array of PowerUpData assets,
/// displays the active item's icon, and awards that item to the player when hit by a bullet.
/// </summary>
public class ItemBubble : MonoBehaviour
{
    [Header("Items")]
    [SerializeField] private PowerUpData[] items;
    [Tooltip("Seconds between each item cycle.")]
    [SerializeField] private float cycleInterval = 1f;

    [Header("References")]
    [SerializeField] private SpriteRenderer iconRenderer;

    private int currentIndex;
    private float cycleTimer;

    private void Start()
    {
        currentIndex = 0;
        cycleTimer = 0f;
        UpdateIcon();
    }

    private void Update()
    {
        if (items == null || items.Length <= 1) return;

        cycleTimer += Time.deltaTime;
        if (cycleTimer >= cycleInterval)
        {
            cycleTimer = 0f;
            currentIndex = (currentIndex + 1) % items.Length;
            UpdateIcon();
        }
    }

    private void UpdateIcon()
    {
        if (iconRenderer == null || items == null || items.Length == 0) return;
        iconRenderer.sprite = items[currentIndex] != null ? items[currentIndex].icon : null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<Bullet>() == null) return;

        if (items != null && items.Length > 0 && items[currentIndex] != null)
        {
            PowerUpManager pm = FindFirstObjectByType<PowerUpManager>();
            pm?.GivePowerUp(items[currentIndex]);
        }

        // Destroy the parent bubble (this component lives on a child object)
        Destroy(transform.parent != null ? transform.parent.gameObject : gameObject);
    }
}

