using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [Header("Available Items")]
    [SerializeField] private List<ShopItemData> allItems =
        new List<ShopItemData>();

    [Header("Shop Placements")]
    [SerializeField] private ShopItemPlacement[] itemPlacements;

    [Header("Selected Item Display")]
    [SerializeField] private TMP_Text selectedItemNameText;
    [SerializeField] private TMP_Text selectedItemDescriptionText;

    [Header("Buy Button")]
    [SerializeField] private Button buyButton;
    [SerializeField] private TMP_Text buyButtonText;

    private ShopItemPlacement selectedPlacement;

    private void Start()
    {
        if (buyButton != null)
        {
            buyButton.onClick.AddListener(BuySelectedItem);
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ScoreChanged += HandleMoneyChanged;
        }

        GenerateShop();
    }

    private void GenerateShop()
    {
        if (allItems.Count < itemPlacements.Length)
        {
            Debug.LogError(
                "There are not enough shop items for all item placements.",
                this
            );

            return;
        }

        List<ShopItemData> availableItems =
            new List<ShopItemData>(allItems);

        // Shuffle the item list.
        for (int i = availableItems.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            ShopItemData temporaryItem =
                availableItems[i];

            availableItems[i] =
                availableItems[randomIndex];

            availableItems[randomIndex] =
                temporaryItem;
        }

        // Assign the first three shuffled items.
        for (int i = 0; i < itemPlacements.Length; i++)
        {
            itemPlacements[i].Initialize(
                availableItems[i],
                this
            );
        }

        // Automatically select the first item.
        if (itemPlacements.Length > 0)
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

        if (selectedItemDescriptionText != null)
        {
            selectedItemDescriptionText.text =
                item.description;
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

        ShopItemData selectedItem =
            selectedPlacement.ItemData;

        bool purchaseSuccessful =
            ScoreManager.Instance.TrySpendScore(
                selectedItem.price
            );

        if (!purchaseSuccessful)
        {
            // Show that player cannot afford the item.
            return;
        }

        selectedPlacement.MarkAsPurchased();

        ApplyPurchasedItem(selectedItem);
        UpdateBuyButton();
    }

    private void ApplyPurchasedItem(
        ShopItemData purchasedItem
    )
    {
        /*
         * Add the actual item effect here later.
         *
         * Examples:
         * - Make close buttons larger
         * - Slow down ad spawning
         * - Increase rewards
         * - Automatically close an ad
         */

        Debug.Log(
            $"Purchased item: {purchasedItem.itemName}"
        );
    }

    private void HandleMoneyChanged(int newAmount)
    {
        UpdateBuyButton();
    }

    private void UpdateBuyButton()
    {
        if (buyButton == null)
            return;

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

        bool canAfford =
            ScoreManager.Instance != null &&
            ScoreManager.Instance.CanAfford(
                selectedPlacement.ItemData.price
            );

        buyButton.interactable = canAfford;
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
}