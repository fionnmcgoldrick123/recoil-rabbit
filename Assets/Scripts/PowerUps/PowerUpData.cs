using UnityEngine;

/// <summary>
/// Defines a power-up item. Assign an icon and a PowerUpEffect asset.
/// Create via Assets > Create > PowerUps > Power Up Data.
/// </summary>
[CreateAssetMenu(fileName = "NewPowerUp", menuName = "PowerUps/Power Up Data")]
public class PowerUpData : ScriptableObject
{
    [Header("Display")]
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Effect")]
    public PowerUpEffect effect;
}
