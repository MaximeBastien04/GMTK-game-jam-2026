using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    [Header("Dave UI")]
    [SerializeField] private GameObject davePopup;
    [SerializeField] private TMP_Text daveSpeechText;
    [SerializeField] private Button confirmButton;

    [Header("Main UI")]
    [SerializeField] private Button playButton;
    [SerializeField] private Sprite playButtonDisabledSprite;
    [SerializeField] private Image playButtonImage;

    [Header("Shop")]
    [SerializeField] private GameObject shopObject;
    [SerializeField] private Button shopButton;
    [SerializeField] private ShopManager shopManager;
    [SerializeField] private ShopItemData magnifierItem;

    [Header("Inventory")]
    [SerializeField] private InventoryManager inventoryManager;

    [Header("Ad Area")]
    [SerializeField] private RectTransform adContainer;

    [Header("Tutorial Ad Prefabs")]
    [SerializeField] private TutorialAd firstRewardAdPrefab;
    [SerializeField] private TutorialAd lockedAdPrefab;
    [SerializeField] private FalseAdvertisementAd falseAdPrefab;

    [Header("Tutorial Settings")]
    [SerializeField] private float initialPopupDelay = 2f;
    [SerializeField] private int lockedAdAmount = 3;

    [Header("Typewriter Settings")]
    [SerializeField] private float characterDelay = 0.03f;
    [SerializeField] private bool allowSkipTyping = true;

    private bool dialogueConfirmed;
    private bool shopOpened;
    private bool magnifierPurchased;
    private bool magnifierUsed;

    private int lockedAdsRemaining;

    private Coroutine tutorialRoutine;

    private Coroutine typewriterRoutine;
    private bool isTyping;
    private string currentDialogue;

    public void BeginTutorial()
    {
        if (tutorialRoutine != null)
        {
            return;
        }

        tutorialRoutine =
            StartCoroutine(TutorialSequence());
    }

    private IEnumerator TutorialSequence()
    {
        PrepareTutorial();

        yield return new WaitForSeconds(
            initialPopupDelay
        );

        SetDaveVisible(true);

        yield return ShowDialogue(
            "Hi, I'm Dave. I'm the new coach."
        );

        yield return ShowDialogue(
            "I think you can use a little practice."
        );

        SetDaveVisible(false);
        TutorialAd firstAd =
            SpawnTutorialAd(firstRewardAdPrefab);

        if (firstAd == null)
        {
            Debug.LogError(
                "The first tutorial ad could not be spawned.",
                this
            );

            yield break;
        }

        bool firstAdClosed = false;

        void HandleFirstAdClosed(
            TutorialAd closedAd
        )
        {
            firstAdClosed = true;

            closedAd.AdClosed -=
                HandleFirstAdClosed;
        }

        // Subscribe immediately, before showing the instruction.
        firstAd.AdClosed +=
            HandleFirstAdClosed;

        yield return null;

        // Start showing the instruction, but do not force the player
        // to press OK before being allowed to close the ad.
        BeginDialogue(
            "Click on the close button on the ad to earn some money."
        );

        // Wait until either:
        // 1. The player confirms Dave's message.
        // 2. The player closes the ad first.
        yield return new WaitUntil(
            () => dialogueConfirmed || firstAdClosed
        );

        if (firstAdClosed)
        {
            /*
             * The player closed the ad while Dave was still visible.
             * Replace the current instruction directly with the
             * congratulations dialogue.
             */
            dialogueConfirmed = false;

            yield return ShowDialogue(
                "Great job! You earned 20 euros."
            );
        }
        else
        {
            /*
             * The player pressed OK first.
             * Hide Dave and wait for the player to close the ad.
             */
            SetDaveVisible(false);

            yield return new WaitUntil(
                () => firstAdClosed
            );

            // Bring Dave back after the ad closes.
            yield return ShowDialogue(
                "Great job! You earned 20 euros."
            );
        }

        yield return ShowDialogue(
            "You should buy an item to make your work easier."
        );

        yield return ShowDialogue(
            "Click the shop icon to open up the shop."
        );

        SetDaveVisible(false);

        shopButton.interactable = true;

        yield return new WaitUntil(
            () => shopOpened
        );

        OpenTutorialShop();

        yield return new WaitUntil(
            () => magnifierPurchased
        );

        CloseTutorialShop();

        SpawnLockedAds();

        yield return ShowDialogue(
            "Try using the item you just bought. " +
            "It will make your job easier."
        );

        SetDaveVisible(false);

        yield return new WaitUntil(
            () => magnifierUsed
        );

        UnlockTutorialAds();

        yield return new WaitUntil(
            () => lockedAdsRemaining <= 0
        );

        yield return ShowDialogue(
            "The more ads you close, the more money you get."
        );

        yield return ShowDialogue(
            "But be wary of false advertisement."
        );

        SetDaveVisible(false);

        FalseAdvertisementAd falseAd =
            SpawnFalseAdvertisement();

        bool falseAdClicked = false;

        if (falseAd != null)
        {
            falseAd.FalseAdvertisementClicked +=
                HandleFalseAdClicked;
        }

        void HandleFalseAdClicked()
        {
            falseAdClicked = true;

            if (falseAd != null)
            {
                falseAd.FalseAdvertisementClicked -=
                    HandleFalseAdClicked;
            }
        }

        yield return new WaitUntil(
            () => falseAdClicked
        );

        yield return ShowDialogue(
            "Alright, you get the gist of it."
        );

        yield return ShowDialogue(
            "I heard you're going on a trip at the end of the month."
        );

        yield return ShowDialogue(
            "Those tickets are very expensive. " +
            "So you better start working for it."
        );

        yield return ShowDialogue(
            "Have a nice shift!"
        );

        FinishTutorial();
    }

    private void PrepareTutorial()
    {
        SetDaveVisible(false);
        SetObjectActive(shopObject, false);

        shopOpened = false;
        magnifierPurchased = false;
        magnifierUsed = false;

        if (shopButton != null)
        {
            shopButton.interactable = false;
            shopButton.onClick.AddListener(
                HandleShopButtonClicked
            );
        }

        if (shopManager != null)
        {
            shopManager.ItemPurchased +=
                HandleItemPurchased;
        }

        if (inventoryManager != null)
        {
            inventoryManager.ItemUsed +=
                HandleItemUsed;
        }

        if (playButton != null)
        {
            playButton.interactable = false;
        }

        if (playButtonImage != null &&
            playButtonDisabledSprite != null)
        {
            playButtonImage.sprite =
                playButtonDisabledSprite;
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(
                ConfirmDialogue
            );
        }
    }

    private IEnumerator ShowDialogue(string dialogue)
    {
        BeginDialogue(dialogue);

        yield return new WaitUntil(
            () => dialogueConfirmed
        );
    }

    private IEnumerator TypeDialogue(string dialogue)
    {
        isTyping = true;

        if (daveSpeechText != null)
        {
            daveSpeechText.text = string.Empty;

            foreach (char character in dialogue)
            {
                daveSpeechText.text += character;

                yield return new WaitForSeconds(
                    characterDelay
                );
            }
        }

        isTyping = false;
        typewriterRoutine = null;
    }

    private void ConfirmDialogue()
    {
        if (isTyping)
        {
            if (!allowSkipTyping)
            {
                return;
            }

            if (typewriterRoutine != null)
            {
                StopCoroutine(typewriterRoutine);
                typewriterRoutine = null;
            }

            if (daveSpeechText != null)
            {
                daveSpeechText.text =
                    currentDialogue;
            }

            isTyping = false;

            if (confirmButton != null)
            {
                confirmButton.interactable = true;
            }

            return;
        }

        dialogueConfirmed = true;
    }

    private void BeginDialogue(string dialogue)
    {
        dialogueConfirmed = false;
        currentDialogue = dialogue;

        SetDaveVisible(true);

        if (confirmButton != null)
        {
            confirmButton.interactable = true;
        }

        if (typewriterRoutine != null)
        {
            StopCoroutine(typewriterRoutine);
            typewriterRoutine = null;
        }

        typewriterRoutine =
            StartCoroutine(
                TypeDialogue(dialogue)
            );
    }

    private void HandleShopButtonClicked()
    {
        if (!shopButton.interactable)
        {
            return;
        }

        shopOpened = true;
        shopButton.interactable = false;
    }

    private void OpenTutorialShop()
    {
        SetObjectActive(shopObject, true);

        if (shopManager != null)
        {
            shopManager.GenerateTutorialShop(
                magnifierItem
            );
        }
    }

    private void CloseTutorialShop()
    {
        SetObjectActive(shopObject, false);
    }

    private void HandleItemPurchased(
        ShopItemData purchasedItem
    )
    {
        if (purchasedItem == magnifierItem)
        {
            magnifierPurchased = true;
        }
    }

    private void HandleItemUsed(
        ShopItemData usedItem
    )
    {
        if (usedItem == magnifierItem)
        {
            magnifierUsed = true;
        }
    }

    private TutorialAd SpawnTutorialAd(TutorialAd prefab)
    {
        if (prefab == null ||
            adContainer == null)
        {
            return null;
        }

        TutorialAd newAd =
            Instantiate(
                prefab,
                adContainer,
                false
            );

        RectTransform adRect =
            newAd.GetComponent<RectTransform>();

        if (adRect != null)
        {
            adRect.anchoredPosition =
                Vector2.zero;
        }
        
        if (ItemEffectManager.Instance != null)
        {
            ItemEffectManager.Instance.ApplyActiveEffects(
                newAd.gameObject
            );
        }

        return newAd;
    }

    private void SpawnLockedAds()
    {
        lockedAdsRemaining =
            lockedAdAmount;

        for (int i = 0;
             i < lockedAdAmount;
             i++)
        {
            TutorialAd newAd =
                SpawnTutorialAd(lockedAdPrefab);

            if (newAd == null)
            {
                lockedAdsRemaining--;
                continue;
            }

            newAd.SetInteractable(false);

            RectTransform adRect =
                newAd.GetComponent<RectTransform>();

            if (adRect != null)
            {
                float horizontalOffset =
                    (i - 1) * 150f;

                adRect.anchoredPosition =
                    new Vector2(
                        horizontalOffset,
                        0f
                    );
            }

            newAd.AdClosed +=
                HandleLockedAdClosed;
        }
    }

    private void UnlockTutorialAds()
    {
        TutorialAd[] tutorialAds =
            adContainer.GetComponentsInChildren
                <TutorialAd>(true);

        foreach (TutorialAd tutorialAd in tutorialAds)
        {
            tutorialAd.SetInteractable(true);
        }
    }

    private void HandleLockedAdClosed(
        TutorialAd closedAd
    )
    {
        closedAd.AdClosed -=
            HandleLockedAdClosed;

        lockedAdsRemaining--;
    }

    private FalseAdvertisementAd
        SpawnFalseAdvertisement()
    {
        if (falseAdPrefab == null ||
            adContainer == null)
        {
            return null;
        }

        FalseAdvertisementAd newAd =
            Instantiate(
                falseAdPrefab,
                adContainer,
                false
            );

        RectTransform adRect =
            newAd.GetComponent<RectTransform>();

        if (adRect != null)
        {
            adRect.anchoredPosition =
                Vector2.zero;
        }

        return newAd;
    }

    private void FinishTutorial()
    {
        SetDaveVisible(false);

        RemoveListeners();

        tutorialRoutine = null;

        GameLoopManager gameLoop = FindAnyObjectByType<GameLoopManager>();

        if (gameLoop != null)
        {
            gameLoop.CompleteTutorial();
        }
    }

    private void SetDaveVisible(bool visible)
    {
        SetObjectActive(davePopup, visible);
    }

    private static void SetObjectActive(
        GameObject target,
        bool active
    )
    {
        if (target != null)
        {
            target.SetActive(active);
        }
    }

    private void RemoveListeners()
    {
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(
                ConfirmDialogue
            );
        }

        if (shopButton != null)
        {
            shopButton.onClick.RemoveListener(
                HandleShopButtonClicked
            );
        }

        if (shopManager != null)
        {
            shopManager.ItemPurchased -=
                HandleItemPurchased;
        }

        if (inventoryManager != null)
        {
            inventoryManager.ItemUsed -=
                HandleItemUsed;
        }
    }

    private void OnDestroy()
    {
        RemoveListeners();
    }
}