using UnityEngine;
using UnityEngine.InputSystem;

public class WaterHideSpot : MonoBehaviour
{
    [Header("Hide Duration (seconds)")]
    [Tooltip("Cuánto tiempo permanece escondido en este spot. 0 usa el default del Player.")]
    [SerializeField] private float hideSeconds = 0f;

    [Header("Visual")]
    [SerializeField] private GameObject wetOverlay;

    [Header("Prompt")]
    [SerializeField] private GameObject promptKeyboard;
    [SerializeField] private GameObject promptGamepad;

    [Header("Exit points (optional)")]
    [SerializeField] private Transform exitUp;
    [SerializeField] private Transform exitDown;
    [SerializeField] private Transform exitLeft;
    [SerializeField] private Transform exitRight;

    private WaterMaskHideController currentHider;
    private Vector2 lastHideDir = Vector2.down;

    public bool IsOccupied => currentHider != null;

    // ✅ Player podrá leer esta duración
    public float HideSeconds => hideSeconds;

    private void Awake()
    {
        SetPrompt(false, null);
        SetWet(false);
    }

    public bool TryHide(WaterMaskHideController hider)
    {
        return TryHide(hider, Vector2.down);
    }

    public bool TryHide(WaterMaskHideController hider, Vector2 hideDir)
    {
        if (IsOccupied) return false;

        currentHider = hider;
        lastHideDir = QuantizeTo4(hideDir);

        SetPrompt(false, hider);
        SetWet(true);

        var bottle = GetComponent<BottleHideSpot>();
        if (bottle)
            bottle.OnPlayerHide();

        return true;
    }

    public void Unhide(WaterMaskHideController hider)
    {
        if (currentHider != hider) return;

        currentHider = null;
        SetWet(false);
    }

    public Transform GetExitPoint()
    {
        if (!exitUp && !exitDown && !exitLeft && !exitRight) return null;

        if (Mathf.Abs(lastHideDir.x) > Mathf.Abs(lastHideDir.y))
            return lastHideDir.x > 0 ? exitRight : exitLeft;
        else
            return lastHideDir.y > 0 ? exitUp : exitDown;
    }

    private void SetWet(bool wet)
    {
        if (wetOverlay) wetOverlay.SetActive(wet);
    }

    private void SetPrompt(bool show, WaterMaskHideController player)
    {
        if (promptKeyboard) promptKeyboard.SetActive(false);
        if (promptGamepad) promptGamepad.SetActive(false);

        if (!show || player == null) return;

        var pi = player.GetComponent<PlayerInput>();
        if (pi != null && pi.currentControlScheme != null && pi.currentControlScheme.Contains("Gamepad"))
        {
            if (promptGamepad) promptGamepad.SetActive(true);
        }
        else
        {
            if (promptKeyboard) promptKeyboard.SetActive(true);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponent<WaterMaskHideController>();
        if (!player) return;

        player.SetNearbySpot(this);

        if (!IsOccupied && player.CanUseWaterHide())
            SetPrompt(true, player);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var player = other.GetComponent<WaterMaskHideController>();
        if (!player) return;

        SetPrompt(false, player);
        player.ClearNearbySpot(this);
    }

    private static Vector2 QuantizeTo4(Vector2 v)
    {
        if (v.sqrMagnitude < 0.001f) return Vector2.down;

        if (Mathf.Abs(v.x) > Mathf.Abs(v.y))
            return new Vector2(Mathf.Sign(v.x), 0f);
        else
            return new Vector2(0f, Mathf.Sign(v.y));
    }
}
