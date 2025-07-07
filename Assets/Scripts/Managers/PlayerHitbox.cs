using UnityEngine;

public class PlayerHitbox : MonoBehaviour
{
    public bool isActive = true;
    private void OnTriggerStay2D(Collider2D other)
    {
        // Debug.Log(other.name);
        if (other.CompareTag("Enemy"))
            {
                // Debug.Log(other.name+"Hello");
                BaseEnemy enemy = other.GetComponent<BaseEnemy>();
                if (enemy != null && GetComponent<PlayerContextManager>().CurrentAnimationState == AnimationState.Attacking)
                {
                    // Debug.Log(other.name+ "HI");
                    if (enemy.IsAlive() && isActive)
                    {
                        enemy.TakeDamage(1);
                        CameraShaker.Instance.Shake();
                        isActive = false;
                        
                        // CombatManager.Instance.UseCharge();

                        // TODO: play effects, score, etc.
                    }
                }
            }   
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy")) {
            isActive = true;
        }   
    }
}
