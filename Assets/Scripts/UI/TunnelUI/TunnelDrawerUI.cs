using UnityEngine;

public class TunnelDrawerUI : BaseTimerUI
{
    [Header("UI References")]
    [SerializeField] private GameObject timerUI;
    [SerializeField] private TunnelDrawer tunnelDrawer;

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
        ShowTimerUI();
        UpdateTimerDisplay(time);
    }

    private void ShowTimerUI()
    {
        if (timerUI != null) timerUI.SetActive(true);
        SetUIActive(true);
    }

    private void HideTimerUI()
    {
        if (timerUI != null) timerUI.SetActive(false);
        SetUIActive(false);
    }
}