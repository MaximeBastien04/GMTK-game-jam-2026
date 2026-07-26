using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlotHover :
    MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("Slot")]
    [Min(0)]
    [SerializeField] private int slotIndex;

    [Header("Information Text")]
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text itemDescriptionText;

    [Header("Empty Text")]
    [SerializeField] private string defaultName = "";
    [TextArea]
    [SerializeField] private string defaultDescription = "";

    public void OnPointerEnter(
        PointerEventData eventData
    )
    {
        ShowItemInformation();
    }

    public void OnPointerExit(
        PointerEventData eventData
    )
    {
        ClearItemInformation();
    }

    private void ShowItemInformation()
    {
        if (InventoryManager.Instance == null)
        {
            ClearItemInformation();
            return;
        }

        ShopItemData item =
            InventoryManager.Instance.GetItem(
                slotIndex
            );

        if (item == null)
        {
            ClearItemInformation();
            return;
        }

        if (itemNameText != null)
        {
            itemNameText.text =
                item.itemName;
        }

        if (itemDescriptionText != null)
        {
            itemDescriptionText.text =
                item.description;
        }
    }

    private void ClearItemInformation()
    {
        if (itemNameText != null)
        {
            itemNameText.text =
                defaultName;
        }

        if (itemDescriptionText != null)
        {
            itemDescriptionText.text =
                defaultDescription;
        }
    }

    private void OnDisable()
    {
        ClearItemInformation();
    }
}