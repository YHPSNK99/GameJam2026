using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemEquip : MonoBehaviour
{
    [Header("Slots")]
    [SerializeField] private ItemData slotA;
    [SerializeField] private ItemData slotB;

    [Header("Equipped (solo lectura en inspector)")]
    [SerializeField] private ItemData equipped;

    public ItemData SlotA => slotA;
    public ItemData SlotB => slotB;
    public ItemData Equipped => equipped;

    // Input System (PlayerInput -> Send Messages)
    // Action: "Equip" => method: "OnEquip"
    public void OnEquip()
    {
        EquipOrSwap();
    }

    private void EquipOrSwap()
    {
        int count = (slotA ? 1 : 0) + (slotB ? 1 : 0);

        if (count == 0) return;

        if (count == 1)
        {
            equipped = slotA ? slotA : slotB;
            return;
        }

        // count == 2 => alterna entre A y B
        if (equipped == null || equipped == slotB)
            equipped = slotA;
        else
            equipped = slotB;
    }

    // Lo llama el pickup al recoger
    public bool TryAddItem(ItemData item)
    {
        if (!item) return false;

        // Evitar duplicados (opcional)
        if (slotA == item || slotB == item) return false;

        if (slotA == null)
        {
            slotA = item;
            if (equipped == null) equipped = slotA; // auto-equip al primer item
            return true;
        }

        if (slotB == null)
        {
            slotB = item;
            if (equipped == null) equipped = slotB;
            return true;
        }

        return false; // inventario lleno (2 items)
    }

    private void OnEquippedChanged(ItemData newEquipped)
    {
        // Aquí conectas UI / VFX / SFX
        // Debug.Log($"Equipped: {(newEquipped ? newEquipped.type.ToString() : "None")}");
    }
}
