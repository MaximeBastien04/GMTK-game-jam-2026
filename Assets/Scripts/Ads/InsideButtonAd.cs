using UnityEngine;
using UnityEngine.UI;

public class InsideButtonAd : Ad
{
    protected override int RewardAmount => 2;

    [Header("References")]
    [SerializeField] private Button insideButton;

    private RectTransform insideButtonRect;

    private void Awake()
    {
        if (insideButton == null)
        {
            Debug.LogError(
                $"{name}: Inside button is not assigned.",
                this
            );

            return;
        }

        insideButtonRect =
            insideButton.GetComponent<RectTransform>();

        insideButton.onClick.AddListener(
            CloseAd
        );
    }

    public void ApplyMagnifier(
        Vector2 newSize,
        Vector2 newPosition
    )
    {
        if (insideButtonRect == null &&
            insideButton != null)
        {
            insideButtonRect =
                insideButton.GetComponent<RectTransform>();
        }

        if (insideButtonRect == null)
        {
            return;
        }

        insideButtonRect.sizeDelta =
            newSize;

        insideButtonRect.anchoredPosition =
            newPosition;
    }

    private void OnDestroy()
    {
        if (insideButton != null)
        {
            insideButton.onClick.RemoveListener(
                CloseAd
            );
        }
    }
}