using UnityEngine;
using UnityEngine.UI;

public class CloseButtonAd : Ad
{
    protected override int RewardAmount => 2;

    [SerializeField] private Button closeButton;

    private void Awake()
    {
        closeButton.onClick.AddListener(CloseAd);
    }

    private void OnDestroy()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(CloseAd);
    }
}