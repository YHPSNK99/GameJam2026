using UnityEngine;
using UnityEngine.UI;

public class PlayerInventoryUI_Glow : MonoBehaviour
{
    [Header("Player reference")]
    [SerializeField] private ItemEquip playerEquip;

    [Header("Slot A UI")]
    [SerializeField] private Image slotAIcon;
    [SerializeField] private Image slotAGlow;

    [Header("Slot B UI")]
    [SerializeField] private Image slotBIcon;
    [SerializeField] private Image slotBGlow;

    private void Awake()
    {
        if (!playerEquip)
            playerEquip = FindFirstObjectByType<ItemEquip>();
    }

    private void LateUpdate()
    {
        if (!playerEquip) return;

        var a = playerEquip.SlotA;
        var b = playerEquip.SlotB;
        var eq = playerEquip.Equipped;

        UpdateIcon(slotAIcon, a);
        UpdateIcon(slotBIcon, b);

        if (slotAGlow) slotAGlow.enabled = (a != null && eq == a);
        if (slotBGlow) slotBGlow.enabled = (b != null && eq == b);
    }

    private void UpdateIcon(Image icon, ItemData item)
    {
        if (!icon) return;

        // Seguridad: si por error alguien arrastró el frame aquí,
        // el frame suele tener un sprite asignado siempre.
        // El icono normalmente empieza vacío y lo asigna el script.
        bool looksLikeFrame = icon.sprite != null && item == null;
        if (looksLikeFrame)
        {
            // No tocar: probablemente es un frame mal asignado
            return;
        }

        if (item == null)
        {
            icon.enabled = false;
            icon.sprite = null;
            return;
        }

        icon.enabled = true;
        icon.sprite = item.icon;
        icon.preserveAspect = true;
    }
}
