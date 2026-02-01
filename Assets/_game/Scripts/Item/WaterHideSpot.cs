using UnityEngine;
using UnityEngine.InputSystem;

public class WaterHideSpot : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private GameObject wetOverlay;

    [Header("Prompt")]
    [SerializeField] private GameObject promptKeyboard;
    [SerializeField] private GameObject promptGamepad;

    private WaterMaskHideController currentHider;

    public bool IsOccupied => currentHider != null;

    private void Awake()
    {
        SetPrompt(false, null);
        SetWet(false);
    }

    public bool TryHide(WaterMaskHideController hider)
    {
        if (IsOccupied) return false;

        currentHider = hider;
        SetPrompt(false, hider);
        SetWet(true);
        return true;
    }

    public void Unhide(WaterMaskHideController hider)
    {
        if (currentHider != hider) return;
        currentHider = null;
        SetWet(false);
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
        if (pi != null && pi.currentControlScheme == "Gamepad")
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
}
