// KillFeedbackUI.cs
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

public class KillFeedbackUI : MonoBehaviour
{
    public TMP_Text scoreText;
    public Image killLabelImage;
    public float punchScale = 1.5f;
    public float punchDuration = 0.4f;
    public float riseDistance = 20f;
    public float riseDuration = 2f;
    public GameObject panel;

    private int currentScore = 0;

    public void PlayKillFeedback(int scoreToAdd)
    {
        int from = currentScore;
        int to = currentScore + scoreToAdd;
        currentScore = to;

        // Tween the score value
        DOTween.To(() => from, x =>
        {
            from = x;
            scoreText.text = "+" + from.ToString();
        }, to, 1f).SetEase(Ease.OutQuad);

        // Score text punch and rise
        scoreText.transform.localScale = Vector3.zero;

        Sequence scoreSeq = DOTween.Sequence();
        scoreSeq.Append(scoreText.transform.DOScale(Vector3.one * punchScale, 0.5f).SetEase(Ease.OutBack))
                .Append(scoreText.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.InOutSine));


        // Kill label effect
        killLabelImage.transform.localScale = Vector3.zero;
        killLabelImage.DOFade(1, 0.1f);
        killLabelImage.transform.DOScale(1.2f, 0.25f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                killLabelImage.transform.DOScale(1f, 0.5f);
                killLabelImage.DOFade(0, 0.5f).SetDelay(1f);
            });
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            panel.SetActive(false);
        }
    }
}
