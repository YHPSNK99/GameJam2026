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

    public ItemData Equipped => equipped;
    public ItemData SlotA => slotA;
    public ItemData SlotB => slotB;

    // Input System (PlayerInput Send Messages)
    // Acción llamada "Equip" -> método "OnEquip"
    public void OnEquip()
    {
        EquipOrSwap();
    }

    private void EquipOrSwap()
    {
        int count = (slotA ? 1 : 0) + (slotB ? 1 : 0);

        if (count == 0)
            return;

        if (count == 1)
        {
            equipped = slotA ? slotA : slotB;
            OnEquippedChanged(equipped);
            return;
        }

        // Si hay 2, alterna
        if (equipped == null || equipped == slotB)
            equipped = slotA;
        else
            equipped = slotB;

        OnEquippedChanged(equipped);
    }

    // Llama esto cuando recojas un item
    public bool TryAddItem(ItemData item)
    {
        if (!item) return false;

        // opcional: evitar duplicados
        if (slotA == item || slotB == item) return false;

        if (slotA == null)
        {
            slotA = item;
            if (equipped == null) { equipped = slotA; OnEquippedChanged(equipped); }
            return true;
        }

        if (slotB == null)
        {
            slotB = item;
            if (equipped == null) { equipped = slotB; OnEquippedChanged(equipped); }
            return true;
        }

        // Ya tiene 2 items
        return false;
    }

    private void OnEquippedChanged(ItemData newEquipped)
    {
        // Aquí conectas UI / VFX / SFX
        // Debug.Log($"Equipped: {(newEquipped ? newEquipped.type.ToString() : "None")}");
    }
}
