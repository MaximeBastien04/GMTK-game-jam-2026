using UnityEngine;
using UnityEngine.UI;
using System;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Inventory Slots")]
    [SerializeField] private Image[] inventoryItemImages;
    [SerializeField] private Button[] inventoryItemButtons;

    public int Capacity => inventoryItemImages.Length;
    public int ItemCount { get; private set; }

    public bool HasAvailableSlot =>
        ItemCount < inventoryItemImages.Length;

    private ShopItemData[] inventoryItems;

    public event Action<ShopItemData> ItemUsed;

    [Header("Item use Sound")]
    [SerializeField] private AudioClip useItemSFX;

    [Range(0f, 1f)]
    [SerializeField] private float useItemSFXVolume = 1f;

    [SerializeField]
    private Vector2 useItemSFXPitchRange =
        new Vector2(0.95f, 1.05f);

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        inventoryItems =
            new ShopItemData[inventoryItemImages.Length];

        InitializeSlots();
        RegisterButtonListeners();
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

            if (inventoryItemButtons != null &&
                i < inventoryItemButtons.Length &&
                inventoryItemButtons[i] != null)
            {
                inventoryItemButtons[i].interactable = false;
            }
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

            if (inventoryItemButtons != null && i < inventoryItemButtons.Length && inventoryItemButtons[i] != null)
            {
                inventoryItemButtons[i].interactable = true;
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

    public bool UseItem(int slotIndex)
    {
        if (slotIndex < 0 ||
            slotIndex >= inventoryItems.Length)
        {
            return false;
        }

        ShopItemData item =
            inventoryItems[slotIndex];

        if (item == null)
        {
            return false;
        }

        if (ItemEffectManager.Instance == null)
        {
            Debug.LogError(
                "No ItemEffectManager instance was found.",
                this
            );

            return false;
        }

        bool itemUsed = ItemEffectManager.Instance.TryUseItem(item);

        PlayUseItemSFX();

        if (!itemUsed)
        {
            return false;
        }

        // This still allows the tutorial to detect magnifier usage.
        ItemUsed?.Invoke(item);

        RemoveItemFromSlot(slotIndex);

        return true;
    }

    private void RemoveItemFromSlot(int slotIndex)
    {
        inventoryItems[slotIndex] = null;

        Image itemImage =
            inventoryItemImages[slotIndex];

        if (itemImage != null)
        {
            itemImage.sprite = null;

            Color color =
                itemImage.color;

            color.a = 0f;

            itemImage.color =
                color;
        }

        if (inventoryItemButtons != null &&
    slotIndex < inventoryItemButtons.Length &&
    inventoryItemButtons[slotIndex] != null)
        {
            inventoryItemButtons[slotIndex].interactable = false;
        }

        ItemCount =
            Mathf.Max(0, ItemCount - 1);
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

    private void RegisterButtonListeners()
    {
        if (inventoryItemButtons == null)
        {
            Debug.LogError(
                "Inventory item buttons array is not assigned.",
                this
            );

            return;
        }

        if (inventoryItemButtons.Length != inventoryItems.Length)
        {
            Debug.LogError(
                "The number of inventory buttons must match " +
                "the number of inventory item images.",
                this
            );

            return;
        }

        for (int i = 0; i < inventoryItemButtons.Length; i++)
        {
            Button button = inventoryItemButtons[i];

            if (button == null)
            {
                Debug.LogWarning(
                    $"Inventory slot {i} has no ItemButton assigned.",
                    this
                );

                continue;
            }

            int capturedIndex = i;

            button.onClick.AddListener(
                () => UseItem(capturedIndex)
            );
        }
    }

    private void OnDestroy()
    {
        if (inventoryItemButtons == null)
        {
            return;
        }

        foreach (Button button in inventoryItemButtons)
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
            }
        }
    }

    private void PlayUseItemSFX()
    {
        if (useItemSFX != null)
        {
            audioSource.PlayOneShot(useItemSFX, useItemSFXVolume);
            audioSource.pitch = UnityEngine.Random.Range(
                useItemSFXPitchRange.x,
                useItemSFXPitchRange.y
            );
        }
    }
}