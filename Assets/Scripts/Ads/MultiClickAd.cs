using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MultiClickAd : Ad
{
    protected override int RewardAmount => 3;

    [SerializeField] private Button clickButton;
    [SerializeField] private TMP_Text clickCounterText;
    [SerializeField] private int clicksRequired = 5;

    private int clicksRemaining;

    private void Awake()
    {
        clicksRemaining = clicksRequired;
        UpdateCounter();

        clickButton.onClick.AddListener(HandleClick);
    }

    private void HandleClick()
    {
        clicksRemaining--;

        UpdateCounter();

        if (clicksRemaining <= 0)
        {
            CloseAd();
        }
    }

    private void UpdateCounter()
    {
        if (clickCounterText != null)
        {
            clickCounterText.text = clicksRemaining.ToString();
        }
    }

    private void OnDestroy()
    {
        if (clickButton != null)
            clickButton.onClick.RemoveListener(HandleClick);
    }
}