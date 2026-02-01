using UnityEngine;

public class EnemyAI2D : MonoBehaviour
{
    [Header("Auto: busca Player por Tag")]
    [SerializeField] Transform player;

    [Header("Patrulla (usa hijos WP_A y WP_B)")]
    [SerializeField] Transform wpA;
    [SerializeField] Transform wpB;

    [Header("Movimiento")]
    [SerializeField] float patrolSpeed = 2f;
    [SerializeField] float chaseSpeed = 3.2f;
    [SerializeField] float reachDistance = 0.15f;

    [Header("Detección")]
    [SerializeField] float aggroRange = 4f;
    [SerializeField] float loseRange = 6f;

    [Header("Debug colores")]
    [SerializeField] bool debugColors = true;

    Rigidbody2D rb;
    SpriteRenderer sr;
    bool chasing = false;
    Transform currentWp;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (wpA == null)
        {
            var t = transform.Find("WP_A");
            if (t != null) wpA = t;
        }
        if (wpB == null)
        {
            var t = transform.Find("WP_B");
            if (t != null) wpB = t;
        }

        currentWp = wpA;
    }

    void FixedUpdate()
    {
        if (player == null || wpA == null || wpB == null) return;

        float distToPlayer = Vector2.Distance(rb.position, player.position);

        if (!chasing && distToPlayer <= aggroRange) chasing = true;
        if (chasing && distToPlayer >= loseRange) chasing = false;

        if (debugColors && sr != null)
            sr.color = chasing ? Color.red : Color.green;   // ROJO persigue / VERDE patrulla

        if (chasing) ChasePlayer();
        else Patrol();
    }

    void Patrol()
    {
        if (currentWp == null) currentWp = wpA;

        Vector2 target = currentWp.position;
        MoveTowards(target, patrolSpeed);

        if (Vector2.Distance(rb.position, target) <= reachDistance)
            currentWp = (currentWp == wpA) ? wpB : wpA;
    }

    void ChasePlayer()
    {
        MoveTowards(player.position, chaseSpeed);
    }

    void MoveTowards(Vector2 target, float speed)
    {
        Vector2 newPos = Vector2.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);
    }

    // Círculos de rango (se ven en Scene cuando seleccionas el Enemy)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, aggroRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, loseRange);
    }
}
