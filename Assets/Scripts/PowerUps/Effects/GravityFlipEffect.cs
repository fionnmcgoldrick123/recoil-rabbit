using UnityEngine;

[CreateAssetMenu(fileName = "GravityFlipEffect", menuName = "PowerUps/Effects/Gravity Flip")]
public class GravityFlipEffect : PowerUpEffect
{
    public float duration = 4f;

    public override void Activate(PowerUpManager manager)
    {
        manager.Player.StartGravityFlip(duration);
    }
}
