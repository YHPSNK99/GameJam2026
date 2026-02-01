using UnityEngine;
using UnityEngine.UI;

public class PlayerInventoryUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ItemEquip playerEquip;

    [Header("UI")]
    [SerializeField] private Image slotAIcon;
    [SerializeField] private Image slotBIcon;

    [Header("Colors")]
    [SerializeField] private Color equippedColor = Color.white;
    [SerializeField] private Color unequippedColor = new Color(1f, 1f, 1f, 0.45f);
    [SerializeField] private Color emptyColor = new Color(1f, 1f, 1f, 0f);

    private void Start()
    {
        if (!playerEquip)
            playerEquip = FindFirstObjectByType<ItemEquip>();
    }

    private void Update()
    {
        if (!playerEquip) return;

        var a = playerEquip.SlotA;
        var b = playerEquip.SlotB;
        var eq = playerEquip.Equipped;

        UpdateSlot(slotAIcon, a, eq == a);
        UpdateSlot(slotBIcon, b, eq == b);
    }

    private void UpdateSlot(Image img, ItemData item, bool isEquipped)
    {
        if (!img) return;

        if (item == null)
        {
            img.sprite = null;
            img.color = emptyColor; // invisible
            return;
        }

        img.sprite = item.icon;
        img.color = isEquipped ? equippedColor : unequippedColor;
    }
}
