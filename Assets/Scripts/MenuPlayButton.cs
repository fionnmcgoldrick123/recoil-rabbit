using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MenuPlayButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private string sceneName;
    [SerializeField] private float hoverScale = 1.08f;
    [SerializeField] private float scaleSpeed = 12f;

    private Vector3 baseScale;
    private Vector3 targetScale;

    private void Awake()
    {
        baseScale = transform.localScale;
        targetScale = baseScale;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * scaleSpeed);
    }

    private void OnDisable()
    {
        transform.localScale = baseScale;
        targetScale = baseScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = baseScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = baseScale;
    }

    public void PlayGame()
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("MenuPlayButton needs a scene name assigned.", this);
            return;
        }

        if (SceneTransition.Instance != null)
            SceneTransition.LoadScene(sceneName);
        else
            SceneManager.LoadScene(sceneName);
    }
}