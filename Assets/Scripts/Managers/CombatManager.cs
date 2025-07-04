using UnityEngine;
using System.Collections.Generic;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    [Header("Combat Settings")]
    public int maxCharges = 3;
    public float combatDuration = 10f;

    private List<BaseEnemy> activeEnemies = new List<BaseEnemy>();
    [SerializeField] private int currentCharges;
    private float timeRemaining;
    public bool combatActive = false;

    public delegate void CombatEvent();
    public event CombatEvent OnCombatStart;
    public event CombatEvent OnCombatEnd;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        if (!combatActive) return;

        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0f)
        {
            Debug.Log("Combat time ended.");
            EndCombat(); // time ran out, but not a fail
        }
    }

    public void StartCombat(List<BaseEnemy> enemies, int charges)
    {
        if (enemies == null || enemies.Count == 0)
        {
            Debug.LogWarning("Combat started with no enemies.");
            EndCombat();
            return;
        }

        activeEnemies = new List<BaseEnemy>(enemies);
        foreach (var enemy in activeEnemies)
        {
            enemy.OnEnemyKilled += HandleEnemyKilled;
            enemy.OnEnemyCountdownFinished += HandleEnemyFired;
            enemy.StartCountdown();
        }

        currentCharges = charges;
        timeRemaining = combatDuration;
        combatActive = true;
        GamePhaseManager.Instance.SetPhase(GamePhase.Combat);

        OnCombatStart?.Invoke();
        Debug.Log("Combat started.");
    }

    public void UseCharge()
    {
        if (!combatActive) return;

        currentCharges--;
        Debug.Log($"Charge used. Remaining: {currentCharges}");

        if (currentCharges <= 0)
        {
            Debug.Log("No charges left.");
            EndCombat();
        }
    }

    private void HandleEnemyKilled(BaseEnemy enemy)
    {
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
            enemy.OnEnemyKilled -= HandleEnemyKilled;
            enemy.OnEnemyCountdownFinished -= HandleEnemyFired;
        }
    }

    private void HandleEnemyFired(BaseEnemy enemy)
    {
        Debug.Log("Player hit by enemy!");
        EndCombatImmediate(); // this is a failure — enemy fired
    }

    public void PauseAllEnemyCountdowns()
    {
        foreach (var enemy in activeEnemies)
        {
            enemy.PauseCountdown();
        }
    }

    public void ResumeAllEnemyCountdowns()
    {
        foreach (var enemy in activeEnemies)
        {
            enemy.ResumeCountdown();
        }
    }

    public void EndCombat()
    {
        if (!combatActive) return;

        combatActive = false;
        foreach (var enemy in activeEnemies)
        {
            enemy.OnEnemyKilled -= HandleEnemyKilled;
            enemy.OnEnemyCountdownFinished -= HandleEnemyFired;
        }

        activeEnemies.Clear();
        GamePhaseManager.Instance.SetPhase(GamePhase.TunnelDrawing);

        OnCombatEnd?.Invoke();
        Debug.Log("Combat ended — returning to Tunnel Drawing phase.");
    }

    public void EndCombatImmediate()
    {
        if (!combatActive) return;

        combatActive = false;
        foreach (var enemy in activeEnemies)
        {
            enemy.OnEnemyKilled -= HandleEnemyKilled;
            enemy.OnEnemyCountdownFinished -= HandleEnemyFired;
        }

        activeEnemies.Clear();
        GamePhaseManager.Instance.SetPhase(GamePhase.Idle); // or GameOver phase if needed

        OnCombatEnd?.Invoke();
        Debug.Log("Combat ended due to failure.");
    }

    public int GetCurrentCharges() => currentCharges;
    public float GetTimeRemaining() => timeRemaining;
    public bool IsCombatActive() => combatActive;
}
