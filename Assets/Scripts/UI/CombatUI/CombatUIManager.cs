using UnityEngine;
using TMPro;

public class chargesUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject chargesUI;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.red;
    [SerializeField] private float warningThreshold = 3f;
    private Vector3 originalScale;
    private bool isShaking = false;
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float shakeAmount = 0.1f;

    private void Awake()
    {
        if (timerText != null)
            originalScale = timerText.rectTransform.localScale;
    }

    private void OnEnable()
    {
        CombatManager.Instance.OnTimerChanged += HandleTimerChanged;
        CombatManager.Instance.OnCombatStateChanged += SetchargesUIActive;
    }

    private void OnDisable()
    {
        if (CombatManager.Instance == null) return;
        CombatManager.Instance.OnTimerChanged -= HandleTimerChanged;
        CombatManager.Instance.OnCombatStateChanged -= SetchargesUIActive;
    }

    private void HandleTimerChanged(float time)
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
        // Optionally, trigger shake if warning
        if (warning && !isShaking)
            ShakeTimer();
    }

    private void ShakeTimer()
    {
        if (!isShaking && timerText != null)
            StartCoroutine(ShakeTimerCoroutine());
    }

    private System.Collections.IEnumerator ShakeTimerCoroutine()
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

    private void SetchargesUIActive(bool active)
    {
        if (chargesUI != null)
            chargesUI.SetActive(active);
        if (timerText != null)
            timerText.gameObject.SetActive(active);
    }
}
