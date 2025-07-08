using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float lifeTime = 5f;  // fallback destroy if somehow stays on-screen forever
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        Destroy(gameObject, lifeTime); // safety destroy
    }

    void Update()
    {
        CheckIfOutOfCameraBounds();
    }

    private void CheckIfOutOfCameraBounds()
    {
        Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);

        if (viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 || viewportPos.y > 1)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            Debug.Log("Projectile hit the player!");
            Destroy(gameObject);

            // Example: if player has health script
            // collision.collider.GetComponent<PlayerHealth>()?.TakeDamage(1);
        }
        else
        {
            // Optional: destroy on anything else
            // Destroy(gameObject);
        }
    }
}
