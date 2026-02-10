using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PointerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera cam;

    [Header("Raycast")]
    [SerializeField] private LayerMask interactMask; // objetos interactuables
    [SerializeField] private LayerMask groundMask;   // suelo para mover

    [Header("Settings")]
    [SerializeField] private float maxDistance = 500f;

    private Vector2 pointerPos;

    void Awake()
    {
        if (!cam) cam = Camera.main;
    }

    // SEND MESSAGES: Action "Point" llamará este método (Value/Vector2)
    public void OnPoint(InputValue value)
    {
        pointerPos = value.Get<Vector2>();
    }

    // SEND MESSAGES: Action "Click" llamará este método (Button)
    public void OnClick(InputValue value)
    {
        if (!value.isPressed) return; // solo al presionar

        // Si estás en pausa, ignora clicks de gameplay (por seguridad)
        if (GamePauseController.Instance != null && GamePauseController.Instance.IsPaused)
            return;

        if (!cam) return;

        Ray ray = cam.ScreenPointToRay(pointerPos);

        // 1) Intentar interactuar primero
        if (interactMask.value != 0 && Physics.Raycast(ray, out RaycastHit hitI, maxDistance, interactMask))
        {
            var interact = hitI.collider.GetComponentInParent<IInteractable>();
            if (interact != null)
            {
                interact.Interact();
                return;
            }
        }

        // 2) Si no interactuó, mover al suelo
        if (groundMask.value != 0 && Physics.Raycast(ray, out RaycastHit hitG, maxDistance, groundMask))
        {
            Vector3 dest = hitG.point;

            // Aquí conecta tu sistema de movimiento:
            // - NavMeshAgent: agent.SetDestination(dest);
            // - tu script: moverController.MoveTo(dest);
            Debug.Log("Click move to: " + dest);
        }
    }

    // Opcional: click derecho para otra acción
    public void OnRightClick(InputValue value)
    {
        if (!value.isPressed) return;

        if (GamePauseController.Instance != null && GamePauseController.Instance.IsPaused)
            return;

        Debug.Log("Right click!");
    }
}

// Interfaz para interactuables (ponla en otro archivo si prefieres)
public interface IInteractable
{
    void Interact();
}
