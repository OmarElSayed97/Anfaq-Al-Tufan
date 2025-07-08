using UnityEngine;
using System;
using UnityEditor;

public class GamePhaseManager : MonoBehaviour
{
    public static GamePhaseManager Instance { get; private set; }

    [Header("Debug Info")]
    [SerializeField] private GamePhase currentPhaseDebug;
    [SerializeField] private bool isInputLockedDebug;

    public GamePhase CurrentPhase { get; private set; } = GamePhase.TunnelDrawing; // Default starting phase
    public bool IsInputLocked { get; private set; }

    public event Action<GamePhase> OnPhaseChanged; // (newPhase)

    private GamePhaseState currentState;

    [SerializeField] private TunnelDrawer tunnelDrawer;
    [SerializeField] private TunnelNavigator tunnelNavigator;
    [SerializeField] private CombatHandler combatHandler;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Initialize state based on starting phase
        currentState = CreateState(CurrentPhase);
        currentState?.OnEnter();
        OnPhaseChanged += HandlePhaseChanged;
        UpdateDebugFields();
    }

    private void Update()
    {
        currentState?.OnUpdate();
        UpdateDebugFields();
    }

    private void HandlePhaseChanged(GamePhase newPhase)
    {
        currentState?.OnExit();
        currentState = CreateState(newPhase);
        currentState?.OnEnter();
        UpdateDebugFields();
    }

    private GamePhaseState CreateState(GamePhase phase)
    {
        switch (phase)
        {
            case GamePhase.Idle: return new IdleState();
            case GamePhase.TunnelDrawing: return new TunnelDrawingState(tunnelDrawer);
            case GamePhase.TunnelNavigation: return new TunnelNavigationState(tunnelNavigator);
            case GamePhase.Combat: return new CombatState(combatHandler);
            default: return null;
        }
    }

    public void SetPhase(GamePhase newPhase)
    {
        if (newPhase == CurrentPhase)
            return;

        CurrentPhase = newPhase;
        Debug.Log($"[GamePhaseManager] Phase changed to: {newPhase}");
        OnPhaseChanged?.Invoke(newPhase);
        UpdateDebugFields();
    }

    public void SetInputLock(bool isLocked)
    {
        IsInputLocked = isLocked;
        Debug.Log($"[GamePhaseManager] Input Lock set to: {isLocked}");
        UpdateDebugFields();
    }

    public bool IsPhase(GamePhase phase) => CurrentPhase == phase;

    private void UpdateDebugFields()
    {
        currentPhaseDebug = CurrentPhase;
        isInputLockedDebug = IsInputLocked;
    }
}

// Helper attribute to make fields read-only in the inspector
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label);
        GUI.enabled = true;
    }
}
#endif

public class ReadOnlyAttribute : PropertyAttribute { }
