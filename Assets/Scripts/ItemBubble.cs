using UnityEngine;

/// <summary>
/// Attach to a child of a bubble. Cycles through an array of PowerUpData assets,
/// displays the active item's icon, and awards that item to the player when hit by a bullet or touched by the player.
/// Also awards shotgun ammo when the bubble is destroyed.
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
        // Check if this is a bullet or the player
        Bullet bullet = other.GetComponent<Bullet>();
        PlayerController player = other.GetComponent<PlayerController>();
        
        if (bullet == null && player == null) return;

        // Award powerup to player
        if (items != null && items.Length > 0 && items[currentIndex] != null)
        {
            PowerUpManager pm = FindFirstObjectByType<PowerUpManager>();
            pm?.GivePowerUp(items[currentIndex]);
        }

        // Award shotgun ammo when bubble is destroyed
        WeaponController weaponController = FindFirstObjectByType<WeaponController>();
        if (weaponController != null)
        {
            weaponController.AddShotgunAmmo(1);
        }

        // Destroy the parent bubble (this component lives on a child object)
        Destroy(transform.parent != null ? transform.parent.gameObject : gameObject);
    }
}

