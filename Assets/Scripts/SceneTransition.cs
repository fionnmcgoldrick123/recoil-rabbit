using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SceneTransition : MonoBehaviour
{
    private const string StartTrigger = "Start";
    private const string EndTrigger = "End";

    [System.Serializable]
    private class LevelLabelEntry
    {
        public string sceneName;
        public string levelText;
    }

    public static SceneTransition Instance;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private TMP_Text levelText;

    [Header("Level Labels")]
    [SerializeField] private string labelPrefix = "WORLD";
    [SerializeField] private List<LevelLabelEntry> levelLabels = new List<LevelLabelEntry>();

    private bool isTransitioning;
    private string pendingSceneName;
    private int pendingBuildIndex = -1;
    private Coroutine endRoutine;
    private Coroutine startRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (animator == null)
            animator = GetComponent<Animator>();

        if (levelText == null)
            levelText = GetComponentInChildren<TMP_Text>(true);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        RunTimerManager.ResetTimer();
        RefreshLevelText(SceneManager.GetActiveScene().name);
        BeginStartTransition();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        RunTimerManager.ResetTimer();
        RefreshLevelText(scene.name);
        BeginStartTransition();
    }

    public static void LoadScene(string sceneName)
    {
        if (Instance == null)
        {
            SceneManager.LoadScene(sceneName);
            return;
        }

        Instance.BeginSceneLoad(sceneName, -1);
    }

    public static void LoadScene(int buildIndex)
    {
        if (Instance == null)
        {
            SceneManager.LoadScene(buildIndex);
            return;
        }

        Instance.BeginSceneLoad(null, buildIndex);
    }

    public static void ReloadActiveScene()
    {
        Scene active = SceneManager.GetActiveScene();
        if (!string.IsNullOrEmpty(active.name))
            LoadScene(active.name);
        else
            LoadScene(active.buildIndex);
    }

    private void BeginSceneLoad(string sceneName, int buildIndex)
    {
        if (isTransitioning)
            return;

        isTransitioning = true;
        pendingSceneName = sceneName;
        pendingBuildIndex = buildIndex;

        if (animator == null)
        {
            ExecuteLoad();
            return;
        }

        animator.ResetTrigger(StartTrigger);
        animator.SetTrigger(EndTrigger);

        if (endRoutine != null)
            StopCoroutine(endRoutine);
        endRoutine = StartCoroutine(WaitForEndThenLoad());
    }

    public void OnEndTransitionComplete()
    {
        if (endRoutine != null)
        {
            StopCoroutine(endRoutine);
            endRoutine = null;
        }

        ExecuteLoad();
    }

    private IEnumerator WaitForEndThenLoad()
    {
        float elapsed = 0f;
        float maxWait = 1f;

        while (elapsed < maxWait)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        endRoutine = null;
        ExecuteLoad();
    }

    private void BeginStartTransition()
    {
        if (startRoutine != null)
            StopCoroutine(startRoutine);
        startRoutine = StartCoroutine(PlayStartNextFrame());
    }

    private IEnumerator PlayStartNextFrame()
    {
        yield return null;
        startRoutine = null;

        if (animator == null)
        {
            isTransitioning = false;
            yield break;
        }

        animator.Rebind();
        animator.Update(0f);
        animator.ResetTrigger(EndTrigger);
        animator.SetTrigger(StartTrigger);

        isTransitioning = false;
    }

    private void RefreshLevelText(string sceneName)
    {
        if (levelText == null)
            return;

        levelText.text = GetLevelLabel(sceneName);
    }

    private string GetLevelLabel(string sceneName)
    {
        string levelValue = sceneName;

        for (int i = 0; i < levelLabels.Count; i++)
        {
            LevelLabelEntry entry = levelLabels[i];
            if (entry != null && entry.sceneName == sceneName && !string.IsNullOrWhiteSpace(entry.levelText))
            {
                levelValue = entry.levelText;
                break;
            }
        }

        if (string.IsNullOrWhiteSpace(labelPrefix))
            return levelValue;

        return $"{labelPrefix}\n{levelValue}";
    }

    private void ExecuteLoad()
    {
        if (!string.IsNullOrEmpty(pendingSceneName))
        {
            string sceneName = pendingSceneName;
            pendingSceneName = null;
            pendingBuildIndex = -1;
            SceneManager.LoadScene(sceneName);
            return;
        }

        if (pendingBuildIndex >= 0)
        {
            int idx = pendingBuildIndex;
            pendingBuildIndex = -1;
            SceneManager.LoadScene(idx);
            return;
        }

        isTransitioning = false;
    }
}