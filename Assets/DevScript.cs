using UnityEngine;
using UnityEngine.SceneManagement;

public class DevScript : MonoBehaviour
{
    [SerializeField] private GameObject devMenuCanvas;
    
    private WeaponController weaponController;
    
    // Level management
    private string[] levelNames = new string[]
    {
        "Assets/Scenes/Levels/World1/1-1.unity",
        "Assets/Scenes/Levels/World1/1-2.unity",
        "Assets/Scenes/Levels/World1/1-3.unity",
        "Assets/Scenes/Levels/World1/1-4.unity",
        "Assets/Scenes/Levels/World1/1-5.unity"
    };
    
    private int currentLevelIndex = 0;

    void Start()
    {
        // Don't destroy this object when loading new scenes
        DontDestroyOnLoad(gameObject);
        
        // Subscribe to scene load events
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // Find weapon controller in the scene
        FindWeaponController();
        
        // Set current level index based on active scene
        UpdateCurrentLevelIndex();
    }

    private void OnDestroy()
    {
        // Unsubscribe from scene load events
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Disable the dev menu canvas when a new scene loads
        if (devMenuCanvas != null)
        {
            devMenuCanvas.SetActive(false);
        }
        
        // Re-find weapon controller in the new scene
        FindWeaponController();
        
        // Update current level index
        UpdateCurrentLevelIndex();
    }

    void Update()
    {
        // R - Restart Current Level
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartLevel();
        }
        
        // N - Next Level (with looping)
        if (Input.GetKeyDown(KeyCode.N))
        {
            NextLevel();
        }
        
        // B - Previous Level (with looping)
        if (Input.GetKeyDown(KeyCode.B))
        {
            PreviousLevel();
        }
        
        // M - Open/Close Dev Menu
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (devMenuCanvas != null)
            {
                devMenuCanvas.SetActive(!devMenuCanvas.activeSelf);
            }
        }
        
        // I - Give 1000 Infinite Recoil Shots
        if (Input.GetKeyDown(KeyCode.I))
        {
            GiveInfiniteShots();
        }
    }

    private void RestartLevel()
    {
        Time.timeScale = 1f; // Ensure time is running
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void NextLevel()
    {
        currentLevelIndex++;
        
        // Loop back to first level if at the end
        if (currentLevelIndex >= levelNames.Length)
        {
            currentLevelIndex = 0;
        }
        
        LoadLevelByIndex();
    }

    private void PreviousLevel()
    {
        currentLevelIndex--;
        
        // Loop to last level if at the beginning
        if (currentLevelIndex < 0)
        {
            currentLevelIndex = levelNames.Length - 1;
        }
        
        LoadLevelByIndex();
    }

    private void LoadLevelByIndex()
    {
        Time.timeScale = 1f; // Ensure time is running
        string scenePath = levelNames[currentLevelIndex];
        SceneManager.LoadScene(scenePath);
    }

    private void GiveInfiniteShots()
    {
        // Find weapon controller if not already cached
        if (weaponController == null)
        {
            FindWeaponController();
        }
        
        if (weaponController != null)
        {
            weaponController.AddShotgunAmmo(1000);
            Debug.Log("Added 1000 shotgun ammo!");
        }
        else
        {
            Debug.LogWarning("Could not find WeaponController to give ammo!");
        }
    }

    private void FindWeaponController()
    {
        weaponController = FindFirstObjectByType<WeaponController>();
    }

    private void UpdateCurrentLevelIndex()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        
        // Match scene name to our level array
        for (int i = 0; i < levelNames.Length; i++)
        {
            if (levelNames[i].Contains(sceneName))
            {
                currentLevelIndex = i;
                return;
            }
        }
    }

    private string GetCurrentLevelName()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        return string.IsNullOrEmpty(sceneName) ? "Unknown" : sceneName;
    }
}
