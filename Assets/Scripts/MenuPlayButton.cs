using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPlayButton : MonoBehaviour
{
    [SerializeField] private string sceneName;

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