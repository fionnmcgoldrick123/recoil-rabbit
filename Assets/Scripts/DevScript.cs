using UnityEngine;
using UnityEngine.SceneManagement;

public class DevScript : MonoBehaviour
{
    [SerializeField] private GameObject devMenuCanvas;
    
    private WeaponController weaponController;
    
    
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
        
        DontDestroyOnLoad(gameObject);
        
        
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        
        FindWeaponController();
        
        
        UpdateCurrentLevelIndex();
    }

    private void OnDestroy()
    {
        
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        
        if (devMenuCanvas != null)
        {
            devMenuCanvas.SetActive(false);
        }
        
        
        FindWeaponController();
        
        
        UpdateCurrentLevelIndex();
    }

    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartLevel();
        }
        
        
        if (Input.GetKeyDown(KeyCode.N))
        {
            NextLevel();
        }
        
        
        if (Input.GetKeyDown(KeyCode.B))
        {
            PreviousLevel();
        }
        
        
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (devMenuCanvas != null)
            {
                devMenuCanvas.SetActive(!devMenuCanvas.activeSelf);
            }
        }
        
        
        if (Input.GetKeyDown(KeyCode.I))
        {
            GiveInfiniteShots();
        }
    }

    private void RestartLevel()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void NextLevel()
    {
        currentLevelIndex++;
        
        
        if (currentLevelIndex >= levelNames.Length)
        {
            currentLevelIndex = 0;
        }
        
        LoadLevelByIndex();
    }

    private void PreviousLevel()
    {
        currentLevelIndex--;
        
        
        if (currentLevelIndex < 0)
        {
            currentLevelIndex = levelNames.Length - 1;
        }
        
        LoadLevelByIndex();
    }

    private void LoadLevelByIndex()
    {
        Time.timeScale = 1f; 
        string scenePath = levelNames[currentLevelIndex];
        SceneManager.LoadScene(scenePath);
    }

    private void GiveInfiniteShots()
    {
        
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
