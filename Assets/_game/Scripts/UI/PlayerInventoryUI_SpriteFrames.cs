using UnityEngine;
using UnityEngine.UI;

public class PlayerInventoryUI_SpriteFrames : MonoBehaviour
{
    [Header("Player reference")]
    [SerializeField] private ItemEquip playerEquip;

    [Header("Slot A UI")]
    [SerializeField] private Image slotAFrame;
    [SerializeField] private Image slotAIcon;

    [Header("Slot B UI")]
    [SerializeField] private Image slotBFrame;
    [SerializeField] private Image slotBIcon;

    [Header("Frame Sprites (2 referencias)")]
    [SerializeField] private Sprite frameNormal;
    [SerializeField] private Sprite frameEquipped;

    private void Awake()
    {
        // Si no lo asignas a mano, intenta encontrarlo
        if (!playerEquip)
            playerEquip = FindFirstObjectByType<ItemEquip>();
    }

    private void LateUpdate()
    {
        if (!playerEquip) return;

        var a = playerEquip.SlotA;
        var b = playerEquip.SlotB;
        var eq = playerEquip.Equipped;

        UpdateSlot(slotAFrame, slotAIcon, a, eq == a);
        UpdateSlot(slotBFrame, slotBIcon, b, eq == b);
    }

    private void UpdateSlot(Image frame, Image icon, ItemData item, bool equipped)
    {
        if (frame)
            frame.sprite = equipped ? frameEquipped : frameNormal;

        if (!icon) return;

        if (item == null)
        {
            icon.enabled = false;  // oculta el icono si no hay item
            icon.sprite = null;
            return;
        }

        icon.enabled = true;
        icon.sprite = item.icon;
        icon.preserveAspect = true;
    }
}
