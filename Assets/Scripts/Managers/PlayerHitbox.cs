using UnityEngine;

public class PlayerHitbox : MonoBehaviour
{
private void OnTriggerEnter2D(Collider2D other)
{
        Debug.Log(other.name);
    if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null && CombatManager.Instance.IsCombatActive())
            {
                if (enemy.IsAlive())
                {
                    enemy.TakeDamage(1);
                    CameraShaker.Instance.Shake();
                    CombatManager.Instance.UseCharge();

                    // TODO: play effects, score, etc.
                }
            }
        }
    
}
}
