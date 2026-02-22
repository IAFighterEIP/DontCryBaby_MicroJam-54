using UnityEngine;
using System;

public class LevelTimer : MonoBehaviour
{
    public event Action OnTimeUp;

    [Header("Game Duration")]
    [Tooltip("Real gameplay duration in seconds (ex: 300 for 5 minutes).")]
    [SerializeField] private float gameDurationSeconds = 180f;

    [Header("Displayed Clock Range (always ends here)")]
    [Tooltip("Displayed start time in minutes:seconds (20:00).")]
    [SerializeField] private int displayStartMinutes = 20;
    [SerializeField] private int displayStartSeconds = 0;

    [Tooltip("Displayed end time in minutes:seconds (23:00).")]
    [SerializeField] private int displayEndMinutes = 23;
    [SerializeField] private int displayEndSeconds = 0;

    [Header("Behavior")]
    [SerializeField] private bool autoStartOnPlay = true;

    public bool IsRunning { get; private set; }
    public bool HasEnded { get; private set; }
    public float ElapsedSeconds { get; private set; }

    private int DisplayStartTotalSeconds => Mathf.Max(0, displayStartMinutes * 60 + displayStartSeconds);
    private int DisplayEndTotalSeconds   => Mathf.Max(0, displayEndMinutes * 60 + displayEndSeconds);
    private int DisplaySpanSeconds       => Mathf.Max(0, DisplayEndTotalSeconds - DisplayStartTotalSeconds);

    private void Start()
    {
        if (autoStartOnPlay)
            StartGame();
    }

    private void Update()
    {
        if (!IsRunning || HasEnded) return;

        ElapsedSeconds += Time.deltaTime;

        if (ElapsedSeconds >= gameDurationSeconds)
        {
            Debug.Log(ElapsedSeconds);
            ElapsedSeconds = gameDurationSeconds;
            EndGame();
        }
    }

    public void StartGame()
    {
        HasEnded = false;
        ElapsedSeconds = 0f;
        IsRunning = true;
    }

    public void Pause(bool pause)
    {
        if (HasEnded) return;
        IsRunning = !pause;
    }

    public void EndGame()
    {
        if (HasEnded) return;

        HasEnded = true;
        IsRunning = false;
        OnTimeUp?.Invoke();
    }

    /// <summary>
    /// Displayed clock time scaled so it ALWAYS reaches displayEnd at the exact end of the game duration.
    /// </summary>
    public int GetDisplayTotalSeconds()
    {
        if (gameDurationSeconds <= 0.0001f)
            return DisplayEndTotalSeconds;

        float t = Mathf.Clamp01(ElapsedSeconds / gameDurationSeconds); // 0 -> 1
        int add = Mathf.FloorToInt(DisplaySpanSeconds * t);
        return DisplayStartTotalSeconds + add;
    }

    public string GetClockText()
    {
        int total = GetDisplayTotalSeconds();
        int minutes = total / 60;
        int seconds = total % 60;
        return $"{minutes:00}:{seconds:00}";
    }
}