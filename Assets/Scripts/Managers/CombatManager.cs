using UnityEngine;
using System.Collections.Generic;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    [Header("Combat Settings")]
    public int maxCharges = 3;
    public float combatDuration = 10f;

    private List<BaseEnemy> activeEnemies = new List<BaseEnemy>();
    [Header("UI")]
    [SerializeField] private GameObject combatUI;
    [SerializeField] private TMPro.TextMeshProUGUI timerText;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.red;
    [SerializeField] private float warningThreshold = 3f;
    private Vector3 originalScale;
    private bool isShaking = false;
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float shakeAmount = 0.1f;
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
        timeRemaining = Mathf.Max(timeRemaining, 0f); // Clamp to zero

        UpdateCombatTimerUI(timeRemaining);
        if (timeRemaining <= 0f)
        {
            Debug.Log("Combat time ended.");
            EndCombatImmediate(); // time ran out, but not a fail
        }
    }
    private void UpdateCombatTimerUI(float time)
    {
        if (timerText == null) return;

        timerText.text = Mathf.CeilToInt(time).ToString();

        if (time < warningThreshold)
        {
            timerText.color = warningColor;
            if (!isShaking)
                StartCoroutine(ShakeTimer());
        }
        else
        {
            timerText.color = normalColor;
            timerText.rectTransform.localScale = originalScale;
            isShaking = false;
        }
    }
    private System.Collections.IEnumerator ShakeTimer()
    {
        isShaking = true;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            if (timerText == null) yield break;

            Vector3 randomOffset = Random.insideUnitSphere * shakeAmount;
            randomOffset.z = 0f; // UI text should not shake in Z
            timerText.rectTransform.localScale = originalScale + randomOffset;

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (timerText != null)
            timerText.rectTransform.localScale = originalScale;

        isShaking = false;
    }


    public void StartCombat(List<BaseEnemy> enemies, int charges)
    {
        if (timerText != null)
        {
            timerText.gameObject.SetActive(true);
            originalScale = timerText.rectTransform.localScale;
        }

        if (enemies == null || enemies.Count == 0)
        {
            Debug.LogWarning("Combat started with no enemies.");
            EndCombat();
            return;
        }

        activeEnemies = new List<BaseEnemy>(enemies);
        combatUI.SetActive(true);
        foreach (var enemy in activeEnemies)
        {
            enemy.OnEnemyKilled += HandleEnemyKilled;
            enemy.OnEnemyCountdownFinished += HandleEnemyFired;
            enemy.StartCountdown();
        }

        CurrentCharges = charges;
        timeRemaining = combatDuration;
        combatActive = true;
        GamePhaseManager.Instance.SetPhase(GamePhase.Combat);

        OnCombatStart?.Invoke();
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
            Invoke(nameof(EndCombat), 0.1f);
           // EndCombat();
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
        if (timerText != null)
            timerText.gameObject.SetActive(false);

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
