using UnityEngine;

public enum ShopItemType
{
    Antivirus,
    AntivirusPremium,
    Clover,
    Coffee,
    Hourglass,
    Magnifier
}

[CreateAssetMenu(
    fileName = "New Shop Item",
    menuName = "Shop/Shop Item"
)]
public class ShopItemData : ScriptableObject
{
    [Header("Item Information")]
    public string itemName;

    [TextArea(2, 5)]
    public string description;

    [Min(0)]
    public int price;

    public Sprite sprite;

    [Header("Item Effect")]
    public ShopItemType itemType;
}