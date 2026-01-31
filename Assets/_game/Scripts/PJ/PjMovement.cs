using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class PjMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5.5f;
    [SerializeField] private bool normalizeDiagonal = true;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float inputDeadzone = 0.15f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 moveDir;

    // Idle por defecto mirando hacia abajo/frente
    private Vector2 lastLookDir = Vector2.down;

    private static readonly int HashHorizontal = Animator.StringToHash("horizontal");
    private static readonly int HashVertical = Animator.StringToHash("vertical");
    private static readonly int HashSpeed = Animator.StringToHash("speed");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!spriteRenderer) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // Movimiento top-down snappy
        rb.gravityScale = 0f;
        rb.drag = 0f;
        rb.angularDrag = 0f;
        rb.freezeRotation = true;
    }

    // PlayerInput (Send Messages) -> Acción "Move" llama a OnMove
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    private void FixedUpdate()
    {
        moveDir = moveInput;

        if (normalizeDiagonal && moveDir.sqrMagnitude > 1f)
            moveDir = moveDir.normalized;

        // Mover
        rb.MovePosition(rb.position + moveDir * moveSpeed * Time.fixedDeltaTime);

        // Animación
        float speed = moveDir.magnitude;
        animator.SetFloat(HashSpeed, speed);

        if (speed > inputDeadzone)
        {
            // 4 direcciones para el blend tree
            Vector2 dir4 = QuantizeTo4(moveDir);
            lastLookDir = dir4;

            animator.SetFloat(HashHorizontal, dir4.x);
            animator.SetFloat(HashVertical, dir4.y);

            // Mirror: derecha usa anim izquierda con flipX
            UpdateFlip(dir4);
        }
        else
        {
            // Idle: conserva última dirección + flip correcto
            animator.SetFloat(HashHorizontal, lastLookDir.x);
            animator.SetFloat(HashVertical, lastLookDir.y);

            UpdateFlip(lastLookDir);
        }
    }

    private void UpdateFlip(Vector2 lookDir)
    {
        // Solo volteamos si está mirando horizontalmente.
        // Si está arriba/abajo, dejamos el último estado de flip (o puedes forzarlo si quieres).
        if (lookDir.x > 0.01f)
            spriteRenderer.flipX = true;   // derecha (mirror)
        else if (lookDir.x < -0.01f)
            spriteRenderer.flipX = false;  // izquierda (original)
    }

    private static Vector2 QuantizeTo4(Vector2 v)
    {
        if (Mathf.Abs(v.x) > Mathf.Abs(v.y))
            return new Vector2(Mathf.Sign(v.x), 0f);   // izquierda/derecha
        else
            return new Vector2(0f, Mathf.Sign(v.y));   // arriba/abajo
    }

    private void OnDisable()
    {
        moveInput = Vector2.zero;
        moveDir = Vector2.zero;
        if (rb) rb.velocity = Vector2.zero;
        if (animator) animator.SetFloat(HashSpeed, 0f);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (moveSpeed < 0f) moveSpeed = 0f;
        if (inputDeadzone < 0f) inputDeadzone = 0f;
    }
#endif
}
