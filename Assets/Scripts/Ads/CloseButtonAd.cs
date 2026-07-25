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

    [Header("Advertisement")]
    [SerializeField] private Image adImage;

    [SerializeField] private Sprite[] advertisementSprites;

    private void Awake()
    {
        RandomizeAdvertisement();

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseAd);
        }

        if (insideButton != null)
        {
            insideButton.onClick.AddListener(
                HandleInsideButtonClicked
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

    private void RandomizeAdvertisement()
    {
        if (adImage == null)
        {
            Debug.LogWarning(
                $"{name}: Ad Image is not assigned.",
                this
            );

            return;
        }

        if (advertisementSprites == null ||
            advertisementSprites.Length == 0)
        {
            Debug.LogWarning(
                $"{name}: No advertisement sprites have been assigned.",
                this
            );

            return;
        }

        adImage.sprite =
            advertisementSprites[
                Random.Range(
                    0,
                    advertisementSprites.Length
                )
            ];
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