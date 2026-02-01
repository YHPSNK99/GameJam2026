using UnityEngine;

[DisallowMultipleComponent]
public class WaterMaskHideController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private ItemEquip itemEquip;
    [SerializeField] private MonoBehaviour movementScript; // arrastra tu PjMovement aquí
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Hide visuals")]
    [SerializeField] private bool hideSpriteCompletely = true; // true = desaparece
    [SerializeField] private string hideTriggerName = "Hide";  // trigger en animator (opcional)

    private WaterHideSpot nearbySpot;
    private WaterHideSpot hiddenInSpot;
    private bool isHidden;

    private int hideTriggerHash;

    private void Awake()
    {
        if (!itemEquip) itemEquip = GetComponent<ItemEquip>();
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!spriteRenderer) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        hideTriggerHash = Animator.StringToHash(hideTriggerName);
    }

    // Input System: acción "Defend" => método "OnDefend"
    public void OnDefend()
    {
        if (isHidden)
        {
            Unhide();
            return;
        }

        if (nearbySpot == null) return;
        if (!CanUseWaterHide()) return;
        if (nearbySpot.IsOccupied) return;

        Hide();
    }

    public bool CanUseWaterHide()
    {
        if (!itemEquip || itemEquip.Equipped == null) return false;
        return itemEquip.Equipped.type == ItemType.MaskWater;
    }

    public void SetNearbySpot(WaterHideSpot spot) => nearbySpot = spot;

    public void ClearNearbySpot(WaterHideSpot spot)
    {
        if (nearbySpot == spot) nearbySpot = null;
    }

    private void Hide()
    {
        if (!nearbySpot.TryHide(this)) return;

        hiddenInSpot = nearbySpot;
        nearbySpot = null;

        isHidden = true;

        // Dispara animación (si existe)
        if (animator)
            animator.SetTrigger(hideTriggerHash);

        // Bloquear movimiento
        if (movementScript) movementScript.enabled = false;

        // Desactivar collider del player (para que no lo empujen / detecten)
        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        // Ocultar visual del player
        if (spriteRenderer && hideSpriteCompletely)
            spriteRenderer.enabled = false;
    }

    private void Unhide()
    {
        if (!isHidden) return;

        isHidden = false;

        if (hiddenInSpot)
        {
            hiddenInSpot.Unhide(this);
            hiddenInSpot = null;
        }

        // Reactivar collider
        var col = GetComponent<Collider2D>();
        if (col) col.enabled = true;

        // Reactivar movimiento
        if (movementScript) movementScript.enabled = true;

        // Mostrar visual
        if (spriteRenderer && hideSpriteCompletely)
            spriteRenderer.enabled = true;
    }
}
