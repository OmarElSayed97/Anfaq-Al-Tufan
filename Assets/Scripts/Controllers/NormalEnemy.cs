using UnityEngine;

public class NormalEnemy : BaseEnemy
{
    [Header("Patrol Settings")]
    public float patrolSpeed = 2f;
    public float minPatrolDuration = 1f;
    public float maxPatrolDuration = 3f;
    public float minIdleTime = 1f;
    public float maxIdleTime = 2f;

    public float leftBound = -5f;
    public float rightBound = 5f;

    private float patrolTimer;
    private float idleTimer;
    private int moveDirection = 1; // 1 = right, -1 = left

    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 5f;

    [Header("Player Reference")]
    public GameObject player;

    protected override void Start()
    {
        base.Start();
        SetRandomPatrol();
        // StartCountdown(); // Removed: shooting timer handled by CombatManager
    }

    protected override void HandleState()
    {
        // Prevent patrolling while in combat and player is above ground
        bool playerAbove = false;
        if (player != null)
        {
            var pcm = player.GetComponent<PlayerContextManager>();
            if (pcm != null)
                playerAbove = pcm.CurrentLocation == PlayerLocation.AboveGround;
        }
        if (playerAbove)
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

    private void UpdateSpriteDirection()
    {
        Vector3 scale = transform.localScale;
        scale.x = moveDirection < 0 ? -1 : 1;
        transform.localScale = scale;
    }

    private void SetRandomPatrol()
    {
        patrolTimer = Random.Range(minPatrolDuration, maxPatrolDuration);
        moveDirection = Random.value < 0.5f ? -1 : 1;
        currentState = EnemyState.Patrolling;
        //SetAnimation(EnemyAnimationState.Walk);
    }

    private void Patrol()
    {
        patrolTimer -= Time.deltaTime;

        // Move
        Vector3 pos = transform.position;
        pos.x += moveDirection * patrolSpeed * Time.deltaTime;
        // Clamp within bounds
        pos.x = Mathf.Clamp(pos.x, leftBound, rightBound);

        transform.position = pos;

        // If hit boundary, flip direction
        if (pos.x <= leftBound || pos.x >= rightBound)
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
        idleTimer = Random.Range(minIdleTime, maxIdleTime);
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
        Debug.Log("NormalEnemy performs attack!");

        if (attackType == EnemyAttackType.Ranged)
        {
            FireProjectileAtPlayer();
        }

        // After attacking, go back to idle then resume patrol
        SetIdle();
        // countdownRemaining = countdownDuration; // Removed
        // StartCountdown(); // Removed
    }

    private void FireProjectileAtPlayer()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("Projectile prefab not assigned!");
            return;
        }

        // Get player position at this moment
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("Player not found in scene!");
            return;
        }

        Vector3 direction = (player.transform.position - transform.position).normalized;

        // Spawn projectile
        GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        Debug.Log("Projectile Spawned");
        // Give it velocity or set direction
        Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction * projectileSpeed;
        }
        else
        {
            // If no Rigidbody2D, fallback to moving manually
            proj.transform.right = direction;
        }
    }

}
