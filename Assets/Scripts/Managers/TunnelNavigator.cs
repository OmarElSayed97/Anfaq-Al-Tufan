using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using System.Collections;
using Unity.VisualScripting;

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
    public LayerMask enemyLayer;

    private List<Vector3> path;
    private int currentIndex = 0;
    private bool isNavigating = false;
    private bool hovering = false;
    private float lastUndergroundY;
    private float previousY;
    private float deepestY;
    private bool hasRecordedBurstDepth;
    private bool enemiesChecked;
    public float sliceRadius = 4f;

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
                    playerContext.SetAnimationState(AnimationState.Bursting);
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

        // Camera shake (keep here, as it's gameplay feedback)
        CameraShaker.Instance.Shake();

        playerContext.PlayBurstingAnimation();

        // Animate handled by PlayerContextManager's OnBursting/OnHovering
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

        // Set to hovering state, animation handled by PlayerContextManager
        playerContext.SetAnimationState(AnimationState.Hovering);
        
        hovering = true;
        DecideNextPhase();

        Debug.Log("Tunnel navigation and burst complete. Awaiting player input.");
    }

    void EndHover(GamePhase nextPhase)
    {
        characterVisual.localPosition = Vector3.zero;
        characterVisual.localRotation = Quaternion.Euler(Vector3.zero);
        characterVisual.DOKill();

        playerContext.SetAnimationState(AnimationState.None);
        GamePhaseManager.Instance.SetPhase(nextPhase);
        hovering = false;
        GamePhaseManager.Instance.SetInputLock(false); // Unlock input after hover
    }

    bool AreEnemiesNearby()
    {
        if (!enemiesChecked)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(playerContext.playerTransform.position, sliceRadius, enemyLayer);
            List<BaseEnemy> enemiesInRange = new List<BaseEnemy>();

            foreach (var col in colliders)
            {
                BaseEnemy enemy = col.GetComponent<BaseEnemy>();
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
