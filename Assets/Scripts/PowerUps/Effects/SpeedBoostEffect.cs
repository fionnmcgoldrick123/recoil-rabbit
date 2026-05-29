using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "SpeedBoostEffect", menuName = "PowerUps/Effects/Speed Boost")]
public class SpeedBoostEffect : PowerUpEffect
{
    public float speedMultiplier = 2f;

    public float duration = 3f;

    public override void Activate(PowerUpManager manager)
    {
        manager.Player.ApplySpeedBoost(speedMultiplier, duration);
    }
}
