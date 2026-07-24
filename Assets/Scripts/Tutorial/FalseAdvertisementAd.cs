using System;
using UnityEngine;
using UnityEngine.UI;

public class FalseAdvertisementAd : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button fakeCloseButton;
    [SerializeField] private Button buyButton;

    public event Action FalseAdvertisementClicked;

    private bool hasBeenClicked;

    private void Awake()
    {
        if (fakeCloseButton != null)
        {
            fakeCloseButton.interactable = false;
        }

        if (buyButton != null)
        {
            buyButton.onClick.AddListener(
                HandleBuyClicked
            );
        }
    }

    private void HandleBuyClicked()
    {
        if (hasBeenClicked)
        {
            return;
        }

        hasBeenClicked = true;

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.SetScore(0);
        }

        FalseAdvertisementClicked?.Invoke();

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (buyButton != null)
        {
            buyButton.onClick.RemoveListener(
                HandleBuyClicked
            );
        }
    }
}