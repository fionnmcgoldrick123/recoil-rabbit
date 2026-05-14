using UnityEngine;

/// <summary>
/// Base class for all power-up effects. Create a subclass and mark it with
/// [CreateAssetMenu] to register a new power-up type in the Unity editor.
/// </summary>
public abstract class PowerUpEffect : ScriptableObject
{
    public abstract void Activate(PowerUpManager manager);
}
