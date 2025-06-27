using UnityEngine;

public class PlayerContextManager : MonoBehaviour
{
    public Transform playerTransform;

    public PlayerLocation CurrentLocation 
    {
        get => playerTransform.position.y < 0 ? 
                    PlayerLocation.Underground : 
                    PlayerLocation.AboveGround; 
    }

    public float Depth
    {
        get => Mathf.Abs(Mathf.Min(0, playerTransform.position.y));
    }

    public AnimationState CurrentAnimationState { get; private set; } = AnimationState.None;

    public bool IsAnimating => CurrentAnimationState != AnimationState.None;

    public void SetAnimationState(AnimationState newState)
    {
        CurrentAnimationState = newState;
        // Optional: trigger animation logic or events
    }

    

    private void Update()
    {
        // Debug helper (optional)
        Debug.DrawLine(Vector3.zero, new Vector3(0, playerTransform.position.y, 0), Color.cyan);
    }
}
