using System;
using UnityEngine;
using UnityEngine.UI;

public class TutorialAd : MonoBehaviour
{
    [Header("Close Button")]
    [SerializeField] private Button closeButton;

    [Header("Tutorial Settings")]
    [SerializeField] private int rewardAmount = 1;
    [SerializeField] private bool interactableAtStart = true;

    public event Action<TutorialAd> AdClosed;

    private bool hasClosed;

    private void Awake()
    {
        if (closeButton == null)
        {
            Debug.LogError(
                $"{name}: TutorialAd has no Close Button assigned.",
                this
            );

            return;
        }

        closeButton.interactable =
            interactableAtStart;

        closeButton.onClick.AddListener(
            HandleCloseClicked
        );
    }

    public void SetInteractable(bool isInteractable)
    {
        if (closeButton != null)
        {
            closeButton.interactable =
                isInteractable;
        }
    }

    private void HandleCloseClicked()
    {
        if (hasClosed)
        {
            return;
        }

        hasClosed = true;

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(
                rewardAmount
            );
        }

        AdClosed?.Invoke(this);

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(
                HandleCloseClicked
            );
        }
    }
}