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

    private GameObject EndUI;

    private void Awake()
    {
        timer.OnTimeUp += HandleTimeUp;

        EndUI = GameObject.Find("Canvas");

        if (EndUI != null) {
            GameObject exitButtonObj = EndUI.transform.Find("ExitButton")?.gameObject;
            GameObject playButtonObj = EndUI.transform.Find("PlayButton")?.gameObject;
            GameObject ScoreObj = EndUI.transform.Find("ScoreText")?.gameObject;
            exitButtonObj?.SetActive(false);
            playButtonObj?.SetActive(false);
            ScoreObj?.SetActive(false);
        }
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

        // 6️⃣ Show cursor and End UI
        Cursor.visible = true;
        if (EndUI != null) {
            GameObject exitButtonObj = EndUI.transform.Find("ExitButton")?.gameObject;
            GameObject playButtonObj = EndUI.transform.Find("PlayButton")?.gameObject;
            GameObject ScoreObj = EndUI.transform.Find("ScoreText")?.gameObject;

            if (exitButtonObj != null)
                exitButtonObj.SetActive(true);
            if (playButtonObj != null)
                playButtonObj.SetActive(true);
            if (ScoreObj != null) {
                ScoreObj.GetComponent<TMP_Text>().text = "Score: " + finalScore.ToString("0");
                ScoreObj.SetActive(true);
            }
        }

        Debug.Log("Time's up! Final Score: " + finalScore);
    }
}