using UnityEngine;

public abstract class Ad : MonoBehaviour
{
    protected abstract int RewardAmount { get; }

    protected void CloseAd()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(RewardAmount);
        }

        Destroy(gameObject);
    }
}