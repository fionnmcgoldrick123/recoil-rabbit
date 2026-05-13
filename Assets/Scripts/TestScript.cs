using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

public class TestScript : MonoBehaviour
{
    private WeaponController weaponController;
    private Camera mainCamera;
    // private PixelPerfectCamera pixelPerfectCamera;
    private bool isLowResolution = true; // Start at 320x180
    private bool isPaused = false;

    private void Start()
    {
        weaponController = FindFirstObjectByType<WeaponController>();
        mainCamera = Camera.main;
        // pixelPerfectCamera = mainCamera.GetComponent<PixelPerfectCamera>();
        // SetCameraResolution(isLowResolution);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q)){
            // toggle pause
            isPaused = !isPaused;
            Time.timeScale = isPaused ? 0f : 1f;
            Debug.Log(isPaused ? "Game paused" : "Game resumed");
        }

        if(Input.GetKeyDown(KeyCode.V)){
            // restart scene called TestScene
            SceneTransition.LoadScene("TestScene");
        }

        if(Input.GetKeyDown(KeyCode.Z)){
            // infinite ammo
            if (weaponController != null)
            {
                weaponController.AddShotgunAmmo(999);
                Debug.Log("Added 999 shotgun ammo!");
            }
        }

        // if(Input.GetKeyDown(KeyCode.P)){
        //     // toggle camera resolution
        //     isLowResolution = !isLowResolution;
        //     // SetCameraResolution(isLowResolution);
        //     Debug.Log($"Camera resolution switched to {(isLowResolution ? "320x180" : "640x360")}");
        // }
    }

    // private void SetCameraResolution(bool lowRes)
    // {
    //     if (pixelPerfectCamera == null) 
    //     {
    //         Debug.LogError("PixelPerfectCamera component not found on main camera!");
    //         return;
    //     }

    //     if (lowRes)
    //     {
    //         // 320x180 reference resolution
    //         pixelPerfectCamera.refResolutionX = 320;
    //         pixelPerfectCamera.refResolutionY = 180;
    //         Debug.Log("Resolution set to 320x180");
    //     }
    //     else
    //     {
    //         // 640x360 reference resolution
    //         pixelPerfectCamera.refResolutionX = 640;
    //         pixelPerfectCamera.refResolutionY = 360;
    //         Debug.Log("Resolution set to 640x360");
    //     }
    // }
}
