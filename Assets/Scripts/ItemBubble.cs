using UnityEngine;

public class ItemBubble : MonoBehaviour
{
    [Header("Items")]
    [SerializeField] private PowerUpData[] items;
    [SerializeField] private float cycleInterval = 1f;

    [Header("References")]
    [SerializeField] private SpriteRenderer iconRenderer;

    private int currentIndex;
    private float cycleTimer;
    private bool hasBeenCollected;

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
        if (hasBeenCollected) return;

        
        Bullet bullet = other.GetComponent<Bullet>();
        PlayerController player = other.GetComponent<PlayerController>();
        
        if (bullet == null && player == null) return;

        hasBeenCollected = true;

        
        if (items != null && items.Length > 0 && items[currentIndex] != null)
        {
            PowerUpManager pm = FindFirstObjectByType<PowerUpManager>();
            pm?.GivePowerUp(items[currentIndex]);
        }

        
        WeaponController weaponController = FindFirstObjectByType<WeaponController>();
        if (weaponController != null)
        {
            weaponController.AddShotgunAmmo(1);
        }

        
        Destroy(transform.parent != null ? transform.parent.gameObject : gameObject);
    }
}

