using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class WaterMaskHideController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private ItemEquip itemEquip;
    [SerializeField] private PjMovement movement;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Rigidbody2D rb;

    [Header("Hide visuals")]
    [SerializeField] private bool hideSpriteCompletely = true;
    [SerializeField] private string hideTriggerName = "Hide"; // Trigger Animator (opcional)

    [Header("Auto Unhide")]
    [SerializeField] private float hideDuration = 5f;
    [SerializeField] private bool lockInputWhileHidden = true;

    [Header("Exit")]
    [Tooltip("Si true, al salir se teletransporta al exit point del spot (ExitUp/Down/Left/Right).")]
    [SerializeField] private bool useExitPoints = true;

    private WaterHideSpot nearbySpot;
    private WaterHideSpot hiddenInSpot;

    private bool isHidden;
    private Coroutine autoUnhideCo;

    private int hideTriggerHash;

    // Capturamos el último input de movimiento para decidir por qué lado esconderse/salir
    private Vector2 lastMoveInput = Vector2.down;

    private void Awake()
    {
        if (!itemEquip) itemEquip = GetComponent<ItemEquip>();
        if (!movement) movement = GetComponent<PjMovement>();
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!spriteRenderer) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (!rb) rb = GetComponent<Rigidbody2D>();

        hideTriggerHash = Animator.StringToHash(hideTriggerName);
    }

    // Input System: acción "Move" => método "OnMove" (Send Messages)
    public void OnMove(InputValue value)
    {
        lastMoveInput = value.Get<Vector2>();
    }

    // Input System: acción "Defend" => método "OnDefend"
    public void OnDefend()
    {
        // Si está escondido, ignorar input para que dure sí o sí
        if (isHidden)
        {
            if (lockInputWhileHidden) return;

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
        if (hideDuration <= 0f) hideDuration = 0.1f;

        // Dirección con la que se esconde (si no hay input, down)
        Vector2 hideDir = lastMoveInput;
        if (hideDir.sqrMagnitude < 0.001f) hideDir = Vector2.down;

        if (!nearbySpot.TryHide(this, hideDir)) return;

        hiddenInSpot = nearbySpot;
        nearbySpot = null;

        isHidden = true;

        // Anim trigger (opcional)
        if (animator && !string.IsNullOrEmpty(hideTriggerName))
            animator.SetTrigger(hideTriggerHash);

        // Bloquear movimiento
        if (movement) movement.enabled = false;

        // Desactivar collider
        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        // Ocultar sprite
        if (spriteRenderer && hideSpriteCompletely)
            spriteRenderer.enabled = false;

        // Auto-unhide
        if (autoUnhideCo != null) StopCoroutine(autoUnhideCo);
        autoUnhideCo = StartCoroutine(AutoUnhideAfter(hideDuration));
    }

    private IEnumerator AutoUnhideAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (isHidden) Unhide();
        autoUnhideCo = null;
    }

    private void Unhide()
    {
        if (!isHidden) return;
        isHidden = false;

        if (autoUnhideCo != null)
        {
            StopCoroutine(autoUnhideCo);
            autoUnhideCo = null;
        }

        // Teleport al exit point del spot (si existe)
        if (useExitPoints && hiddenInSpot != null)
        {
            Transform exit = hiddenInSpot.GetExitPoint();
            if (exit != null)
            {
                if (rb) rb.position = exit.position;
                else transform.position = exit.position;
            }
        }

        // Libera spot y apaga overlay
        if (hiddenInSpot)
        {
            hiddenInSpot.Unhide(this);
            hiddenInSpot = null;
        }

        // Reactivar collider
        var col = GetComponent<Collider2D>();
        if (col) col.enabled = true;

        // Reactivar movimiento
        if (movement) movement.enabled = true;

        // Mostrar sprite
        if (spriteRenderer && hideSpriteCompletely)
            spriteRenderer.enabled = true;
    }
}
