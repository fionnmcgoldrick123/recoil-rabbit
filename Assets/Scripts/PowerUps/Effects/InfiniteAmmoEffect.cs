using UnityEngine;

[CreateAssetMenu(fileName = "InfiniteAmmoEffect", menuName = "PowerUps/Effects/Infinite Ammo")]
public class InfiniteAmmoEffect : PowerUpEffect
{
    public float duration = 2f;

    public override void Activate(PowerUpManager manager)
    {
        manager.Weapon.StartInfiniteAmmoOverride(duration);
    }
}
