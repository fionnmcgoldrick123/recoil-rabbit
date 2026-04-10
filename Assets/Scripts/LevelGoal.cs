using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelGoal : MonoBehaviour
{
    [Header("Next Scene")]
    [SerializeField] private string nextSceneName;
    [SerializeField] private int nextSceneIndex = -1;

    [Header("Settings")]
    [SerializeField] private float loadDelay = 0.5f;

    private bool collected = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;

        if (other.GetComponent<PlayerController>() != null)
        {
            collected = true;
            Invoke(nameof(LoadNextScene), loadDelay);
        }
    }

    private void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else if (nextSceneIndex >= 0)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            int next = SceneManager.GetActiveScene().buildIndex + 1;
            SceneManager.LoadScene(next);
        }
    }
}
