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

    public void OnEnter()
    {
        InputManager.Instance.swipePressStarted += OnSwipePressStarted;
        InputManager.Instance.swipePressEnded += OnSwipePressEnded;
    }

    public void OnUpdate()
    {
        // No update logic needed for swipe input management
    }

    public void OnExit()
    {
        InputManager.Instance.swipePressStarted -= OnSwipePressStarted;
        InputManager.Instance.swipePressEnded -= OnSwipePressEnded;
    }

    void OnSwipePressStarted(Vector2 pos)
    {
        // Debug.Log($"[SwipeInputManager] OnSwipePressStarted at {pos}, isSwiping={isSwiping}");
        if (!CanAcceptInput()) return;
        swipeStartScreen = pos;
        isSwiping = true;
    }

    void OnSwipePressEnded(Vector2 pos)
    {
        // Debug.Log($"[SwipeInputManager] OnSwipePressEnded at {pos}, isSwiping={isSwiping}");
        if (!isSwiping || !CanAcceptInput()) return;
        isSwiping = false;
        HandleSwipe(swipeStartScreen, pos);
        CombatManager.Instance.UseCharge(); // Use a charge after a successful swipe
        GamePhaseManager.Instance.SetInputLock(true); // Unlock input after swipe
    }

    void HandleSwipe(Vector2 swipeStartScreen, Vector2 swipeEndScreen)
    {
        Vector3 swipeStartWorld = ScreenToWorld(swipeStartScreen);
        Vector3 swipeEndWorld = ScreenToWorld(swipeEndScreen);
        Vector3 direction = swipeEndWorld - swipeStartWorld;
        float distance = direction.magnitude;
        if (distance < minSwipeDistance)
        {
            // Debug.Log("Swipe too short.");
            return;
        }
        direction.Normalize();
        Vector3 rawTarget = playerTransform.position + direction * Mathf.Min(distance, slashRadius);
        Vector3 clampedTarget = rawTarget;
        if (clampedTarget.y < 0f) clampedTarget.y = 0f;
        // Curve midpoint for style
        Vector3 midpoint = (playerTransform.position + clampedTarget) / 2f;
        Vector3 perpendicular = Vector3.Cross(direction, Vector3.forward).normalized;
        float curveAmount = Random.Range(-0.3f, 0.3f);
        midpoint += perpendicular * curveAmount;
        Vector3[] path = new Vector3[] {
            playerTransform.position,
            midpoint,
            clampedTarget
        };
        // Visual slash line
        slashLine.positionCount = 2;
        slashLine.SetPosition(0, playerTransform.position);
        slashLine.SetPosition(1, clampedTarget);
        slashLine.enabled = true;
        if (dashTrail != null)
        {
            dashTrail.Clear();
            dashTrail.enabled = true;
        }
        CombatManager.Instance.PauseAllEnemyCountdowns();
        playerTransform.DOPath(path, Vector3.Distance(playerTransform.position, clampedTarget) / dashSpeed, PathType.CatmullRom)
            .SetEase(Ease.OutSine)
            .OnComplete(() =>
            {
                slashLine.enabled = false;
                if (dashTrail != null) dashTrail.enabled = false;
                CombatManager.Instance.ResumeAllEnemyCountdowns();
                GamePhaseManager.Instance.SetInputLock(false); // Unlock input after swipe
            });
    }

    bool CanAcceptInput()
    {
        return CombatManager.Instance.IsCombatActive() &&
               !GamePhaseManager.Instance.IsInputLocked &&
               playerContext.CurrentLocation == PlayerLocation.AboveGround;
    }

    Vector3 ScreenToWorld(Vector2 screenPos)
    {
        Vector3 pos = new Vector3(screenPos.x, screenPos.y, -mainCam.transform.position.z);
        return mainCam.ScreenToWorldPoint(pos);
    }

    void OnDrawGizmosSelected()
    {
        if (playerTransform != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerTransform.position, slashRadius);
        }
    }
}
