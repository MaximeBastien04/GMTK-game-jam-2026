using System;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Money")]
    [SerializeField] private int startingMoney;

    [Header("UI")]
    [SerializeField] private TMP_Text scoreText;

    public int CurrentScore { get; private set; }

    public event Action<int> ScoreChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        CurrentScore = startingMoney;
    }

    private void Start()
    {
        UpdateScoreUI();
    }

    public void AddScore(int amount)
    {
        if (amount <= 0)
            return;

        CurrentScore += amount;

        MoneyChanged();
    }

    public bool CanAfford(int amount)
    {
        return CurrentScore >= amount;
    }

    public bool TrySpendScore(int amount)
    {
        if (amount < 0)
            return false;

        if (!CanAfford(amount))
            return false;

        CurrentScore -= amount;

        MoneyChanged();

        return true;
    }

    private void MoneyChanged()
    {
        UpdateScoreUI();
        ScoreChanged?.Invoke(CurrentScore);
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"€ {CurrentScore}";
        }
    }
}