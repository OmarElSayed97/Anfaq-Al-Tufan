using UnityEngine;
using System;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public EnemyData data;
    public KillFeedbackUI UI;
    public GameObject player;

    private int currentHealth;
    private bool isDead = false;
    private EnemyState currentState = EnemyState.Idle;
    private Animator animator;
    private Coroutine flashCoroutine;
    private float patrolTimer;
    private float idleTimer;
    private int moveDirection = 1; // 1 = right, -1 = left

    public event Action<Enemy> OnEnemyKilled;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        currentHealth = data.maxHealth;
        SetAnimation(EnemyAnimationState.Idle);
        SetRandomPatrol();
    }

    private void Update()
    {
        if (isDead) return;
        HandleState();
    }

    private void HandleState()
    {
        bool playerAbove = false;
        if (player != null)
        {
            var pcm = player.GetComponent<PlayerContextManager>();
            if (pcm != null)
                playerAbove = pcm.CurrentLocation == PlayerLocation.AboveGround;
        }
        if (playerAbove && CombatManager.Instance.combatActive)
        {
            AimAtPlayer();
            currentState = EnemyState.Idle;
            SetAnimation(EnemyAnimationState.Idle);
            return;
        }
        if (currentState == EnemyState.Patrolling)
        {
            Patrol();
        }
        else if (currentState == EnemyState.Idle)
        {
            idleTimer -= Time.deltaTime;
            if (idleTimer <= 0f)
            {
                SetRandomPatrol();
            }
        }
        else if (currentState == EnemyState.Attacking)
        {
            PerformAttack();
        }
    }

    public void TakeDamage(int amount)
    {
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

    private IEnumerator FlashEffect()
    {
        GetComponent<SpriteRenderer>().material.SetInt("_Flash", 1);
        yield return new WaitForSeconds(0.2f);
        GetComponent<SpriteRenderer>().material.SetInt("_Flash", 0);
    }

    private void Die()
    {
        isDead = true;
        currentState = EnemyState.Dead;
        SetAnimation(EnemyAnimationState.Die);
        OnEnemyKilled?.Invoke(this);
        if (UI != null)
            UI.PlayKillFeedback(150, transform.position);
        transform.localScale = Vector3.zero; // Optionally scale down to zero
        GetComponent<Collider2D>().enabled = false; // Disable collider
    }

    public void Revive () {
        if (!isDead) return;
        isDead = false;
        currentHealth = data.maxHealth;
        SetAnimation(EnemyAnimationState.Idle);
        GetComponent<Collider2D>().enabled = true; // Re-enable collider
        transform.localScale = Vector3.one; // Reset scale
        SetRandomPatrol();
    }

    private void SetAnimation(EnemyAnimationState state)
    {
        animator?.SetTrigger(state.ToString());
    }

    private void UpdateSpriteDirection()
    {
        Vector3 scale = transform.localScale;
        scale.x = moveDirection < 0 ? -1 : 1;
        transform.localScale = scale;
    }

    private void SetRandomPatrol()
    {
        patrolTimer = UnityEngine.Random.Range(data.minPatrolDuration, data.maxPatrolDuration);
        moveDirection = UnityEngine.Random.value < 0.5f ? -1 : 1;
        currentState = EnemyState.Patrolling;
    }

    private void Patrol()
    {
        patrolTimer -= Time.deltaTime;
        Vector3 pos = transform.position;
        pos.x += moveDirection * data.patrolSpeed * Time.deltaTime;
        pos.x = Mathf.Clamp(pos.x, data.leftBound, data.rightBound);
        transform.position = pos;
        if (pos.x <= data.leftBound || pos.x >= data.rightBound)
        {
            moveDirection *= -1;
        }
        UpdateSpriteDirection();
        if (patrolTimer <= 0f)
        {
            SetIdle();
        }
    }

    private void SetIdle()
    {
        idleTimer = UnityEngine.Random.Range(data.minIdleTime, data.maxIdleTime);
        currentState = EnemyState.Idle;
        SetAnimation(EnemyAnimationState.Idle);
    }

    private void AimAtPlayer()
    {
        if (player == null) return;
        Vector3 toPlayer = player.transform.position - transform.position;
        moveDirection = toPlayer.x < 0 ? -1 : 1;
        UpdateSpriteDirection();
    }

    private void PerformAttack()
    {
        SetAnimation(EnemyAnimationState.Attack);
        if (data.attackType == EnemyAttackType.Ranged)
        {
            FireProjectileAtPlayer();
        }
        SetIdle();
    }

    private void FireProjectileAtPlayer()
    {
        if (data.projectilePrefab == null)
        {
            Debug.LogWarning("Projectile prefab not assigned!");
            return;
        }
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogWarning("Player not found in scene!");
            return;
        }
        Vector3 direction = (playerObj.transform.position - transform.position).normalized;
        GameObject proj = Instantiate(data.projectilePrefab, transform.position, Quaternion.identity);
        Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction * data.projectileSpeed;
        }
        else
        {
            proj.transform.right = direction;
        }
    }

    public bool IsAlive() => !isDead;
    public int GetCurrentHealth() => currentHealth;
    public EnemyType GetEnemyType() => data.enemyType;

    public void StartSlowMo () {
        // Implement slow motion start logic if needed
    }

    public void StopSlowMo () {
        // Implement slow motion stop logic if needed
    }
}
