using UnityEngine;

public class PaletteSwapperManager : MonoBehaviour
{

    [Header("Shared Material (Unlit/PaletteSwapper)")]
    [Tooltip("The material using PaletteSwapShader. Properties are updated at runtime.")]
    [SerializeField] private Material paletteMaterial;

    [Header("Palette ScriptableObjects — Press P to cycle through")]
    [Tooltip("Array of PaletteData scriptable objects. Press P to cycle through them sequentially.")]
    [SerializeField] private PaletteData[] paletteScriptables;

    [Header("Default Palette (for startup and scene transitions)")]
    [Tooltip("The default palette to apply on startup and when the scene ends.")]
    [SerializeField] private PaletteData defaultPalette;

    private int _currentIndex = 0;
    private Color _currentAfterImageColor;

    private void Start()
    {
        if (defaultPalette != null)
        {
            ApplyPaletteData(defaultPalette);
            _currentIndex = 0;
        }
        else if (paletteScriptables != null && paletteScriptables.Length > 0)
        {
            ApplyPaletteData(paletteScriptables[0]);
            _currentIndex = 0;
        }
        else
        {
            Debug.LogWarning("[PaletteSwapper] No palette scriptables assigned.");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            CycleToNextPalette();
    }

    private void OnDestroy()
    {
        if (defaultPalette != null)
        {
            ApplyPaletteData(defaultPalette);
        }
    }

    private void CycleToNextPalette()
    {
        if (paletteScriptables == null || paletteScriptables.Length == 0)
        {
            Debug.LogWarning("[PaletteSwapper] No palette scriptables assigned.");
            return;
        }

        _currentIndex = (_currentIndex + 1) % paletteScriptables.Length;
        ApplyPaletteData(paletteScriptables[_currentIndex]);
        Debug.Log($"[PaletteSwapper] Switched to palette: {paletteScriptables[_currentIndex].paletteName}");
    }

    private void ApplyPaletteData(PaletteData palette)
    {
        if (paletteMaterial == null)
        {
            Debug.LogWarning("[PaletteSwapper] No palette material assigned.");
            return;
        }

        if (palette == null)
        {
            Debug.LogWarning("[PaletteSwapper] Palette scriptable is null.");
            return;
        }

        paletteMaterial.SetColor("_RepColor1", palette.darkColor);
        paletteMaterial.SetColor("_RepColor2", palette.highlightColor);
        paletteMaterial.SetColor("_RepColor3", palette.lightColor);

        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.backgroundColor = palette.backgroundColor;
        }

        _currentAfterImageColor = palette.afterImageColor;
    }

    public Color GetCurrentAfterImageColor()
    {
        return _currentAfterImageColor;
    }

    public void SetPalette(int index)
    {
        if (index >= 0 && index < paletteScriptables.Length)
        {
            _currentIndex = index;
            ApplyPaletteData(paletteScriptables[_currentIndex]);
        }
        else
        {
            Debug.LogWarning($"[PaletteSwapper] Invalid palette index: {index}");
        }
    }
}
