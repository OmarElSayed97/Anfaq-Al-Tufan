using UnityEngine;

public class CombatUIManager : BaseTimerUI
{
    [Header("UI References")]
    [SerializeField] private GameObject chargesUI;

    private void OnEnable()
    {
        CombatManager.Instance.OnTimerChanged += HandleTimerChanged;
        CombatManager.Instance.OnCombatStateChanged += SetUIActive;
    }

    private void OnDisable()
    {
        if (CombatManager.Instance == null) return;
        CombatManager.Instance.OnTimerChanged -= HandleTimerChanged;
        CombatManager.Instance.OnCombatStateChanged -= SetUIActive;
    }

    private void HandleTimerChanged(float time)
    {
        UpdateTimerDisplay(time);
    }

    protected override void SetUIActive(bool active)
    {
        base.SetUIActive(active); 
        if (chargesUI != null)
            chargesUI.SetActive(active);
    }
}