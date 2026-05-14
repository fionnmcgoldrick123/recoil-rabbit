using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach to the Player. Holds the player's current power-up and activates it on Q.
/// </summary>
public class PowerUpManager : MonoBehaviour
{
    [Header("UI (Optional)")]
    [SerializeField] private Image powerUpIconUI;
    [SerializeField] private GameObject powerUpIconRoot;

    private PowerUpData heldPowerUp;

    public PlayerController Player { get; private set; }
    public WeaponController Weapon { get; private set; }

    private void Awake()
    {
        Player = GetComponent<PlayerController>();
        Weapon = GetComponentInChildren<WeaponController>();

        if (powerUpIconRoot != null)
            powerUpIconRoot.SetActive(false);
    }

    private void Update()
    {
        if (Player != null && Player.IsDead) return;

        if (Input.GetKeyDown(KeyCode.E) && heldPowerUp != null)
            ActivatePowerUp();
    }

    public void GivePowerUp(PowerUpData data)
    {
        heldPowerUp = data;
        UpdateUI();
    }

    private void ActivatePowerUp()
    {
        if (heldPowerUp.effect != null)
            heldPowerUp.effect.Activate(this);

        heldPowerUp = null;
        UpdateUI();
    }

    private void UpdateUI()
    {
        bool hasPowerUp = heldPowerUp != null;

        if (powerUpIconRoot != null)
            powerUpIconRoot.SetActive(hasPowerUp);

        if (powerUpIconUI != null)
            powerUpIconUI.sprite = hasPowerUp ? heldPowerUp.icon : null;
    }
}
