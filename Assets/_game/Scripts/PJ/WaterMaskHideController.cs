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
    [SerializeField] private string hideTriggerName = "Hide";

    [Header("Default Hide Duration")]
    [Tooltip("Duración por defecto si el spot no define un tiempo.")]
    [SerializeField] private float hideDuration = 5f;

    [SerializeField] private bool lockInputWhileHidden = true;

    [Header("Exit")]
    [SerializeField] private bool useExitPoints = true;

    private WaterHideSpot nearbySpot;
    private WaterHideSpot hiddenInSpot;

    private bool isHidden;
    private Coroutine autoUnhideCo;
    private int hideTriggerHash;

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

    public void OnMove(InputValue value)
    {
        lastMoveInput = value.Get<Vector2>();
    }

    public void OnDefend()
    {
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
        // Duración final: si el spot tiene tiempo definido, lo usa
        float durationToUse = hideDuration;
        if (nearbySpot != null && nearbySpot.HideSeconds > 0f)
            durationToUse = nearbySpot.HideSeconds;

        if (durationToUse <= 0f) durationToUse = 0.1f;

        Vector2 hideDir = lastMoveInput;
        if (hideDir.sqrMagnitude < 0.001f) hideDir = Vector2.down;

        if (!nearbySpot.TryHide(this, hideDir)) return;

        hiddenInSpot = nearbySpot;
        nearbySpot = null;

        isHidden = true;

        if (animator && !string.IsNullOrEmpty(hideTriggerName))
            animator.SetTrigger(hideTriggerHash);

        if (movement) movement.enabled = false;

        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        if (spriteRenderer && hideSpriteCompletely)
            spriteRenderer.enabled = false;

        if (autoUnhideCo != null) StopCoroutine(autoUnhideCo);
        autoUnhideCo = StartCoroutine(AutoUnhideAfter(durationToUse));
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

        if (useExitPoints && hiddenInSpot != null)
        {
            Transform exit = hiddenInSpot.GetExitPoint();
            if (exit != null)
            {
                if (rb) rb.position = exit.position;
                else transform.position = exit.position;
            }
        }

        if (hiddenInSpot)
        {
            hiddenInSpot.Unhide(this);
            hiddenInSpot = null;
        }

        var col = GetComponent<Collider2D>();
        if (col) col.enabled = true;

        if (movement) movement.enabled = true;

        if (spriteRenderer && hideSpriteCompletely)
            spriteRenderer.enabled = true;
    }
}
