using UnityEngine;

public class PlayerHitbox : MonoBehaviour
{
private void OnTriggerEnter2D(Collider2D other)
{
    Debug.Log(other.name);
    if (other.CompareTag("Enemy"))
        {
    Debug.Log(other.name+"Hello");
            BaseEnemy enemy = other.GetComponent<BaseEnemy>();
            if (enemy != null && CombatManager.Instance.IsCombatActive())
            {
    Debug.Log(other.name+ "HI");
                if (enemy.IsAlive())
                {
                    enemy.TakeDamage(1);
                    CameraShaker.Instance.Shake();
                    // CombatManager.Instance.UseCharge();

                    // TODO: play effects, score, etc.
                }
            }
        }
    
}
}
