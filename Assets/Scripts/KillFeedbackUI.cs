using UnityEngine;
using DG.Tweening;
using TMPro;
using System.Collections.Generic;

public class KillFeedbackUI : MonoBehaviour
{
    public GameObject scorePopupPrefab;     
    public Transform poolParent;
    public int poolSize = 3;
    public TMP_Text totalScoreText;
    private int currentScore = 0;
    private Queue<GameObject> popupPool = new Queue<GameObject>();

    void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject popup = Instantiate(scorePopupPrefab, poolParent);
            popup.SetActive(false);
            popupPool.Enqueue(popup);
        }
    }

    public void PlayKillFeedback(int scoreToAdd, Vector3 worldPosition)
    {
        if (popupPool.Count == 0)
        {
            Debug.LogWarning("No available score popups in pool! Consider increasing pool size.");
            return;
        }

        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);

        GameObject popup = popupPool.Dequeue();
        popup.SetActive(true);
        popup.transform.position = screenPos;

        TMP_Text scoreText = popup.GetComponentInChildren<TMP_Text>();
        scoreText.text = "+" + scoreToAdd.ToString();
        popup.transform.localScale = Vector3.zero;

        Sequence scoreSeq = DOTween.Sequence();
        scoreSeq.Append(popup.transform.DOScale(Vector3.one * 1.5f, 0.4f).SetEase(Ease.OutBack))
                .Join(popup.transform.DOMoveY(popup.transform.position.y + 100f, 1f).SetEase(Ease.OutQuad))
                // .AppendInterval(0.5f)
                .OnComplete(() =>
                {
                    popup.SetActive(false);
                    popupPool.Enqueue(popup);
                });
        currentScore += scoreToAdd;
        totalScoreText.text = "Score: " + currentScore;
    }
}
