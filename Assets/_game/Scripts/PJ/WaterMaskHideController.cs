using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class WaterMaskHideController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private ItemEquip itemEquip;
    [SerializeField] private PjMovement movement;                 // tu script de movimiento
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Hide visuals")]
    [SerializeField] private bool hideSpriteCompletely = true;     // true = desaparece al esconderse
    [SerializeField] private string hideTriggerName = "Hide";      // Trigger en Animator (opcional)

    [Header("Auto Unhide")]
    [Tooltip("Segundos que permanece escondido. Debe ser > 0.")]
    [SerializeField] private float hideDuration = 5f;

    [Tooltip("Si está activo, mientras esté escondido ignorará el input Defend (se queda sí o sí).")]
    [SerializeField] private bool lockInputWhileHidden = true;

    [Header("Exit look direction (optional)")]
    [SerializeField] private bool forceExitLookDir = false;
    [SerializeField] private Vector2 exitLookDir = Vector2.down;

    private WaterHideSpot nearbySpot;
    private WaterHideSpot hiddenInSpot;

    private bool isHidden;
    private Coroutine autoUnhideCo;

    private int hideTriggerHash;

    // Animator movement params (si los usas)
    private static readonly int HashHorizontal = Animator.StringToHash("horizontal");
    private static readonly int HashVertical = Animator.StringToHash("vertical");
    private static readonly int HashSpeed = Animator.StringToHash("speed");

    private void Awake()
    {
        if (!itemEquip) itemEquip = GetComponent<ItemEquip>();
        if (!movement) movement = GetComponent<PjMovement>();
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!spriteRenderer) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        hideTriggerHash = Animator.StringToHash(hideTriggerName);
    }

    // Input System (Send Messages): acción "Defend" => método "OnDefend"
    public void OnDefend()
    {
        // Si está escondido: NO permitir salir manual si lockInputWhileHidden está activo
        if (isHidden)
        {
            if (lockInputWhileHidden)
                return;

            // Si algún día quieres permitir salir manual cuando lockInputWhileHidden=false:
            Unhide();
            return;
        }

        // No está escondido: intentar esconderse
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

        // Reserva spot
        if (!nearbySpot.TryHide(this)) return;

        hiddenInSpot = nearbySpot;
        nearbySpot = null;

        isHidden = true;

        // Animator trigger (opcional)
        if (animator && !string.IsNullOrEmpty(hideTriggerName))
            animator.SetTrigger(hideTriggerHash);

        // Bloquear movimiento
        if (movement) movement.enabled = false;

        // Desactivar collider del player (para que no lo empujen / detecten)
        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        // Ocultar visual del player
        if (spriteRenderer && hideSpriteCompletely)
            spriteRenderer.enabled = false;

        // Auto-unhide (se queda sí o sí este tiempo)
        if (autoUnhideCo != null) StopCoroutine(autoUnhideCo);
        autoUnhideCo = StartCoroutine(AutoUnhideAfter(hideDuration));
    }

    private IEnumerator AutoUnhideAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        // Si sigue escondido, salir
        if (isHidden)
            Unhide();

        autoUnhideCo = null;
    }

    private void Unhide()
    {
        if (!isHidden) return;

        isHidden = false;

        // detener timer si existía
        if (autoUnhideCo != null)
        {
            StopCoroutine(autoUnhideCo);
            autoUnhideCo = null;
        }

        // Libera spot y vuelve visual normal del spot
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

        // Mostrar visual
        if (spriteRenderer && hideSpriteCompletely)
            spriteRenderer.enabled = true;

        // Opcional: forzar dirección al salir (para que anim quede mirando correcto)
        if (forceExitLookDir && animator)
        {
            Vector2 dir = exitLookDir;
            if (dir.sqrMagnitude < 0.001f) dir = Vector2.down;
            dir = QuantizeTo4(dir);

            animator.SetFloat(HashHorizontal, dir.x);
            animator.SetFloat(HashVertical, dir.y);
            animator.SetFloat(HashSpeed, 0f);
        }
    }

    private static Vector2 QuantizeTo4(Vector2 v)
    {
        if (Mathf.Abs(v.x) > Mathf.Abs(v.y))
            return new Vector2(Mathf.Sign(v.x), 0f);
        else
            return new Vector2(0f, Mathf.Sign(v.y));
    }
}
