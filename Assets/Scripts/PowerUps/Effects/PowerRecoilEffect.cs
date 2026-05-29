using UnityEngine;

[CreateAssetMenu(fileName = "PowerRecoilEffect", menuName = "PowerUps/Effects/Power Recoil")]
public class PowerRecoilEffect : PowerUpEffect
{
    public float recoilMultiplier = 4f;

    public override void Activate(PowerUpManager manager)
    {
        manager.Weapon.SetNextShotRecoilMultiplier(recoilMultiplier);
    }
}
