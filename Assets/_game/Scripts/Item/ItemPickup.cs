using UnityEngine;

public class ItemPickup : MonoBehaviour
{

    [SerializeField] private ItemData item;

    private void OnTriggerEnter2D(Collider2D other)
    {
        var equip = other.GetComponent<ItemEquip>();
        if (!equip) return;

        if (equip.TryAddItem(item))
            Destroy(gameObject);
    }
}
