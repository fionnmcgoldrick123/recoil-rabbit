using UnityEngine;

[CreateAssetMenu(fileName = "Palette", menuName = "ScriptableObjects/Palette", order = 1)]
public class PaletteData : ScriptableObject
{
    [SerializeField] public string paletteName = "New Palette";
    [SerializeField] [ColorUsage(false)] public Color darkColor;
    [SerializeField] [ColorUsage(false)] public Color highlightColor;
    [SerializeField] [ColorUsage(false)] public Color lightColor;
    [SerializeField] [ColorUsage(false)] public Color backgroundColor;
    [SerializeField] [ColorUsage(false)] public Color afterImageColor;
}
