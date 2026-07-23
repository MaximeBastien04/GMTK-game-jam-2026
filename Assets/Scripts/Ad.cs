using UnityEngine;
using UnityEngine.UI;

public class Ad : MonoBehaviour
{
    [Header("Ad References")]
    [SerializeField] private Button closeButton;

    [Header("Reward")]
    [SerializeField] private int rewardAmount = 5;

    private void Start()
    {
        closeButton.onClick.AddListener(CloseAd);
    }

    private void CloseAd()
    {
        ScoreManager.Instance.AddScore(rewardAmount);

        Destroy(gameObject);
    }
}