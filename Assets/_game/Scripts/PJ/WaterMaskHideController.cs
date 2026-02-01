using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class WaterMaskHideController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private ItemEquip itemEquip;
    [SerializeField] private PjMovement movement;     // <-- ahora tipado
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Hide visuals")]
    [SerializeField] private bool hideSpriteCompletely = true;
    [SerializeField] private string hideTriggerName = "Hide"; // Trigger Animator (opcional)

    [Header("Auto Unhide")]
    [Tooltip("0 = no auto-salir. >0 = sale solo después de X segundos.")]
    [SerializeField] private float hideDuration = 2.0f;

    [Tooltip("Dirección al salir (solo se usa si forceExitLookDir=true).")]
    [SerializeField] private Vector2 exitLookDir = Vector2.down;

    [SerializeField] private bool forceExitLookDir = true;

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

        // Animator trigger (opcional)
        if (animator && !string.IsNullOrEmpty(hideTriggerName))
            animator.SetTrigger(hideTriggerHash);

        // Bloquear movimiento
        if (movement) movement.enabled = false;

        // Desactivar collider del player
        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        // Ocultar visual
        if (spriteRenderer && hideSpriteCompletely)
            spriteRenderer.enabled = false;

        // Auto-unhide
        if (hideDuration > 0f)
        {
            if (autoUnhideCo != null) StopCoroutine(autoUnhideCo);
            autoUnhideCo = StartCoroutine(AutoUnhideAfter(hideDuration));
        }
    }

    private IEnumerator AutoUnhideAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        // si sigue escondido, salir
        if (isHidden) Unhide();
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

        // Libera spot
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

        // Forzar dirección al salir (para que la anim quede mirando correcto)
        if (forceExitLookDir && animator)
        {
            Vector2 dir = exitLookDir;
            if (dir.sqrMagnitude < 0.001f) dir = Vector2.down;

            // cuantiza a 4 por coherencia con tu anim
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
