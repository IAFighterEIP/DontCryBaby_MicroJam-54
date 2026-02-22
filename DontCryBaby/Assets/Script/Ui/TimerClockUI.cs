using UnityEngine;
using TMPro;

public class TimerClockUI : MonoBehaviour
{
    [SerializeField] private LevelTimer timer;
    [SerializeField] private TMP_Text label;

    private void Reset()
    {
        label = GetComponent<TMP_Text>();
    }

    private void Awake()
    {
        if (label == null) label = GetComponent<TMP_Text>();
        if (timer == null) timer = FindFirstObjectByType<LevelTimer>();
    }

    private void Update()
    {
        if (timer == null || label == null) return;
        label.text = timer.GetClockText();
    }
}