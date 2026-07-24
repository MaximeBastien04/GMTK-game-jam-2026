using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemPlacement : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text itemPriceText;
    [SerializeField] private Image itemSprite;
    [SerializeField] private Button selectionButton;

    [Header("Optional")]
    [SerializeField] private GameObject soldOverlay;

    public ShopItemData ItemData { get; private set; }
    public bool IsPurchased { get; private set; }
    public Image ItemImage => itemSprite;

    private ShopManager shopManager;

    private void Awake()
    {
        if (selectionButton != null)
        {
            selectionButton.onClick.AddListener(
                SelectThisItem
            );
        }
    }

    public void Initialize(
        ShopItemData newItemData,
        ShopManager manager
    )
    {
        ItemData = newItemData;
        shopManager = manager;
        IsPurchased = false;

        if (itemNameText != null)
        {
            itemNameText.text =
                ItemData.itemName;
        }

        if (itemPriceText != null)
        {
            itemPriceText.text =
                $"€ {ItemData.price}";
        }

        if (itemSprite != null)
        {
            itemSprite.sprite =
                ItemData.sprite;

            itemSprite.enabled =
                ItemData.sprite != null;

            SetItemOpacity(1f);
        }

        if (selectionButton != null)
        {
            selectionButton.interactable = true;
        }

        if (soldOverlay != null)
        {
            soldOverlay.SetActive(false);
        }
    }

    public void MarkAsPurchased()
    {
        IsPurchased = true;

        // Hide the shop item without disabling its GameObject.
        SetItemOpacity(0f);

        if (selectionButton != null)
        {
            selectionButton.interactable = false;
        }

        if (soldOverlay != null)
        {
            soldOverlay.SetActive(true);
        }
    }

    private void SetItemOpacity(float opacity)
    {
        if (itemSprite == null)
        {
            return;
        }

        Color color = itemSprite.color;
        color.a = opacity;
        itemSprite.color = color;
    }

    private void SelectThisItem()
    {
        if (shopManager == null ||
            ItemData == null)
        {
            return;
        }

        shopManager.SelectItem(this);
    }

    private void OnDestroy()
    {
        if (selectionButton != null)
        {
            selectionButton.onClick.RemoveListener(
                SelectThisItem
            );
        }
    }
}