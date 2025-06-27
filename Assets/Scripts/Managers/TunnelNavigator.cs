// TunnelNavigator.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class TunnelNavigator : MonoBehaviour
{
    [Header("References")]
    public PlayerContextManager playerContext;
    public Transform characterVisual; // child visual (e.g., triangle)

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float arrivalThreshold = 0.05f;
    public float burstHeight = 1.5f;
    public float burstDuration = 0.25f;
    public Transform radiusIndicator;
    public float sliceRadius = 4f;
    public LayerMask enemyLayer;

    private List<Vector3> path;
    private int currentIndex = 0;
    private bool isNavigating = false;
    private Tween hoverTween;
    private bool hovering = false;
    private float lastUndergroundY;
    private float previousY;
    private float deepestY;
    private bool hasRecordedBurstDepth;
    private bool enemiesChecked;

    public void StartNavigation(Vector3[] tunnelPath)
    {
        if (tunnelPath == null || tunnelPath.Length < 2)
        {
            Debug.LogWarning("Invalid tunnel path");
            return;
        }

        path = new List<Vector3>
        {
            transform.position // Add current position as starting approach point
        };
        path.AddRange(tunnelPath);    // Follow actual tunnel afterward

        currentIndex = 0;
        isNavigating = true;

        lastUndergroundY = transform.position.y;
        previousY = transform.position.y;
        deepestY = transform.position.y;
        hasRecordedBurstDepth = false;

        // Set player animation and phase
        playerContext.SetAnimationState(AnimationState.Traveling);
        GamePhaseManager.Instance.SetInputLock(true);
    }

    public void Initialize()
    {
        // InputManager.Instance.swipePressStarted += OnHoverInput;
    }

    public void FinalizeNavigation()
    {
        // InputManager.Instance.swipePressStarted -= OnHoverInput;
    }

    public void OnUpdate()
    {
        // Traverse through the points
        if (isNavigating && path != null && currentIndex < path.Count)
        {
            enemiesChecked = false;
            Vector3 target = path[currentIndex];
            Vector3 moveDir = target - transform.position;
            moveDir.z = 0;

          // Update deepest Y while underground
           if (transform.position.y < 0f)
            {
                if (transform.position.y < deepestY)
                {
                    deepestY = transform.position.y;
                    hasRecordedBurstDepth = false; // allow update if we go deeper again
                }

                // Only record if rising and we have a valid deepest point
                if (!hasRecordedBurstDepth && transform.position.y > previousY && deepestY < 0f)
                {
                    lastUndergroundY = deepestY;
                    hasRecordedBurstDepth = true;
                }
            }

            // Move toward current path point
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

            // Rotate visual to face movement direction
            if (moveDir.sqrMagnitude > 0.001f)
            {
                float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
                characterVisual.rotation = Quaternion.Euler(0, 0, angle);
            }

            // Check if arrived at this point
            if (Vector3.Distance(transform.position, target) <= arrivalThreshold)
            {
                currentIndex++;

                if (currentIndex >= path.Count)
                {
                    isNavigating = false;
                    StartCoroutine(HandleBurst());
                    // DecideNextPhase();
                }
            }
        } 
    }

    private void DecideNextPhase()
    {
        if (!hovering) return;
        if (AreEnemiesNearby())
        {
            EndHover(GamePhase.Combat);
        }
        else if (!enemiesChecked)
        {
            EndHover(GamePhase.TunnelDrawing);
        }
    }

    IEnumerator HandleBurst()
    {
        playerContext.SetAnimationState(AnimationState.Bursting);

         // Camera shake
        CameraShaker.Instance.Shake();

        // Show and scale up the radius indicator
        float diameter = sliceRadius * 2f;
        radiusIndicator.localScale = Vector3.zero;
        radiusIndicator.gameObject.SetActive(true);
        radiusIndicator.DOScale(new Vector3(diameter, diameter, 1f), 0.4f)
            .SetEase(Ease.OutBack);
        radiusIndicator.DORotate(new Vector3(0f, 0f, 360f), 4f, RotateMode.FastBeyond360).SetEase(Ease.Linear)
        .SetLoops(-1, LoopType.Incremental);

        characterVisual.DOLocalRotate(new Vector3(0, 0, 360), 0.4f, RotateMode.FastBeyond360).SetEase(Ease.InOutQuad)
        .OnComplete(() =>
        {
            characterVisual.localRotation = Quaternion.Euler(Vector3.zero);
        });

        Vector3 start = transform.position;
        float depth = Mathf.InverseLerp(-1, -4, deepestY);
        float dynamicBurst = Mathf.Lerp(0.6f, 3.6f, depth);
        Vector3 peak = start + Vector3.up * dynamicBurst;
        float t = 0f;

        // Burst jump
        while (t < 1f)
        {
            t += Time.deltaTime / burstDuration;
            transform.position = Vector3.Lerp(start, peak, t);
            yield return null;
        }

        // Hover animation
        playerContext.SetAnimationState(AnimationState.Hovering);
        hoverTween = characterVisual.DOMoveY(characterVisual.position.y + 0.15f, 0.5f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);

        GamePhaseManager.Instance.SetInputLock(false);
        hovering = true;
        DecideNextPhase();

        Debug.Log("Tunnel navigation and burst complete. Awaiting player input.");
    }

    void EndHover(GamePhase nextPhase)
    {
        if (hoverTween != null && hoverTween.IsActive())
            hoverTween.Kill();

        characterVisual.localPosition = Vector3.zero;
        characterVisual.localRotation = Quaternion.Euler(Vector3.zero);
        characterVisual.DOKill();
        if (radiusIndicator != null && radiusIndicator.gameObject.activeSelf)
        {
            radiusIndicator.DOKill(); // Stops rotation and scaling tweens

            radiusIndicator.DOScale(Vector3.zero, 0.3f)
                .SetEase(Ease.InBack)
                .OnComplete(() => radiusIndicator.gameObject.SetActive(false));
        }

        playerContext.SetAnimationState(AnimationState.None);
        GamePhaseManager.Instance.SetPhase(nextPhase);
        hovering = false;
    }

    bool AreEnemiesNearby()
    {
        if (!enemiesChecked)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(playerContext.playerTransform.position, sliceRadius, enemyLayer);
            List<Enemy> enemiesInRange = new List<Enemy>();

            foreach (var col in colliders)
            {
                Enemy enemy = col.GetComponent<Enemy>();
                if (enemy != null && enemy.IsAlive())
                    enemiesInRange.Add(enemy);
            }

            // Determine charges based on tunnel depth or fixed for now
            int charges = 3;

            if (enemiesInRange.Count > 0)
            {
                CombatManager.Instance.StartCombat(enemiesInRange, charges);
                enemiesChecked = true;
                return true;
            }
            else
                return false;
        }
        else
            return true;
    }
}
