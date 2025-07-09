using UnityEngine;

public enum EnemyType { Normal, Advanced, Elite, Boss }
public enum EnemyAttackType { Melee, Ranged, AoE, SuicideBomber }
public enum EnemyState { Idle, Patrolling, Attacking, Dead }
public enum EnemyAnimationState { Idle, Walk, Attack, Hit, Die }

[CreateAssetMenu(fileName = "EnemyData", menuName = "ScriptableObjects/EnemyData", order = 1)]
public class EnemyData : ScriptableObject
{
    [Header("Enemy Identity")]
    public EnemyType enemyType = EnemyType.Normal;
    public EnemyAttackType attackType = EnemyAttackType.Melee;

    [Header("Stats")]
    public int maxHealth = 1;
    public float timePressure = 3;

    [Header("Patrol Settings")]
    public float patrolSpeed = 2f;
    public float minPatrolDuration = 1f;
    public float maxPatrolDuration = 3f;
    public float minIdleTime = 1f;
    public float maxIdleTime = 2f;
    public float leftBound = -5f;
    public float rightBound = 5f;

    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 5f;
}
