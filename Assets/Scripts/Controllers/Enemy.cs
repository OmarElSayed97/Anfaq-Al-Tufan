// Enemy.cs
using UnityEngine;
using System;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 1;
    public float countdownDuration = 5f;
    public float attackRange = 5f;
    public int damage = 1;

    public KillFeedbackUI UI;

    private int currentHealth;
    private float countdownRemaining;
    private bool isCountingDown = false;
    private bool isDead = false;

    public event Action<Enemy> OnEnemyKilled;
    public event Action<Enemy> OnEnemyCountdownFinished;



    void Start()
    {
        currentHealth = maxHealth;
        countdownRemaining = countdownDuration;
    }

    void Update()
    {
        if (isCountingDown && !isDead)
        {
            countdownRemaining -= Time.deltaTime;
            if (countdownRemaining <= 0f)
            {
                countdownRemaining = 0f;
                isCountingDown = false;
                OnEnemyCountdownFinished?.Invoke(this);
                Debug.Log("Enemy fired!");
            }
        }
    }

    public void StartCountdown()
    {
        if (!isDead)
            isCountingDown = true;
    }

    public void PauseCountdown()
    {
        isCountingDown = false;
    }

    public void ResumeCountdown()
    {
        if (!isDead && countdownRemaining > 0f)
            isCountingDown = true;
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        isCountingDown = false;
        OnEnemyKilled?.Invoke(this);
        UI.PlayKillFeedback(150, transform.position);
        Debug.Log("Enemy died");
        transform.localScale = Vector2.zero; // or pool it later
    }

    public bool IsAlive() => !isDead;
    public float GetCountdownRemaining() => countdownRemaining;
    public int GetCurrentHealth() => currentHealth;
} 
