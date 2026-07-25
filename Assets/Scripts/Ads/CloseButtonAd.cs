using UnityEngine;
using UnityEngine.UI;

public class CloseButtonAd : Ad
{
    protected override int RewardAmount => 2;

    [Header("Buttons")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button insideButton;

    [Header("Inside Button Penalty")]
    [Min(0)]
    [SerializeField] private int insideButtonPenalty = 1;

    private void Awake()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseAd);
        }
        else
        {
            Debug.LogWarning(
                $"{name}: CloseButton is not assigned.",
                this
            );
        }

        if (insideButton != null)
        {
            insideButton.onClick.AddListener(
                HandleInsideButtonClicked
            );
        }
        else
        {
            Debug.LogWarning(
                $"{name}: InsideButton is not assigned.",
                this
            );
        }
    }

    private void HandleInsideButtonClicked()
    {
        if (ScoreManager.Instance != null)
        {
            /*
             * TrySpendScore prevents the player's money
             * from dropping below zero.
             */
            ScoreManager.Instance.TrySpendScore(
                insideButtonPenalty
            );
        }
        else
        {
            Debug.LogWarning(
                $"{name}: No ScoreManager instance was found.",
                this
            );
        }

        /*
         * Remove the ad without awarding its normal reward.
         * This also lets the AdSpawner unregister it correctly.
         */
        ForceCloseWithoutReward();
    }

    private void OnDestroy()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(
                CloseAd
            );
        }

        if (insideButton != null)
        {
            insideButton.onClick.RemoveListener(
                HandleInsideButtonClicked
            );
        }
    }
}