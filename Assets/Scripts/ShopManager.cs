using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ShopManager : MonoBehaviour
{
    [Header("Available Items")]
    [SerializeField]
    private List<ShopItemData> allItems =
        new List<ShopItemData>();

    [Header("Shop Placements")]
    [SerializeField] private ShopItemPlacement[] itemPlacements;

    [Header("Selected Item Display")]
    [SerializeField] private TMP_Text selectedItemNameText;
    [SerializeField] private TMP_Text selectedItemDescriptionText;
    private string currentItemDescription;

    [Header("Buy Button")]
    [SerializeField] private Button buyButton;
    [SerializeField] private TMP_Text buyButtonText;

    private ShopItemPlacement selectedPlacement;

    [Header("Inventory")]
    [SerializeField] private InventoryManager inventoryManager;

    public event Action<ShopItemData> ItemPurchased;


    [Header("Item buy Sound")]
    [SerializeField] private AudioClip buySFX;

    [Range(0f, 1f)]
    [SerializeField] private float buySFXVolume = 1f;

    [SerializeField]
    private Vector2 buySFXPitchRange =
        new Vector2(0.95f, 1.05f);

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (buyButton != null)
        {
            buyButton.onClick.AddListener(BuySelectedItem);
        }
    }

    private void Start()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ScoreChanged += HandleMoneyChanged;
        }
    }

    public void RefreshShop()
    {
        selectedPlacement = null;
        currentItemDescription = string.Empty;

        GenerateShop();
    }

    private void GenerateShop()
    {
        for (int i = 0; i < itemPlacements.Length; i++)
        {
            if (itemPlacements[i] != null)
            {
                itemPlacements[i].gameObject.SetActive(true);
            }
        }

        if (allItems == null ||
            allItems.Count == 0)
        {
            Debug.LogError(
                "ShopManager has no items assigned to All Items.",
                this
            );

            return;
        }

        if (itemPlacements == null ||
            itemPlacements.Length == 0)
        {
            Debug.LogError(
                "ShopManager has no Item Placements assigned.",
                this
            );

            return;
        }

        if (allItems.Count < itemPlacements.Length)
        {
            Debug.LogError(
                $"ShopManager needs at least {itemPlacements.Length} items, " +
                $"but only {allItems.Count} are assigned.",
                this
            );

            return;
        }

        List<ShopItemData> availableItems =
            new List<ShopItemData>();

        // Ignore empty item entries.
        foreach (ShopItemData item in allItems)
        {
            if (item != null)
            {
                availableItems.Add(item);
            }
        }

        if (availableItems.Count < itemPlacements.Length)
        {
            Debug.LogError(
                "Not enough valid ShopItemData assets are assigned.",
                this
            );

            return;
        }

        // Shuffle the available items.
        for (int i = availableItems.Count - 1; i > 0; i--)
        {
            int randomIndex =
                UnityEngine.Random.Range(0, i + 1);

            ShopItemData temporaryItem =
                availableItems[i];

            availableItems[i] =
                availableItems[randomIndex];

            availableItems[randomIndex] =
                temporaryItem;
        }

        // Reset and populate every shop placement.
        for (int i = 0; i < itemPlacements.Length; i++)
        {
            if (itemPlacements[i] == null)
            {
                Debug.LogError(
                    $"Item Placement element {i} is not assigned.",
                    this
                );

                continue;
            }

            itemPlacements[i].Initialize(
                availableItems[i],
                this
            );
        }

        // Automatically select the first item.
        if (itemPlacements[0] != null)
        {
            SelectItem(itemPlacements[0]);
        }
    }

    public void SelectItem(
        ShopItemPlacement placement
    )
    {
        if (placement == null ||
            placement.ItemData == null)
        {
            return;
        }

        selectedPlacement = placement;

        ShopItemData item =
            selectedPlacement.ItemData;

        if (selectedItemNameText != null)
        {
            selectedItemNameText.text =
                item.itemName;
        }

        currentItemDescription = item.description;

        if (selectedItemDescriptionText != null)
        {
            selectedItemDescriptionText.text =
                currentItemDescription;
        }

        UpdateBuyButton();
    }

    private void BuySelectedItem()
    {
        if (selectedPlacement == null ||
            selectedPlacement.ItemData == null)
        {
            return;
        }

        if (selectedPlacement.IsPurchased)
        {
            return;
        }

        if (ScoreManager.Instance == null)
        {
            Debug.LogError(
                "No ScoreManager instance was found.",
                this
            );

            return;
        }

        if (inventoryManager == null)
        {
            Debug.LogError(
                "ShopManager has no InventoryManager assigned.",
                this
            );

            return;
        }

        if (!inventoryManager.HasAvailableSlot)
        {
            ShowShopMessage("Your inventory is full.");
            return;
        }

        ShopItemData selectedItem =
            selectedPlacement.ItemData;

        if (!ScoreManager.Instance.CanAfford(selectedItem.price))
        {
            ShowShopMessage("You don't have enough money.");
            return;
        }

        /*
         * Add the item before spending money.
         * This prevents money from being removed if adding fails.
         */
        bool itemAdded =
            inventoryManager.TryAddItem(selectedItem);

        if (!itemAdded)
        {
            Debug.Log("Could not add item to inventory.");
            return;
        }

        bool purchaseSuccessful =
            ScoreManager.Instance.TrySpendScore(
                selectedItem.price
            );

        PlayBuySFX();

        selectedPlacement.MarkAsPurchased();

        ItemPurchased?.Invoke(selectedItem);

        UpdateBuyButton();

        ShowDescription();

        if (!purchaseSuccessful)
        {
            Debug.LogError(
                "The item was added, but payment failed.",
                this
            );

            return;
        }

        selectedPlacement.MarkAsPurchased();

        UpdateBuyButton();
    }

    public void GenerateTutorialShop(
    ShopItemData tutorialItem
)
    {
        if (tutorialItem == null)
        {
            Debug.LogError(
                "No tutorial item was provided.",
                this
            );

            return;
        }

        selectedPlacement = null;
        currentItemDescription =
            string.Empty;

        for (int i = 0; i < itemPlacements.Length; i++)
        {
            if (itemPlacements[i] == null)
            {
                continue;
            }

            bool shouldShowItem = i == 0;

            itemPlacements[i].gameObject.SetActive(
                shouldShowItem
            );

            if (shouldShowItem)
            {
                itemPlacements[i].Initialize(
                    tutorialItem,
                    this
                );
            }
        }

        if (itemPlacements.Length > 0 &&
            itemPlacements[0] != null)
        {
            SelectItem(itemPlacements[0]);
        }
    }

    private void HandleMoneyChanged(int newAmount)
    {
        UpdateBuyButton();
    }

    private void UpdateBuyButton()
    {
        if (buyButton == null)
        {
            return;
        }

        if (selectedPlacement == null ||
            selectedPlacement.ItemData == null)
        {
            buyButton.interactable = false;

            if (buyButtonText != null)
            {
                buyButtonText.text = "BUY";
            }

            return;
        }

        if (selectedPlacement.IsPurchased)
        {
            buyButton.interactable = false;

            if (buyButtonText != null)
            {
                buyButtonText.text = "SOLD";
            }

            return;
        }

        // An unpurchased item is selected, so the button is clickable.
        buyButton.interactable = true;

        if (buyButtonText != null)
        {
            buyButtonText.text = "BUY";
        }
    }

    private void ShowDescription()
    {
        if (selectedItemDescriptionText != null)
        {
            selectedItemDescriptionText.text =
                currentItemDescription;
        }
    }

    private void ShowShopMessage(string message)
    {
        if (selectedItemDescriptionText != null)
        {
            selectedItemDescriptionText.text = message;
        }
    }

    private void OnDestroy()
    {
        if (buyButton != null)
        {
            buyButton.onClick.RemoveListener(
                BuySelectedItem
            );
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ScoreChanged -=
                HandleMoneyChanged;
        }
    }

    private void PlayBuySFX()
    {
        if (buySFX != null)
        {
            audioSource.PlayOneShot(buySFX, buySFXVolume);
            audioSource.pitch = UnityEngine.Random.Range(
                buySFXPitchRange.x,
                buySFXPitchRange.y
            );
        }
    }
}