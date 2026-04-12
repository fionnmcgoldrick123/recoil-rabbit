using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelGoal : MonoBehaviour
{
    [Header("Next Scene")]
    [SerializeField] private string nextSceneName;
    [SerializeField] private int nextSceneIndex = -1;

    [Header("Settings")]
    [SerializeField] private float loadDelay = 0.5f;

    private ParticleSystem particles;
    private SpriteRenderer spriteRenderer;

    private bool collected = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        particles = GetComponentInChildren<ParticleSystem>();
        spriteRenderer.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;

        if (other.GetComponent<PlayerController>() != null)
        {
            collected = true;
            if (particles != null)
                particles.Play();

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayLevelGoal();
                
            if (spriteRenderer != null)
                spriteRenderer.enabled = false;

            Invoke(nameof(LoadNextScene), loadDelay);
        }
    }

    private void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneTransition.LoadScene(nextSceneName);
        }
        else if (nextSceneIndex >= 0)
        {
            SceneTransition.LoadScene(nextSceneIndex);
        }
        else
        {
            int next = SceneManager.GetActiveScene().buildIndex + 1;
            SceneTransition.LoadScene(next);
        }
    }
}
