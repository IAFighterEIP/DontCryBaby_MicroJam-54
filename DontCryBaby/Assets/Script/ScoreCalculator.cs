using UnityEngine;
using System.Linq;

public class ScoreCalculator : MonoBehaviour
{
    [Header("Breakables")]
    [Tooltip("If true, collects breakables once on Start. Disable if you spawn breakables during play.")]
    [SerializeField] private bool cacheBreakablesOnStart = true;

    [Tooltip("Optional multiplier if you want to scale score.")]
    [SerializeField] private float scoreMultiplier = 1f;

    private IBreakable[] cachedBreakables;

    private void Start()
    {
        if (cacheBreakablesOnStart)
            cachedBreakables = FindBreakables();
    }

    private IBreakable[] FindBreakables()
    {
        return FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .OfType<IBreakable>()
            .ToArray();
    }

    private IBreakable[] GetBreakables()
        => cacheBreakablesOnStart ? cachedBreakables : FindBreakables();

    // 🔥 THIS is your rule:
    // Score = sum of all remaining life values
    public float CalculateFinalScore()
    {
        var breakables = GetBreakables();
        if (breakables == null || breakables.Length == 0)
            return 0f;

        float totalRemainingLife = 0f;

        foreach (var b in breakables)
        {
            totalRemainingLife += Mathf.Max(0f, b.CurrentLife);
        }

        return totalRemainingLife * scoreMultiplier;
    }

    public ScoreBreakdown GetBreakdown()
    {
        var breakables = GetBreakables();
        float total = 0f;

        if (breakables != null)
        {
            foreach (var b in breakables)
                total += Mathf.Max(0f, b.CurrentLife);
        }

        return new ScoreBreakdown
        {
            TotalRemainingLife = total,
            Multiplier = scoreMultiplier,
            FinalScore = total * scoreMultiplier
        };
    }

    [System.Serializable]
    public struct ScoreBreakdown
    {
        public float TotalRemainingLife;
        public float Multiplier;
        public float FinalScore;
    }
}