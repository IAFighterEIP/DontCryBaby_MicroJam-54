using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LevelTimer timer;
    [SerializeField] private ScoreCalculator scoreCalculator;
    [SerializeField] private AudioSource alarmAudio;

    [Header("UI")]
    [SerializeField] private GameObject endPanel;
    [SerializeField] private TMP_Text scoreText;

    private void Awake()
    {
        timer.OnTimeUp += HandleTimeUp;
    }

    private void OnDestroy()
    {
        timer.OnTimeUp -= HandleTimeUp;
    }

    private void HandleTimeUp()
    {
        // 1️⃣ Calculate score
        float finalScore = scoreCalculator.CalculateFinalScore();

        // 2️⃣ Show score
        if (scoreText != null)
            scoreText.text = finalScore.ToString("0");

        // 3️⃣ Show end panel
        if (endPanel != null)
            endPanel.SetActive(true);

        // 4️⃣ Play alarm
        if (alarmAudio != null)
            alarmAudio.Play();

        // 5️⃣ Freeze game
        Time.timeScale = 0f;
    }
}