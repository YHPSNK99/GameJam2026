using UnityEngine;
using UnityEngine.InputSystem;

public class GamePauseController : MonoBehaviour
{
    public static GamePauseController Instance { get; private set; }

    [Header("Panels (mismo Canvas)")]
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject pausePanel;

    [Header("Input")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private string playerActionMap = "Player";
    [SerializeField] private string uiActionMap = "UI";

    [Header("Cursor")]
    [SerializeField] private bool lockCursorWhenPlaying = true;

    public bool IsPaused { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (!playerInput) playerInput = GetComponent<PlayerInput>();
        if (!playerInput) playerInput = GetComponentInParent<PlayerInput>();

        if (!hudPanel) Debug.LogWarning("[PauseController] hudPanel NO asignado.", this);
        if (!pausePanel) Debug.LogWarning("[PauseController] pausePanel NO asignado.", this);
        if (!playerInput) Debug.LogWarning("[PauseController] playerInput NO encontrado/asignado.", this);
    }

    void Start()
    {
        Resume(); // estado inicial: gameplay
    }

    // Send Messages: acción "Pause" en Player y UI
    public void OnPause()
    {
        TogglePause();
    }

    // Opcional: si en UI usas "Cancel" (ESC) para cerrar
    public void OnCancel()
    {
        if (IsPaused) Resume();
    }

    public void TogglePause()
    {
        if (IsPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        IsPaused = true;
        Time.timeScale = 0f;

        if (hudPanel) hudPanel.SetActive(false);
        if (pausePanel) pausePanel.SetActive(true);

        ForceActionMap(uiActionMap); // <- clave: deshabilita Player y habilita UI

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Opcional:
        // AudioListener.pause = true;
    }

    public void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;

        if (hudPanel) hudPanel.SetActive(true);
        if (pausePanel) pausePanel.SetActive(false);

        ForceActionMap(playerActionMap); // <- clave: deshabilita UI y habilita Player

        if (lockCursorWhenPlaying)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        // AudioListener.pause = false;
    }

    private void ForceActionMap(string mapToEnable)
    {
        if (!playerInput || playerInput.actions == null) return;

        var enableMap = playerInput.actions.FindActionMap(mapToEnable, true);
        var disableMap = playerInput.actions.FindActionMap(
            mapToEnable == uiActionMap ? playerActionMap : uiActionMap, true);

        // IMPORTANTE: deshabilitar primero evita que se filtren inputs
        disableMap.Disable();
        enableMap.Enable();

        // Mantiene PlayerInput sincronizado con el map activo
        if (playerInput.currentActionMap == null || playerInput.currentActionMap.name != mapToEnable)
            playerInput.SwitchCurrentActionMap(mapToEnable);

        // Debug opcional:
         Debug.Log($"[PauseController] Enabled: {enableMap.name} | Disabled: {disableMap.name}");
    }
}
