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
    public float countdownDuration = 5f;
    public float attackRange = 3f;
    public int damage = 1;

    [Header("Runtime")]
    public KillFeedbackUI UI;

    protected int currentHealth;
    protected float countdownRemaining;
    protected bool isCountingDown = false;
    protected bool isDead = false;
    protected EnemyState currentState = EnemyState.Idle;

    protected Animator animator;

    public event Action<BaseEnemy> OnEnemyKilled;
    public event Action<BaseEnemy> OnEnemyCountdownFinished;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
    }

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        countdownRemaining = countdownDuration;
        SetAnimation(EnemyAnimationState.Idle);
    }

    protected virtual void Update()
    {
        if (isDead) return;

        HandleState();
        HandleCountdown();
    }

    protected virtual void HandleState()
    {
        // To be overridden in subclasses for AI behavior
    }

    protected virtual void HandleCountdown()
    {
        if (isCountingDown && !isDead)
        {
            countdownRemaining -= Time.deltaTime;
            if (countdownRemaining <= 0f)
            {
                countdownRemaining = 0f;
                isCountingDown = false;
                OnCountdownFinished();
            }
        }
    }

    public virtual void StartCountdown() => isCountingDown = !isDead;
    public virtual void PauseCountdown() => isCountingDown = false;
    public virtual void ResumeCountdown()
    {
        if (!isDead && countdownRemaining > 0f)
            isCountingDown = true;
    }

    public virtual void TakeDamage(int amount)
    {
        Debug.Log("Enemy Take Damage");   
        StartCoroutine(FlashEffect());
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
        isCountingDown = false;
        currentState = EnemyState.Dead;
        SetAnimation(EnemyAnimationState.Die);

        OnEnemyKilled?.Invoke(this);
        Debug.Log($"{name} died");

        transform.localScale = Vector3.zero; // Placeholder — use pooling later
        UI.PlayKillFeedback(150, transform.position);
    }

    protected virtual void OnCountdownFinished()
    {
        OnEnemyCountdownFinished?.Invoke(this);
        Debug.Log($"{name} finished countdown and is attacking!");
    }

    protected void SetAnimation(EnemyAnimationState state)
    {
        animator?.SetTrigger(state.ToString());
    }

    public bool IsAlive() => !isDead;
    public float GetCountdownRemaining() => countdownRemaining;
    public int GetCurrentHealth() => currentHealth;
    public EnemyType GetEnemyType() => enemyType;
}
