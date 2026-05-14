using UnityEngine;

/// <summary>
/// Grants unlimited shotgun shells for a set duration.
/// Create via Assets > Create > PowerUps > Effects > Infinite Ammo.
/// </summary>
[CreateAssetMenu(fileName = "InfiniteAmmoEffect", menuName = "PowerUps/Effects/Infinite Ammo")]
public class InfiniteAmmoEffect : PowerUpEffect
{
    [Tooltip("How long infinite ammo lasts in seconds.")]
    public float duration = 2f;

    public override void Activate(PowerUpManager manager)
    {
        manager.Weapon.StartInfiniteAmmoOverride(duration);
    }
}
