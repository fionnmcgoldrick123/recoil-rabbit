using System.Collections;
using UnityEngine;

public class CannonLauncher : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform canonObject;
    [SerializeField] private Transform launchPosition;

    [Header("Angle Clamp (degrees, 0 = right, 90 = up, 180 = left)")]
    [SerializeField] private float minAngle = 5f;
    [SerializeField] private float maxAngle = 175f;

    [Header("Pop Animation")]
    [SerializeField] private float popScaleMultiplier = 1.3f;
    [SerializeField] private float popDuration = 0.08f;
    [SerializeField] private float popReturnDuration = 0.12f;

    [Header("Launch")]
    [SerializeField] private float launchPower = 25f;
    [SerializeField] private float launchDelay = 0.15f;

    
    private enum CannonState { Idle, Aiming, Locked, Fired }
    private CannonState state = CannonState.Idle;

    private PlayerController playerInCannon;
    private Vector3 canonOriginalScale;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;

        if (canonObject != null)
            canonOriginalScale = canonObject.localScale;
    }

    private void Update()
    {
        if (state == CannonState.Aiming)
        {
            AimAtMouse();

            if (Input.GetMouseButtonDown(0))
            {
                StartCoroutine(FireSequence());
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (state != CannonState.Idle) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null || player.IsDead) return;

        playerInCannon = player;
        playerInCannon.EnterCannon();

        StartCoroutine(PopAndAim());
    }

    private void AimAtMouse()
    {
        if (canonObject == null) return;

        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Vector2 dirToMouse = (mouseWorld - canonObject.position).normalized;
        float angle = Mathf.Atan2(dirToMouse.y, dirToMouse.x) * Mathf.Rad2Deg;

        
        if (angle < 0f) angle += 360f;

        
        if (angle > 180f)
        {
            float distToMin = Mathf.Abs(Mathf.DeltaAngle(angle, minAngle));
            float distToMax = Mathf.Abs(Mathf.DeltaAngle(angle, maxAngle));
            angle = distToMin < distToMax ? minAngle : maxAngle;
        }
        else
        {
            angle = Mathf.Clamp(angle, minAngle, maxAngle);
        }

        
        Vector3 clampedDir = new Vector3(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            Mathf.Sin(angle * Mathf.Deg2Rad),
            0f
        );

        
        
        Vector3 canonDefaultTip = Vector3.up;
        Quaternion rotation = Quaternion.FromToRotation(canonDefaultTip, clampedDir);
        canonObject.rotation = rotation;
    }

    private IEnumerator PopAndAim()
    {
        state = CannonState.Aiming; 

        if (canonObject != null)
        {
            
            float t = 0f;
            Vector3 targetScale = canonOriginalScale * popScaleMultiplier;
            while (t < 1f)
            {
                t += Time.deltaTime / popDuration;
                canonObject.localScale = Vector3.Lerp(canonOriginalScale, targetScale, t);
                yield return null;
            }

            
            t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / popReturnDuration;
                canonObject.localScale = Vector3.Lerp(targetScale, canonOriginalScale, t);
                yield return null;
            }

            canonObject.localScale = canonOriginalScale;
        }
    }

    private IEnumerator FireSequence()
    {
        state = CannonState.Locked;

        yield return new WaitForSeconds(launchDelay);

        
        
        Vector2 launchDir = canonObject != null
            ? (Vector2)(canonObject.rotation * Vector3.up)
            : Vector2.up;

        if (playerInCannon != null)
        {
            Vector3 shootPos = launchPosition != null
                ? launchPosition.position
                : (canonObject != null ? canonObject.position : transform.position);

            playerInCannon.LaunchFromCannon(launchDir, launchPower, shootPos);
            playerInCannon = null;
        }

        state = CannonState.Fired;

        
        yield return new WaitForSeconds(0.5f);
        state = CannonState.Idle;
    }

    private void OnDrawGizmosSelected()
    {
        if (canonObject == null) return;

        Gizmos.color = Color.yellow;
        
        
        Vector3 tipDir = canonObject.rotation * Vector3.up;
        Gizmos.DrawRay(canonObject.position, tipDir * 2f);
        
        
        Vector3 minDir = new Vector3(
            Mathf.Cos(minAngle * Mathf.Deg2Rad),
            Mathf.Sin(minAngle * Mathf.Deg2Rad),
            0f
        );
        Vector3 maxDir = new Vector3(
            Mathf.Cos(maxAngle * Mathf.Deg2Rad),
            Mathf.Sin(maxAngle * Mathf.Deg2Rad),
            0f
        );
        Gizmos.color = Color.green;
        Gizmos.DrawRay(canonObject.position, minDir * 2f);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(canonObject.position, maxDir * 2f);
    }
}
