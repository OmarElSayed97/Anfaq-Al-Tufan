using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    [Header("Combat Settings")]
    public int maxCharges = 3;
    public float combatDuration = 10f;

    private List<BaseEnemy> activeEnemies = new List<BaseEnemy>();

    [SerializeField] private int currentCharges;
    public int CurrentCharges
    {
        get => currentCharges;
        set
        {
            currentCharges = value;
            OnChargesChanged?.Invoke(currentCharges);
        }
    }
    public event System.Action<int> OnChargesChanged;
    private float timeRemaining;
    public bool combatActive = false;

    // General combat events
    public event System.Action<float> OnTimerChanged;
    public event System.Action<bool> OnCombatStateChanged;

    [Header("Combat State Settings")]
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float shakeAmount = 0.1f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        if (!combatActive) return;

        timeRemaining -= Time.deltaTime;
        timeRemaining = Mathf.Max(timeRemaining, 0f); // Clamp to zero

        // General event for timer change
        OnTimerChanged?.Invoke(timeRemaining);

        if (timeRemaining <= 0f)
        {
            Debug.Log("Combat time ended.");
            EndCombatImmediate(); // time ran out, but not a fail
        }
    }

    public void StartCombat(int charges)
    {
        // General event: combat state changed (active)
        OnCombatStateChanged?.Invoke(true);

        var enemies = FindObjectsByType<BaseEnemy>(FindObjectsSortMode.None).ToList();

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
        }

        CurrentCharges = charges;
        timeRemaining = combatDuration;
        combatActive = true;
        OnTimerChanged?.Invoke(timeRemaining);
        GamePhaseManager.Instance.SetPhase(GamePhase.Combat);

        Debug.Log("Combat started.");
    }

    public void UseCharge()
    {
        if (!combatActive) return;

        CurrentCharges--;
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
        }

        if (activeEnemies.Count == 0)
        {
            Debug.Log("All enemies defeated! Player wins!");
            EndCombat(true);
        }
    }

    public void EndCombat(bool won = false)
    {
        if (!combatActive) return;
        // General event: combat state changed (inactive)
        OnCombatStateChanged?.Invoke(false);

        combatActive = false;
        foreach (var enemy in activeEnemies)
        {
            enemy.OnEnemyKilled -= HandleEnemyKilled;
        }

        activeEnemies.Clear();
        if (!won) {
            Debug.Log("Combat ended without victory.");
            GamePhaseManager.Instance.SetPhase(GamePhase.TunnelDrawing);
        } else
            GamePhaseManager.Instance.SetPhase(GamePhase.GameWinState);

        Debug.Log("Combat ended — returning to Tunnel Drawing phase.");
    }

    public void EndCombatImmediate()
    {
        if (!combatActive) return;
        combatActive = false;
        foreach (var enemy in activeEnemies)
        {
            enemy.OnEnemyKilled -= HandleEnemyKilled;
        }

        activeEnemies.Clear();
        OnCombatStateChanged?.Invoke(false);
        GamePhaseManager.Instance.SetPhase(GamePhase.Idle); // or GameOver phase if needed
        
        Debug.Log("Combat ended due to failure.");
    }

    public int GetCurrentCharges() => currentCharges;
    public float GetTimeRemaining() => timeRemaining;
    public bool IsCombatActive() => combatActive;
}
