using UnityEngine;

public class NormalEnemy : BaseEnemy
{
    protected override void Start()
    {
        base.Start();

        // You can override default enum values here if needed
        enemyType = EnemyType.Normal;
        attackType = EnemyAttackType.Ranged;
    }

    protected override void HandleState()
    {
        switch (currentState)
        {
            case EnemyState.Idle:
                // Idle behavior or animation
                break;

            case EnemyState.Attacking:
                PerformAttack();
                break;

            case EnemyState.Dead:
                // Do nothing
                break;
        }
    }

    protected override void OnCountdownFinished()
    {
        base.OnCountdownFinished();
        currentState = EnemyState.Attacking;
    }

    private void PerformAttack()
    {
        SetAnimation(EnemyAnimationState.Attack);

        Debug.Log("NormalEnemy performs ranged attack!");

        // Reset
        currentState = EnemyState.Idle;
        countdownRemaining = countdownDuration;
    }
}
