using UnityEngine;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{
    [Header("Crosshair")]
    [SerializeField] private Image crosshairImage;

    private Canvas canvas;
    private RectTransform rectTransform;

    private void Awake()
    {
        if (crosshairImage == null)
            crosshairImage = GetComponent<Image>();

        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        if (canvas == null)
            Debug.LogError("[Crosshair] Crosshair must be inside a Canvas!");

        Cursor.visible = false;
    }

    private void Update()
    {
        if (rectTransform == null) return;

        Vector3 mouseScreenPos = Input.mousePosition;
        rectTransform.position = mouseScreenPos;
    }
}
