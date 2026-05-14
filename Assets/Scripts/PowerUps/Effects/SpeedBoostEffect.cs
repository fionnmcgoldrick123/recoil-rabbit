using System.Collections;
using UnityEngine;

/// <summary>
/// Temporarily multiplies the player's movement speed for a set duration.
/// Create via Assets > Create > PowerUps > Effects > Speed Boost.
/// </summary>
[CreateAssetMenu(fileName = "SpeedBoostEffect", menuName = "PowerUps/Effects/Speed Boost")]
public class SpeedBoostEffect : PowerUpEffect
{
    [Tooltip("Multiplier applied to max movement speed.")]
    public float speedMultiplier = 2f;

    [Tooltip("How long the boost lasts in seconds.")]
    public float duration = 3f;

    public override void Activate(PowerUpManager manager)
    {
        manager.Player.ApplySpeedBoost(speedMultiplier, duration);
    }
}
