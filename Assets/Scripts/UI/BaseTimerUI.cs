using UnityEngine;
using TMPro;
using System.Collections;

public abstract class BaseTimerUI : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] protected TMP_Text timerText;
    [SerializeField] protected Color normalColor = Color.white;
    [SerializeField] protected Color warningColor = Color.red;
    [SerializeField] protected float warningThreshold = 3f;
    [SerializeField] protected float shakeDuration = 0.2f;
    [SerializeField] protected float shakeAmount = 0.1f;

    protected Vector3 originalScale;
    protected bool isShaking = false;

    protected virtual void Awake()
    {
        if (timerText != null)
            originalScale = timerText.rectTransform.localScale;
    }

    protected void UpdateTimerDisplay(float time)
    {
        if (timerText == null) return;

        timerText.text = Mathf.CeilToInt(time).ToString();
        bool warning = time < warningThreshold;
        timerText.color = warning ? warningColor : normalColor;

        if (!warning)
        {
            timerText.rectTransform.localScale = originalScale;
            isShaking = false;
        }

        if (warning && !isShaking)
            StartCoroutine(ShakeTimerCoroutine());
    }

    protected IEnumerator ShakeTimerCoroutine()
    {
        isShaking = true;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            if (timerText == null) yield break;
            Vector3 randomOffset = Random.insideUnitSphere * shakeAmount;
            randomOffset.z = 0f;
            timerText.rectTransform.localScale = originalScale + randomOffset;
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (timerText != null)
            timerText.rectTransform.localScale = originalScale;
        isShaking = false;
    }

    protected virtual void SetUIActive(bool active)
    {
        if (timerText != null)
            timerText.gameObject.SetActive(active);
    }
}