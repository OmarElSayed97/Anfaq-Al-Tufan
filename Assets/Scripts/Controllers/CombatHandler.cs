using UnityEngine;
using DG.Tweening;

public class CombatHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    public PlayerContextManager playerContext;
    [SerializeField] private LineRenderer slashLine;
    [SerializeField] private TrailRenderer dashTrail;
    [SerializeField] private float minSwipeDistance = 0.5f;
    [SerializeField] private float slashRadius = 4f;
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private LayerMask enemyLayer;

    private Camera mainCam;
    private Vector2 swipeStartScreen;
    private bool isSwiping = false;

    void Awake()
    {
        mainCam = Camera.main;
        if (dashTrail != null) dashTrail.enabled = false;
    }

    // Register for swipe input events
    public void OnEnter()
    {
        InputManager.Instance.swipePressStarted += OnSwipePressStarted;
        InputManager.Instance.swipePressEnded += OnSwipePressEnded;
    }

    // Maintain player position if underground
    public void OnUpdate()
    {
        if (playerContext.CurrentLocation == PlayerLocation.Underground) {
            playerContext.transform.position = new Vector3(playerContext.transform.position.x, 0.1f, playerContext.transform.position.z);
        }
    }

    // Unregister swipe input events
    public void OnExit()
    {
        InputManager.Instance.swipePressStarted -= OnSwipePressStarted;
        InputManager.Instance.swipePressEnded -= OnSwipePressEnded;
    }

    // Called when swipe input starts
    void OnSwipePressStarted(Vector2 pos)
    {
        if (!CanAcceptInput()) return;
        swipeStartScreen = pos;
        isSwiping = true;
    }

    // Called when swipe input ends
    void OnSwipePressEnded(Vector2 pos)
    {
        if (!isSwiping || !CanAcceptInput()) return;
        if (HandleSwipe(swipeStartScreen, pos) == 1) {
            isSwiping = false;
            GamePhaseManager.Instance.SetInputLock(true);
            CombatManager.Instance.UseCharge();
            playerContext.StopBurstingAnimation();
        };
    }

    // Handles the swipe gesture and moves the player
    int HandleSwipe(Vector2 swipeStartScreen, Vector2 swipeEndScreen)
    {
        // Min Swipe Distance Check
        Vector3 swipeStartWorld = ScreenToWorld(swipeStartScreen);
        Vector3 swipeEndWorld = ScreenToWorld(swipeEndScreen);
        Vector3 direction = swipeEndWorld - swipeStartWorld;
        float distance = direction.magnitude;
        if (distance < minSwipeDistance) return 0;

        playerContext.SetAnimationState(AnimationState.Attacking);

        direction.Normalize();
        Vector3 rawTarget = playerTransform.position + direction * Mathf.Min(distance, slashRadius);
        Vector3 clampedTarget = rawTarget;

        // Clamp target if dash would go underground
        if (rawTarget.y < 0f)
        {
            Vector3 start = playerTransform.position;
            Vector3 end = rawTarget;
            float t = (0f - start.y) / (end.y - start.y);
            t = Mathf.Clamp01(t - 0.05f);
            clampedTarget = Vector3.Lerp(start, end, t);
            clampedTarget.y = 0f;
        }

        // Add a slight curve to the dash path
        Vector3 midpoint = (playerTransform.position + clampedTarget) / 2f;
        Vector3 perpendicular = Vector3.Cross(direction, Vector3.forward).normalized;
        float curveAmount = Random.Range(-0.3f, 0.3f);
        midpoint += perpendicular * curveAmount;
        Vector3[] path = new Vector3[] {
            playerTransform.position,
            midpoint,
            clampedTarget
        };
        // Draw slash line and enable dash trail
        slashLine.positionCount = 2;
        slashLine.SetPosition(0, playerTransform.position);
        slashLine.SetPosition(1, clampedTarget);
        slashLine.enabled = true;
        if (dashTrail != null)
        {
            dashTrail.Clear();
            dashTrail.enabled = true;
        }
        // Animate player along the path
        playerTransform.DOPath(path, Vector3.Distance(playerTransform.position, clampedTarget) / dashSpeed, PathType.CatmullRom)
            .SetEase(Ease.OutSine)
            .OnComplete(() =>
            {
                slashLine.enabled = false;
                if (dashTrail != null) dashTrail.enabled = false;
                GamePhaseManager.Instance.SetInputLock(false);
                playerContext.SetAnimationState(AnimationState.None);
            });
        return 1;
    }

    // Checks if input can be accepted for a swipe
    bool CanAcceptInput()
    {
        return CombatManager.Instance.IsCombatActive() &&
               !GamePhaseManager.Instance.IsInputLocked &&
               playerContext.CurrentLocation == PlayerLocation.AboveGround;
    }

    // Converts screen position to world position
    Vector3 ScreenToWorld(Vector2 screenPos)
    {
        Vector3 pos = new Vector3(screenPos.x, screenPos.y, -mainCam.transform.position.z);
        return mainCam.ScreenToWorldPoint(pos);
    }

    // Draws the slash radius in the editor
    void OnDrawGizmosSelected()
    {
        if (playerTransform != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerTransform.position, slashRadius);
        }
    }
}
