using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

public class TestScript : MonoBehaviour
{
    private WeaponController weaponController;
    private Camera mainCamera;
    
    private bool isLowResolution = true; 
    private bool isPaused = false;

    private void Start()
    {
        weaponController = FindFirstObjectByType<WeaponController>();
        mainCamera = Camera.main;
        
        
    }

    
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q)){
            
            isPaused = !isPaused;
            Time.timeScale = isPaused ? 0f : 1f;
            Debug.Log(isPaused ? "Game paused" : "Game resumed");
        }

        if(Input.GetKeyDown(KeyCode.V)){
            
            SceneTransition.LoadScene("TestScene");
        }

        if(Input.GetKeyDown(KeyCode.Z)){
            
            if (weaponController != null)
            {
                weaponController.AddShotgunAmmo(999);
                Debug.Log("Added 999 shotgun ammo!");
            }
        }

        
        
        
        
        
        
    }

    
    
    
    
    
    
    

    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
}
