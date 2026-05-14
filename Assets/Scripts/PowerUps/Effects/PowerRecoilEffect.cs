using UnityEngine;

/// <summary>
/// Makes the player's next shotgun shot apply a greatly amplified recoil force.
/// Create via Assets > Create > PowerUps > Effects > Power Recoil.
/// </summary>
[CreateAssetMenu(fileName = "PowerRecoilEffect", menuName = "PowerUps/Effects/Power Recoil")]
public class PowerRecoilEffect : PowerUpEffect
{
    [Tooltip("Multiplier applied to the shotgun's recoil force on the next shot.")]
    public float recoilMultiplier = 4f;

    public override void Activate(PowerUpManager manager)
    {
        manager.Weapon.SetNextShotRecoilMultiplier(recoilMultiplier);
    }
}
