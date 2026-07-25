using System;
using UnityEngine;

public abstract class Ad : MonoBehaviour
{
    protected abstract int RewardAmount { get; }

    public int BaseRewardAmount => RewardAmount;
    public bool HasBeenClosed { get; private set; }

    public event Action<Ad> AdClosed;

    protected void CloseAd()
    {
        ResolveAd(true);
    }

    public void ForceCloseAndReward()
    {
        ResolveAd(true);
    }

    public void ForceCloseWithoutReward()
    {
        ResolveAd(false);
    }

    private void ResolveAd(bool giveReward)
    {
        if (HasBeenClosed)
        {
            return;
        }

        HasBeenClosed = true;

        if (giveReward &&
            ScoreManager.Instance != null)
        {
            int bonusReward = 0;

            if (ItemEffectManager.Instance != null)
            {
                bonusReward =
                    ItemEffectManager.Instance.AdRewardBonus;
            }

            ScoreManager.Instance.AddScore(
                RewardAmount + bonusReward
            );
        }

        AdClosed?.Invoke(this);

        Destroy(gameObject);
    }
}