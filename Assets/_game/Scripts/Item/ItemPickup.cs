using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [SerializeField] private ItemData item; // asigna Mask_Water o Mask_Fire

    private void OnTriggerEnter2D(Collider2D other)
    {
        var equip = other.GetComponent<ItemEquip>();
        if (!equip) return;

        if (equip.TryAddItem(item))
        {
            Destroy(gameObject);
        }
        else
        {
            // inventario lleno o duplicado (opcional feedback)
            // Debug.Log("Inventario lleno o item duplicado");
        }
    }
}
