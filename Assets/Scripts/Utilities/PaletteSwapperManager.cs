using UnityEngine;

public class PaletteSwapperManager : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Data types
    // -------------------------------------------------------------------------

    [System.Serializable]
    public class PaletteData
    {
        public string paletteName = "New Palette";
        [ColorUsage(false)] public Color darkColor;
        [ColorUsage(false)] public Color highlightColor;
        [ColorUsage(false)] public Color lightColor;
    }

    // -------------------------------------------------------------------------
    // Inspector fields
    // -------------------------------------------------------------------------

    [Header("Shared Material (Unlit/PaletteSwapper)")]
    [Tooltip("The material using PaletteSwapShader. Properties are updated at runtime.")]
    [SerializeField] private Material paletteMaterial;

    [Header("Palettes — Press P to swap randomly")]
    [SerializeField] private PaletteData[] palettes = new PaletteData[]
    {
        // --- Test Palette 1: Deep Sea ---
        new PaletteData
        {
            paletteName   = "Deep Sea",
            darkColor      = new Color(0.01f, 0.04f, 0.18f),   // near-black navy
            highlightColor = new Color(0.05f, 0.55f, 0.95f),   // bright ocean blue
            lightColor     = new Color(0.82f, 0.96f, 1.00f)    // icy white-blue
        },
        // --- Test Palette 2: Toxic ---
        new PaletteData
        {
            paletteName   = "Toxic",
            darkColor      = new Color(0.04f, 0.16f, 0.00f),   // deep swamp green
            highlightColor = new Color(0.40f, 1.00f, 0.05f),   // acid green
            lightColor     = new Color(0.88f, 1.00f, 0.72f)    // pale lime
        },
        // --- Test Palette 3: Ember ---
        new PaletteData
        {
            paletteName   = "Ember",
            darkColor      = new Color(0.14f, 0.02f, 0.00f),   // charcoal red
            highlightColor = new Color(1.00f, 0.28f, 0.00f),   // hot orange
            lightColor     = new Color(1.00f, 0.94f, 0.65f)    // warm cream
        },
    };

    [Header("Override Materials (optional — swaps whole material per renderer)")]
    [Tooltip("If populated, the manager swaps entire pre-baked materials instead of editing colour properties.")]
    [SerializeField] private Material[] overrideMaterials;

    [Header("Target Renderers (for Override Materials mode)")]
    [Tooltip("Only needed when using Override Materials. Leave empty when using shared Palette Material.")]
    [SerializeField] private Renderer[] targets;

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private int _currentIndex = 0;
    private bool _useOverrideMaterials;

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------

    private void Start()
    {
        _useOverrideMaterials = overrideMaterials != null && overrideMaterials.Length > 0;

        // Apply the first palette on startup.
        if (!_useOverrideMaterials && palettes.Length > 0)
            ApplyPaletteData(palettes[0]);
        else if (_useOverrideMaterials)
            ApplyOverrideMaterial(0);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            SwapToRandomPalette();
    }

    // -------------------------------------------------------------------------
    // Palette logic
    // -------------------------------------------------------------------------

    private void SwapToRandomPalette()
    {
        int length = _useOverrideMaterials ? overrideMaterials.Length : palettes.Length;
        if (length == 0) return;

        int newIndex = _currentIndex;
        if (length > 1)
        {
            while (newIndex == _currentIndex)
                newIndex = Random.Range(0, length);
        }

        _currentIndex = newIndex;

        if (_useOverrideMaterials)
        {
            ApplyOverrideMaterial(_currentIndex);
            Debug.Log($"[PaletteSwapper] Switched to override material #{_currentIndex}.");
        }
        else
        {
            ApplyPaletteData(palettes[_currentIndex]);
            Debug.Log($"[PaletteSwapper] Switched to palette: {palettes[_currentIndex].paletteName}");
        }
    }

    /// <summary>Sets the three replacement colour properties on the shared material.</summary>
    private void ApplyPaletteData(PaletteData palette)
    {
        if (paletteMaterial == null)
        {
            Debug.LogWarning("[PaletteSwapper] No palette material assigned.");
            return;
        }

        paletteMaterial.SetColor("_RepColor1", palette.darkColor);
        paletteMaterial.SetColor("_RepColor2", palette.highlightColor);
        paletteMaterial.SetColor("_RepColor3", palette.lightColor);
    }

    /// <summary>Swaps the entire material on every target renderer.</summary>
    private void ApplyOverrideMaterial(int index)
    {
        if (targets == null || targets.Length == 0)
        {
            Debug.LogWarning("[PaletteSwapper] Override Materials mode is active but no Target Renderers are assigned.");
            return;
        }

        Material mat = overrideMaterials[index];
        foreach (Renderer r in targets)
        {
            if (r != null)
                r.sharedMaterial = mat;
        }
    }

    // -------------------------------------------------------------------------
    // Public API (call from other scripts if needed)
    // -------------------------------------------------------------------------

    /// <summary>Immediately apply a palette by its index in the Palettes array.</summary>
    public void SetPalette(int index)
    {
        if (_useOverrideMaterials)
        {
            if (index < 0 || index >= overrideMaterials.Length) return;
            _currentIndex = index;
            ApplyOverrideMaterial(_currentIndex);
        }
        else
        {
            if (index < 0 || index >= palettes.Length) return;
            _currentIndex = index;
            ApplyPaletteData(palettes[_currentIndex]);
        }
    }
}
