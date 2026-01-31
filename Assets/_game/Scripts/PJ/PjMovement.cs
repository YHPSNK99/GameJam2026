using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class PjMovement : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Units per second.")]
    [SerializeField] private float moveSpeed = 5.5f;

    [Tooltip("If true, diagonal speed is normalized (recommended).")]
    [SerializeField] private bool normalizeDiagonal = true;

    private Rigidbody2D rb;
    private Vector2 moveInput;     // raw input from Input System
    private Vector2 moveDir;       // processed direction (normalized if needed)

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Hard requirements for top-down "snappy" movement
        rb.gravityScale = 0f;
        rb.drag = 0f;
        rb.angularDrag = 0f;
        rb.freezeRotation = true;
    }

    /// <summary>
    /// Input System callback (PlayerInput -> Behavior: Send Messages)
    /// Must match action name: "Move" -> method name "OnMove".
    /// </summary>
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        Debug.Log("MOVE: " + moveInput);
        //moveInput = v;
    }

    private void FixedUpdate()
    {
        // Process direction
        moveDir = moveInput;

        if (normalizeDiagonal && moveDir.sqrMagnitude > 1f)
            moveDir = moveDir.normalized;

        // UnMetal-like: instant response, no acceleration
        Vector2 targetPos = rb.position + moveDir * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(targetPos);
    }

    private void OnDisable()
    {
        // Safety: stop movement if object/input is disabled mid-move
        moveInput = Vector2.zero;
        moveDir = Vector2.zero;
        if (rb) rb.velocity = Vector2.zero;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (moveSpeed < 0f) moveSpeed = 0f;
    }
#endif
}
