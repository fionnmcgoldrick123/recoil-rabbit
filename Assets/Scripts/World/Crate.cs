using UnityEngine;
using System.Collections.Generic;

public class Crate : MonoBehaviour
{
    [Header("Shells")]
    [SerializeField] private GameObject shotgunShellPrefab;
    [SerializeField] private int shellCount = 3;

    [Header("Piece Physics")]
    [SerializeField] private float pieceExplosionForce = 6f;
    [SerializeField] private float pieceTorqueRange = 250f;
    [SerializeField] private float pieceGravityScale = 4f;
    [SerializeField] private float pieceDestroyDelay = 4f;

    private bool broken = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (broken) return;
        if (other.GetComponent<Bullet>() == null) return;

        Break();
    }

    private void Break()
    {
        broken = true;

        // Collect children first to avoid modifying the collection mid-iteration
        List<Transform> pieces = new List<Transform>();
        foreach (Transform child in transform)
            pieces.Add(child);

        foreach (Transform piece in pieces)
        {
            piece.SetParent(null);

            Rigidbody2D rb = piece.GetComponent<Rigidbody2D>();
            if (rb == null)
                rb = piece.gameObject.AddComponent<Rigidbody2D>();

            rb.gravityScale = pieceGravityScale;

            Vector2 dir = (Vector2)(piece.position - transform.position);
            if (dir.sqrMagnitude < 0.001f)
                dir = Random.insideUnitCircle;
            dir = dir.normalized;

            float force = Random.Range(pieceExplosionForce * 0.6f, pieceExplosionForce * 1.4f);
            rb.linearVelocity = dir * force;
            rb.angularVelocity = Random.Range(-pieceTorqueRange, pieceTorqueRange);

            Destroy(piece.gameObject, pieceDestroyDelay);
        }

        // Spawn shotgun shells
        if (shotgunShellPrefab != null)
        {
            for (int i = 0; i < shellCount; i++)
            {
                Vector2 spawnPos = (Vector2)transform.position + Random.insideUnitCircle * 0.15f;
                GameObject shell = Instantiate(shotgunShellPrefab, spawnPos, Quaternion.identity);
                ShotgunShellPickup pickup = shell.GetComponent<ShotgunShellPickup>();
                if (pickup != null)
                    pickup.Launch(Random.insideUnitCircle.normalized);
            }
        }

        Destroy(gameObject);
    }
}
