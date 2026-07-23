using UnityEngine;
using UnityEngine.UI;

public class InsideButtonAd : Ad
{
    protected override int RewardAmount => 2;

    [SerializeField] private Button insideButton;

    private void Awake()
    {
        insideButton.onClick.AddListener(CloseAd);
    }

    private void OnDestroy()
    {
        if (insideButton != null)
            insideButton.onClick.RemoveListener(CloseAd);
    }
}