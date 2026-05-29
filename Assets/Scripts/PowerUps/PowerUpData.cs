using UnityEngine;

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
