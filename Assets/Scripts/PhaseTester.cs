using UnityEngine;

public class PhaseTester : MonoBehaviour
{
    [SerializeField]
    GamePhase startingPhase;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GamePhaseManager.Instance.SetPhase(startingPhase);
    }

   
}
