using UnityEngine;

/// <summary>
/// Flips the player's gravity, allowing them to walk on the ceiling.
/// Movement directions stay the same (right is still right).
/// Gun and all mechanics work normally during the flip.
/// Create via Assets > Create > PowerUps > Effects > Gravity Flip.
/// </summary>
[CreateAssetMenu(fileName = "GravityFlipEffect", menuName = "PowerUps/Effects/Gravity Flip")]
public class GravityFlipEffect : PowerUpEffect
{
    [Tooltip("How long the gravity flip lasts in seconds.")]
    public float duration = 4f;

    public override void Activate(PowerUpManager manager)
    {
        manager.Player.StartGravityFlip(duration);
    }
}
