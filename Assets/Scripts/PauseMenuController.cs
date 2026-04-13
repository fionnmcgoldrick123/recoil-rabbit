using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenuRoot;
    [SerializeField] private Animator pauseAnimator;
    [SerializeField] private string openTrigger = "Open";
    [SerializeField] private string closeTrigger = "Close";
    [SerializeField] private float openAnimationDuration = 0.16f;
    [SerializeField] private float closeAnimationDuration = 0.12f;

    private WeaponController weaponController;
    private bool isPaused;
    private bool isTransitioning;

    private void Awake()
    {
        if (pauseMenuRoot == null)
            pauseMenuRoot = transform.Find("PauseObject")?.gameObject;

        if (pauseAnimator == null)
            pauseAnimator = GetComponent<Animator>();

        if (pauseAnimator != null)
            pauseAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;

        weaponController = FindFirstObjectByType<WeaponController>(FindObjectsInactive.Include);

        WireButton("ResumeButton", ResumeGame);
        WireButton("QuitButton", GoToMainMenu);

        if (pauseMenuRoot != null)
            pauseMenuRoot.SetActive(false);
    }

    private void Update()
    {
        if (!CanPauseInCurrentScene())
            return;

        if (isTransitioning)
            return;

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        if (!CanPauseInCurrentScene() || isPaused || isTransitioning)
            return;

        StartCoroutine(PauseRoutine());
    }

    public void ResumeGame()
    {
        if (!CanPauseInCurrentScene() || !isPaused || isTransitioning)
            return;

        StartCoroutine(ResumeRoutine());
    }

    public void GoToMainMenu()
    {
        if (isTransitioning)
            return;

        StartCoroutine(GoToMainMenuRoutine());
    }

    private IEnumerator PauseRoutine()
    {
        isTransitioning = true;
        Time.timeScale = 0f;
        RunTimerManager.PauseTimer();
        RunTimerManager.SetVisible(false);

        if (weaponController == null)
            weaponController = FindFirstObjectByType<WeaponController>(FindObjectsInactive.Include);

        if (weaponController != null)
            weaponController.HideShotgunHud();

        if (pauseMenuRoot != null)
            pauseMenuRoot.SetActive(true);

        TriggerAnimation(openTrigger);
        if (openAnimationDuration > 0f)
            yield return new WaitForSecondsRealtime(openAnimationDuration);

        isPaused = true;
        isTransitioning = false;
    }

    private IEnumerator ResumeRoutine()
    {
        isTransitioning = true;

        TriggerAnimation(closeTrigger);
        if (closeAnimationDuration > 0f)
            yield return new WaitForSecondsRealtime(closeAnimationDuration);

        if (pauseMenuRoot != null)
            pauseMenuRoot.SetActive(false);

        Time.timeScale = 1f;
        RunTimerManager.SetVisible(true);
        RunTimerManager.ResumeTimer();

        if (weaponController == null)
            weaponController = FindFirstObjectByType<WeaponController>(FindObjectsInactive.Include);

        if (weaponController != null)
            weaponController.ShowShotgunHud();

        isPaused = false;
        isTransitioning = false;
    }

    private IEnumerator GoToMainMenuRoutine()
    {
        isTransitioning = true;

        TriggerAnimation(closeTrigger);
        if (closeAnimationDuration > 0f)
            yield return new WaitForSecondsRealtime(closeAnimationDuration);

        if (pauseMenuRoot != null)
            pauseMenuRoot.SetActive(false);

        Time.timeScale = 1f;

        if (SceneTransition.Instance != null)
            SceneTransition.LoadScene(mainMenuSceneName);
        else
            SceneManager.LoadScene(mainMenuSceneName);

        isPaused = false;
        isTransitioning = false;
    }

    private void TriggerAnimation(string triggerName)
    {
        if (pauseAnimator == null || string.IsNullOrWhiteSpace(triggerName))
            return;

        pauseAnimator.ResetTrigger(openTrigger);
        pauseAnimator.ResetTrigger(closeTrigger);
        pauseAnimator.SetTrigger(triggerName);
    }

    private void WireButton(string childName, UnityEngine.Events.UnityAction action)
    {
        if (pauseMenuRoot == null)
            return;

        Transform buttonTransform = FindChildRecursive(pauseMenuRoot.transform, childName);
        if (buttonTransform == null)
            return;

        Button button = buttonTransform.GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(action);
    }

    private static Transform FindChildRecursive(Transform root, string childName)
    {
        if (root == null)
            return null;

        if (string.Equals(root.name, childName, System.StringComparison.OrdinalIgnoreCase))
            return root;

        foreach (Transform child in root)
        {
            Transform found = FindChildRecursive(child, childName);
            if (found != null)
                return found;
        }

        return null;
    }

    private bool CanPauseInCurrentScene()
    {
        return !string.Equals(SceneManager.GetActiveScene().name, mainMenuSceneName, System.StringComparison.OrdinalIgnoreCase);
    }
}