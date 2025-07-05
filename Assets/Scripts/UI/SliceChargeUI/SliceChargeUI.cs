using TMPro;
using UnityEngine;

public class SliceChargeUI : MonoBehaviour
{

    [SerializeField] private TMP_Text chargeText;

    void Start()
    {
        CombatManager.Instance.OnChargesChanged += UpdateChargeDisplay;
    }
    public void UpdateChargeDisplay(int currentCharges)
    {
        chargeText.gameObject.SetActive(true);
        chargeText.text = $"Slice Charges: {currentCharges}";
    }
    void OnDestroy()
    {
        if (CombatManager.Instance != null)
            CombatManager.Instance.OnChargesChanged -= UpdateChargeDisplay;
    }
}
