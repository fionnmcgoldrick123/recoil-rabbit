using UnityEngine;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{
    [Header("Crosshair")]
    [SerializeField] private Image crosshairImage;

    private RectTransform rectTransform;

    private void Awake()
    {
        if (crosshairImage == null)
            crosshairImage = GetComponent<Image>();

        if (crosshairImage != null)
            crosshairImage.raycastTarget = false;

        rectTransform = GetComponent<RectTransform>();

        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null)
            transform.SetAsLastSibling();

        Cursor.visible = false;
    }

    private void Update()
    {
        if (rectTransform == null) return;

        Vector3 mouseScreenPos = Input.mousePosition;
        rectTransform.position = mouseScreenPos;
    }
}
