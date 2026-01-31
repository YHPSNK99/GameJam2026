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
    [SerializeField] private float inputDeadzone = 0.15f;

    private Rigidbody2D rb;
    private Vector2 moveInput;      // raw input
    private Vector2 moveDir;        // normalized/processed direction
    private Vector2 lastLookDir = Vector2.down; // idle por defecto mirando abajo/frente

    private static readonly int HashHorizontal = Animator.StringToHash("horizontal");
    private static readonly int HashVertical = Animator.StringToHash("vertical");
    private static readonly int HashSpeed = Animator.StringToHash("speed");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!animator) animator = GetComponent<Animator>();

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

        // Movimiento
        Vector2 targetPos = rb.position + moveDir * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(targetPos);

        // Animación: usar dirección, NO posición
        float speed = moveDir.magnitude; // 0..1 normalmente
        animator.SetFloat(HashSpeed, speed);

        if (speed > inputDeadzone)
        {
            // 4 direcciones: cuantiza para evitar diagonales en el blend tree 4-dir
            Vector2 dir4 = QuantizeTo4(moveDir);
            lastLookDir = dir4;

            animator.SetFloat(HashHorizontal, dir4.x);
            animator.SetFloat(HashVertical, dir4.y);
        }
        else
        {
            // Idle: conserva última dirección
            animator.SetFloat(HashHorizontal, lastLookDir.x);
            animator.SetFloat(HashVertical, lastLookDir.y);
        }
    }

    private static Vector2 QuantizeTo4(Vector2 v)
    {
        // decide si predomina X o Y
        if (Mathf.Abs(v.x) > Mathf.Abs(v.y))
            return new Vector2(Mathf.Sign(v.x), 0f);  // derecha/izquierda
        else
            return new Vector2(0f, Mathf.Sign(v.y));  // arriba/abajo
    }

    private void OnDisable()
    {
        moveInput = Vector2.zero;
        moveDir = Vector2.zero;
        if (rb) rb.velocity = Vector2.zero;

        if (animator) animator.SetFloat(HashSpeed, 0f);
    }
}
