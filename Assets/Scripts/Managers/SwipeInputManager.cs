// SwipeInputManager.cs
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class SwipeInputManager : MonoBehaviour
{
    [Header("References")]
    public Transform playerTransform;
    public PlayerContextManager playerContext;
    public LineRenderer slashLine;
    public TrailRenderer dashTrail;
    public float minSwipeDistance = 0.5f;
    public float slashRadius = 4f;
    public float dashSpeed = 10f;
    public LayerMask enemyLayer;

    private Vector3 swipeStart;
    private bool isSwiping = false;
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
        if (dashTrail != null) dashTrail.enabled = false;
    }

    void Update()
    {
        if (!CombatManager.Instance.IsCombatActive()) return;
        if (GamePhaseManager.Instance.IsInputLocked) return;
        if (playerContext.CurrentLocation != PlayerLocation.AboveGround) return;

        if (Input.GetMouseButtonDown(0))
        {
            swipeStart = GetMouseWorldPosition();
            isSwiping = true;
        }

        if (Input.GetMouseButtonUp(0) && isSwiping)
        {
            isSwiping = false;
            Vector3 swipeEnd = GetMouseWorldPosition();
            HandleSwipe(swipeStart, swipeEnd);
        }
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 screenPos = Input.mousePosition;
        screenPos.z = -mainCam.transform.position.z;
        return mainCam.ScreenToWorldPoint(screenPos);
    }

    void HandleSwipe(Vector3 start, Vector3 end)
    {
        Vector3 direction = end - start;
        float distance = direction.magnitude;

        if (distance < minSwipeDistance)
        {
            Debug.Log("Swipe too short.");
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

                Vector2 origin = playerTransform.position;
                Vector2 target = new Vector2(clampedTarget.x, clampedTarget.y);
                Vector2 swipeDir = (target - origin).normalized;
                float swipeLength = Vector2.Distance(origin, clampedTarget);
             

               
            });
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
