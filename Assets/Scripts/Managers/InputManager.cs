using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    // Example events for various input types
    // public Vector2 swipeActionPosition;
    public event Action<Vector2> swipePressStarted;
    public event Action<Vector2> swipePressEnded;

    public Vector2 lastSwipePos;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void OnSwipePos(InputValue value)
    {
        lastSwipePos = value.Get<Vector2>();
        // Debug.Log($"[InputManager] OnSwipePos updated lastSwipePos: {lastSwipePos}");
    }

    public void OnSwipePress(InputValue value)
    {
        float pressed = value.Get<float>();
        // Debug.LogWarning($"[InputManager] OnSwipePress called. Pressed: {pressed}, lastSwipePos: {lastSwipePos}");
        if (pressed > 0.5f)
        {
            Debug.LogWarning($"[InputManager] swipePress Started invoked at {lastSwipePos}");
            swipePressStarted?.Invoke(lastSwipePos);
        }
        else
        {
            Debug.LogWarning($"[InputManager] swipePress Ended invoked at {lastSwipePos}");
            swipePressEnded?.Invoke(lastSwipePos);
        }
    }
}
