using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(Animator))]
public abstract class BaseEnemy : MonoBehaviour
{
    [Header("Enemy Identity")]
    public EnemyType enemyType = EnemyType.Normal;
    public EnemyAttackType attackType = EnemyAttackType.Melee;

    [Header("Stats")]
    public int maxHealth = 1;

    [Header("Runtime")]
    public KillFeedbackUI UI;
    private Coroutine flashCoroutine;

    protected int currentHealth;
    protected bool isDead = false;
    protected EnemyState currentState = EnemyState.Idle;

    protected Animator animator;

    public event Action<BaseEnemy> OnEnemyKilled;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
    }

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        SetAnimation(EnemyAnimationState.Idle);
    }

    protected virtual void Update()
    {
        if (isDead) return;

        HandleState();
    }

    protected virtual void HandleState()
    {
        // To be overridden in subclasses for AI behavior
    }

    public virtual void TakeDamage(int amount)
    {
        Debug.Log("Enemy Take Damage");
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashEffect());
        if (isDead) return;

        currentHealth -= amount;
        SetAnimation(EnemyAnimationState.Hit);

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    public IEnumerator FlashEffect()
    {
        // Example flash effect
        Debug.Log("Enemy flash effect triggered");
        GetComponent<SpriteRenderer>().material.SetInt("_Flash", 1);
        yield return new WaitForSeconds(0.2f);
        GetComponent<SpriteRenderer>().material.SetInt("_Flash", 0);
    }
    protected virtual void Die()
    {
        isDead = true;
        currentState = EnemyState.Dead;
        SetAnimation(EnemyAnimationState.Die);

        OnEnemyKilled?.Invoke(this);
        Debug.Log($"{name} died");

        transform.localScale = Vector3.zero; // Placeholder — use pooling later
        UI.PlayKillFeedback(150, transform.position);
    }

    protected void SetAnimation(EnemyAnimationState state)
    {
        animator?.SetTrigger(state.ToString());
    }

    public bool IsAlive() => !isDead;
    public int GetCurrentHealth() => currentHealth;
    public EnemyType GetEnemyType() => enemyType;
}
