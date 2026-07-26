using TMPro;
using UnityEngine;

public class ItemEffectManager : MonoBehaviour
{
    public static ItemEffectManager Instance
    {
        get;
        private set;
    }

    [Header("References")]
    [SerializeField] private AdSpawner adSpawner;
    [SerializeField] private GameLoopManager gameLoopManager;

    [Header("Antivirus Settings")]
    [Min(1)]
    [SerializeField] private int antivirusCloseAmount = 5;

    [Min(1)]
    [SerializeField] private int premiumAntivirusCloseAmount = 10;

    [Header("Coffee Settings")]
    [Min(0f)]
    [SerializeField] private float coffeeTimeAmount = 30f;

    [Header("Clover Settings")]
    [Min(0)]
    [SerializeField] private int cloverRewardBonus = 1;

    [Header("Magnifier Settings")]
    [SerializeField] private float insideButtonPositionY = -36.4f;
    [SerializeField] private float magnifiedButtonHeight = 305f;
    [SerializeField] private float textLeftInset = 210f;
    [SerializeField] private float textRightInset = 210f;
    [SerializeField] private float magnifiedFontSize = 38f;

    public int AdRewardBonus { get; private set; }

    public bool MagnifierIsActive { get; private set; }

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public bool TryUseItem(ShopItemData item)
    {
        if (item == null)
        {
            return false;
        }

        switch (item.itemType)
        {
            case ShopItemType.Antivirus:
                return UseAntivirus(
                    antivirusCloseAmount
                );

            case ShopItemType.AntivirusPremium:
                return UseAntivirus(
                    premiumAntivirusCloseAmount
                );

            case ShopItemType.Clover:
                return UseClover();

            case ShopItemType.Coffee:
                return UseCoffee();

            case ShopItemType.Magnifier:
                return UseMagnifier();

            default:
                Debug.LogWarning(
                    $"No effect exists for {item.itemType}.",
                    this
                );

                return false;
        }
    }

    private bool UseAntivirus(int amount)
    {
        if (adSpawner == null)
        {
            Debug.LogError(
                "ItemEffectManager has no AdSpawner assigned.",
                this
            );

            return false;
        }

        int adsClosed =
            adSpawner.CloseLastAdsAndReward(amount);

        // Do not consume the item if there were no ads.
        return adsClosed > 0;
    }

    private bool UseClover()
    {
        /*
         * This allows multiple clovers to stack.
         * Change += to = if you only want one clover bonus.
         */
        AdRewardBonus +=
            cloverRewardBonus;

        return true;
    }

    private bool UseCoffee()
    {
        if (gameLoopManager == null)
        {
            Debug.LogError(
                "ItemEffectManager has no GameLoopManager assigned.",
                this
            );

            return false;
        }

        return gameLoopManager.AddTime(
            coffeeTimeAmount
        );
    }

    private bool UseMagnifier()
    {
        MagnifierIsActive = true;

        // Apply to normal InsideButtonAds.
        InsideButtonAd[] existingInsideButtonAds =
            FindObjectsByType<InsideButtonAd>();

        foreach (InsideButtonAd ad in existingInsideButtonAds)
        {
            ApplyMagnifierToObject(ad.gameObject);
        }

        // Apply to tutorial ads that use the same visual hierarchy.
        TutorialAd[] existingTutorialAds =
            FindObjectsByType<TutorialAd>();

        foreach (TutorialAd tutorialAd in existingTutorialAds)
        {
            ApplyMagnifierToObject(
                tutorialAd.gameObject
            );
        }

        return true;
    }

    public void ApplyActiveEffects(GameObject spawnedAd)
    {
        if (spawnedAd == null)
        {
            return;
        }

        // CloseButtonAd must never receive the magnifier effect.
        if (spawnedAd.GetComponent<CloseButtonAd>() != null)
        {
            return;
        }

        if (MagnifierIsActive)
        {
            ApplyMagnifierToObject(spawnedAd);
        }
    }

    public void ApplyMagnifierToObject(GameObject adObject)
    {
        if (adObject == null)
        {
            return;
        }

        if (adObject.GetComponent<CloseButtonAd>() != null)
        {
            return;
        }

        Transform insideButtonTransform =
            adObject.transform.Find("InsideButton");

        if (insideButtonTransform == null)
        {
            insideButtonTransform = FindChildRecursive(adObject.transform, "InsideButton");
        }

        RectTransform insideButtonRect =
            insideButtonTransform.GetComponent<RectTransform>();

        if (insideButtonRect != null)
        {
            Vector2 position =
                insideButtonRect.anchoredPosition;

            position.y =
                insideButtonPositionY;

            insideButtonRect.anchoredPosition =
                position;

            Vector2 size =
                insideButtonRect.sizeDelta;

            size.y =
                magnifiedButtonHeight;

            insideButtonRect.sizeDelta =
                size;
        }

        Transform textTransform =
            insideButtonTransform.Find("BtnText");

        if (textTransform == null)
        {
            textTransform =
                FindChildRecursive(
                    insideButtonTransform,
                    "BtnText"
                );
        }

        if (textTransform != null)
        {
            RectTransform textRect =
                textTransform.GetComponent<RectTransform>();

            if (textRect != null)
            {
                Vector2 offsetMin =
                    textRect.offsetMin;

                Vector2 offsetMax =
                    textRect.offsetMax;

                offsetMin.x =
                    textLeftInset;

                // Unity stores the right inset as a negative value.
                offsetMax.x =
                    -textRightInset;

                textRect.offsetMin =
                    offsetMin;

                textRect.offsetMax =
                    offsetMax;
            }

            TMP_Text buttonText =
                textTransform.GetComponent<TMP_Text>();

            if (buttonText != null)
            {
                buttonText.enableAutoSizing = false;
                buttonText.fontSize =
                    magnifiedFontSize;
            }
        }

        Transform closeButtonTransform =
            insideButtonTransform.Find("CloseButton");

        if (closeButtonTransform == null)
        {
            closeButtonTransform =
                FindChildRecursive(
                    insideButtonTransform,
                    "CloseButton"
                );
        }

        if (closeButtonTransform != null)
        {
            RectTransform closeButtonRect =
                closeButtonTransform.GetComponent<RectTransform>();

            if (closeButtonRect != null)
            {
                Vector2 size =
                    closeButtonRect.sizeDelta;

                size.y =
                    magnifiedButtonHeight;

                closeButtonRect.sizeDelta =
                    size;
            }
        }
    }
    private Transform FindChildRecursive(
    Transform parent,
    string childName
)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
            {
                return child;
            }

            Transform result =
                FindChildRecursive(
                    child,
                    childName
                );

            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    public void ResetShiftEffects()
    {
        AdRewardBonus = 0;
        MagnifierIsActive = false;
    }
}