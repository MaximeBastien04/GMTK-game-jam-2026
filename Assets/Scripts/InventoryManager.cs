using UnityEngine;
using UnityEngine.UI;
using System;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Inventory Slots")]
    [Tooltip(
        "Assign the Item Image from each of the four ItemHolder objects."
    )]
    [SerializeField] private Image[] inventoryItemImages;

    public int Capacity => inventoryItemImages.Length;
    public int ItemCount { get; private set; }

    public bool HasAvailableSlot =>
        ItemCount < inventoryItemImages.Length;

    private ShopItemData[] inventoryItems;

    public event Action<ShopItemData> ItemUsed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        inventoryItems =
            new ShopItemData[inventoryItemImages.Length];

        InitializeSlots();
    }

    private void InitializeSlots()
    {
        for (int i = 0; i < inventoryItemImages.Length; i++)
        {
            Image itemImage = inventoryItemImages[i];

            if (itemImage == null)
            {
                Debug.LogWarning(
                    $"Inventory slot {i} has no Item Image assigned.",
                    this
                );

                continue;
            }

            itemImage.sprite = null;
            SetImageOpacity(itemImage, 0f);
            itemImage.raycastTarget = false;
        }

        ItemCount = 0;
    }

    public bool TryAddItem(ShopItemData item)
    {
        if (item == null)
        {
            return false;
        }

        for (int i = 0; i < inventoryItems.Length; i++)
        {
            if (inventoryItems[i] != null)
            {
                continue;
            }

            inventoryItems[i] = item;

            Image itemImage =
                inventoryItemImages[i];

            if (itemImage != null)
            {
                itemImage.sprite = item.sprite;
                SetImageOpacity(itemImage, 1f);
            }

            ItemCount++;

            Debug.Log(
                $"Added {item.itemName} to inventory slot {i + 1}."
            );

            return true;
        }

        Debug.Log("Inventory is full.");
        return false;
    }

    public void UseItem(int slotIndex)
    {
        if (slotIndex < 0 ||
            slotIndex >= inventoryItems.Length)
        {
            return;
        }

        ShopItemData item =
            inventoryItems[slotIndex];

        if (item == null)
        {
            return;
        }

        ItemUsed?.Invoke(item);

        /*
         * For now the item remains in the inventory.
         * Later, you can consume it here if items
         * should only be usable once.
         */
    }

    public ShopItemData GetItem(int slotIndex)
    {
        if (slotIndex < 0 ||
            slotIndex >= inventoryItems.Length)
        {
            return null;
        }

        return inventoryItems[slotIndex];
    }

    private static void SetImageOpacity(
        Image image,
        float opacity
    )
    {
        Color color = image.color;
        color.a = opacity;
        image.color = color;
    }
}