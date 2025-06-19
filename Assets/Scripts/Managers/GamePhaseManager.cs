// GamePhaseManager.cs
using UnityEngine;
using System;

public class GamePhaseManager : MonoBehaviour
{
    public static GamePhaseManager Instance { get; private set; }

    public GamePhase CurrentPhase { get; private set; } = GamePhase.Idle;
    public bool IsInputLocked { get; private set; }

    public event Action<GamePhase> OnPhaseChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetPhase(GamePhase newPhase)
    {
        if (newPhase == CurrentPhase)
            return;

        CurrentPhase = newPhase;
        OnPhaseChanged?.Invoke(newPhase);
    }

    public void SetInputLock(bool isLocked)
    {
        IsInputLocked = isLocked;
        Debug.Log($"Input Lock set to: {isLocked}");
    }

    public bool IsPhase(GamePhase phase) => CurrentPhase == phase;
} 
