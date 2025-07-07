using UnityEngine;
using TMPro;

public class TunnelDrawerUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject timerUI;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.red;
    [SerializeField] private float warningThreshold = 3f;
    private Vector3 originalScale;
    private bool isShaking = false;
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float shakeAmount = 0.1f;

    [SerializeField] private TunnelDrawer tunnelDrawer;

    void Awake()
    {
        if (timerText != null)
            originalScale = timerText.rectTransform.localScale;
    }

    void OnEnable()
    {
        if (tunnelDrawer == null) tunnelDrawer = GetComponent<TunnelDrawer>();
        if (tunnelDrawer == null) return;
        tunnelDrawer.OnDrawTimeoutChanged += UpdateTimer;
        tunnelDrawer.OnDrawTimeoutEnded += HideTimerUI;
        ShowTimerUI();
    }

    void OnDisable()
    {
        if (tunnelDrawer == null) return;
        tunnelDrawer.OnDrawTimeoutChanged -= UpdateTimer;
        tunnelDrawer.OnDrawTimeoutEnded -= HideTimerUI;
    }

    private void UpdateTimer(float time)
    {
        if (timerText == null) return;
        if (timerText.gameObject.activeSelf == false)
            timerText.gameObject.SetActive(true);
        timerText.text = Mathf.CeilToInt(time).ToString();
        bool warning = time < warningThreshold;
        timerText.color = warning ? warningColor : normalColor;
        if (!warning)
        {
            timerText.rectTransform.localScale = originalScale;
            isShaking = false;
        }
        if (warning && !isShaking)
            ShakeTimer();
    }

    private void ShowTimerUI()
    {
        if (timerUI != null)
            timerUI.SetActive(true);
        if (timerText != null)
            timerText.gameObject.SetActive(true);
    }

    private void HideTimerUI()
    {
        if (timerUI != null)
            timerUI.SetActive(false);
        if (timerText != null)
            timerText.gameObject.SetActive(false);
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
}
