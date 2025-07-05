using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEditor;

public class PlayerContextManager : MonoBehaviour
{
    [Header("References")]
    public Transform playerTransform;

    [Header("Animation Settings")]
    public Transform radiusIndicator;
    public float sliceRadius = 4f;
    public Transform characterVisual; // child visual (e.g., triangle)

    [Header("Debug Info (Read Only)")]
    [SerializeField, ReadOnly] private PlayerLocation currentLocation;
    [SerializeField, ReadOnly] private float depth;
    [SerializeField, ReadOnly] private AnimationState currentAnimationState = AnimationState.None;
    [SerializeField, ReadOnly] private bool isAnimating;

    private Tween hoverTween;
    private Dictionary<AnimationState, Action> animationStateActions;

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

    private void Awake()
    {
        animationStateActions = new Dictionary<AnimationState, Action>
        {
            { AnimationState.None, OnNone },
            { AnimationState.Hovering, OnHovering },
            { AnimationState.Traveling, OnTraveling },
            { AnimationState.Bursting, OnBursting },
            { AnimationState.Attacking, OnAttacking }
        };
    }

    public void SetAnimationState(AnimationState newState)
    {
        CurrentAnimationState = newState;
        // if (animationStateActions.TryGetValue(newState, out var action))
        //     action?.Invoke();
    }

    // Example handlers
    private void OnNone() { }
    private void OnHovering()
    {
        StopBurstingAnimation();
        hoverTween = PlayHoveringAnimation();
    }

    private void OnBursting()
    {
        PlayBurstingAnimation();
        hoverTween = PlayHoveringAnimation();
    }

    private void OnAttacking()
    {
        StopBurstingAnimation();
        if (hoverTween != null && hoverTween.IsActive())
            StopHoveringAnimation(hoverTween);
    }

    public void PlayBurstingAnimation()
    {
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
    }

    public void StopBurstingAnimation()
    {
        if (radiusIndicator != null && radiusIndicator.gameObject.activeSelf)
        {
            radiusIndicator.DOKill();

            radiusIndicator.DOScale(Vector3.zero, 0.3f)
                .SetEase(Ease.InBack)
                .OnComplete(() => radiusIndicator.gameObject.SetActive(false));
        }

        characterVisual.localPosition = Vector3.zero;
        characterVisual.localRotation = Quaternion.Euler(Vector3.zero);
        characterVisual.DOKill();
    }

    public Tween PlayHoveringAnimation()
    {
        return characterVisual.DOMoveY(characterVisual.position.y + 0.15f, 0.5f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    public void StopHoveringAnimation(Tween hoverTween)
    {
        if (hoverTween != null && hoverTween.IsActive())
            hoverTween.Kill();
        characterVisual.localPosition = Vector3.zero;
        characterVisual.localRotation = Quaternion.Euler(Vector3.zero);
        characterVisual.DOKill();
    }

    private void OnTraveling() { Debug.Log("Jumping animation."); }

    private void Update()
    {
        Debug.DrawLine(Vector3.zero, new Vector3(0, playerTransform.position.y, 0), Color.cyan);

        // Update debug fields for inspector
        currentLocation = CurrentLocation;
        depth = Depth;
        currentAnimationState = CurrentAnimationState;
        isAnimating = IsAnimating;
    }
}