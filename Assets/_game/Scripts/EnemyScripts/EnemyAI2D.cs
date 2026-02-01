using UnityEngine;

public class EnemyAI2D : MonoBehaviour
{
    enum State { Wander, Chase, Search }

    [Header("Player")]
    [SerializeField] Transform player;

    [Header("Roam Area")]
    [SerializeField] Collider2D roamArea;
    [SerializeField] LayerMask wallsMask;

    [Header("Movimiento")]
    [SerializeField] float wanderSpeed = 4.0f;
    [SerializeField] float chaseSpeed = 6.5f;

    [Header("Detección - AUMENTADO")]
    [SerializeField] float aggroRange = 8f;       // Aumentado para mayor rango
    [SerializeField] float loseRange = 12f;       // Aumentado para mayor rango

    [Header("Wander")]
    [SerializeField] float stepDistanceMin = 3f;
    [SerializeField] float stepDistanceMax = 8f;
    [SerializeField] float wallMargin = 0.5f;
    [SerializeField] float reachDistance = 0.5f;

    [Header("Obstacle Avoidance")]
    [SerializeField] float avoidanceRayDistance = 1.5f;
    [SerializeField] int avoidanceRayCount = 5;
    [SerializeField] float avoidanceRaySpread = 60f;
    [SerializeField] float avoidanceForce = 3f;

    [Header("Chase Wall Detection")]
    [SerializeField] float wallCheckDistance = 1.2f;
    [SerializeField] float cornerCheckDistance = 0.8f;
    [SerializeField] float slideAlongWallForce = 1.5f;
    [SerializeField] float minDistanceToSlide = 0.6f;

    [Header("Anti-atasco")]
    [SerializeField] float stuckCheckTime = 1.0f;
    [SerializeField] float stuckMoveThreshold = 0.05f;
    [SerializeField] int maxStuckRetries = 3;

    [Header("Path Validation")]
    [SerializeField] float pathCheckDistance = 0.5f;
    [SerializeField] int maxPathCheckSteps = 15;

    [Header("Cuando se esconde")]
    [SerializeField] float searchTime = 2.0f;

    [Header("Debug")]
    [SerializeField] bool showDebugLogs = true;  // Activar/desactivar logs

    Rigidbody2D rb;
    SpriteRenderer sr;

    State state = State.Wander;

    Vector2 wanderTarget;
    Vector2 lastPos;
    float stuckTimer = 0f;
    int stuckRetries = 0;

    Vector2 lastKnownPlayerPos;
    float searchTimer;
    Vector2 lastChaseDirection = Vector2.zero;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        // Validar y configurar Rigidbody2D correctamente
        if (rb == null)
        {
            Debug.LogError($"[Enemy AI] ERROR CRÍTICO: No hay Rigidbody2D en {gameObject.name}. ¡Agrégalo!");
            enabled = false; // Desactivar el script si no hay Rigidbody2D
            return;
        }

        // Configuración automática del Rigidbody2D para que funcione correctamente
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f; // Sin gravedad para juegos top-down
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // No rotar

        if (showDebugLogs)
            Debug.Log($"[Enemy AI] Rigidbody2D configurado en {gameObject.name}");

        // Buscar jugador
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
            {
                player = p.transform;
                if (showDebugLogs)
                    Debug.Log($"[Enemy AI] Jugador encontrado: {player.name}");
            }
            else
            {
                Debug.LogWarning($"[Enemy AI] No se encontró jugador con tag 'Player'. Asigna el jugador manualmente o ponle el tag correcto.");
            }
        }

        // Buscar RoamArea
        if (roamArea == null)
        {
            var ra = GameObject.Find("RoamArea");
            if (ra != null)
            {
                roamArea = ra.GetComponent<Collider2D>();
                if (showDebugLogs)
                    Debug.Log($"[Enemy AI] RoamArea encontrada");
            }
        }

        lastPos = rb.position;
        PickNewWanderTarget();

        if (showDebugLogs)
            Debug.Log($"[Enemy AI] {gameObject.name} inicializado. Estado: {state}");
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        // Cambiar color según estado (verde=wander, rojo=chase, amarillo=search)
        if (sr != null)
        {
            if (state == State.Wander) sr.color = Color.green;
            else if (state == State.Chase) sr.color = Color.red;
            else sr.color = Color.yellow;
        }

        if (player == null)
        {
            DoWander();
            return;
        }

        bool hidden = IsPlayerHidden(player);
        float distToPlayer = Vector2.Distance(rb.position, player.position);

        // Lógica de estados
        if (!hidden)
        {
            if (state != State.Chase && distToPlayer <= aggroRange)
            {
                state = State.Chase;
                lastChaseDirection = Vector2.zero;

                if (showDebugLogs)
                    Debug.Log($"[Enemy AI] ¡DETECTADO! Distancia: {distToPlayer:F2}. Comenzando persecución.");
            }

            if (state == State.Chase)
            {
                lastKnownPlayerPos = player.position;

                if (distToPlayer >= loseRange)
                {
                    state = State.Wander;
                    PickNewWanderTarget();

                    if (showDebugLogs)
                        Debug.Log($"[Enemy AI] Jugador perdido. Volviendo a deambular.");
                }
            }
        }
        else
        {
            if (state == State.Chase)
            {
                state = State.Search;
                searchTimer = searchTime;

                if (showDebugLogs)
                    Debug.Log($"[Enemy AI] Jugador escondido. Buscando en última posición conocida.");
            }
        }

        // Ejecutar comportamiento según estado
        if (state == State.Wander) DoWander();
        else if (state == State.Chase) DoChaseImproved();
        else DoSearch();
    }

    void DoWander()
    {
        float moved = Vector2.Distance(rb.position, lastPos);
        lastPos = rb.position;

        if (moved < stuckMoveThreshold)
        {
            stuckTimer += Time.fixedDeltaTime;
        }
        else
        {
            stuckTimer = 0f;
            stuckRetries = 0;
        }

        if (stuckTimer >= stuckCheckTime)
        {
            stuckTimer = 0f;
            stuckRetries++;

            if (showDebugLogs)
                Debug.Log($"[Enemy AI] Atascado detectado. Intento {stuckRetries}/{maxStuckRetries}");

            if (stuckRetries >= maxStuckRetries)
            {
                stuckRetries = 0;
                PickEmergencyWanderTarget();
            }
            else
            {
                PickNewWanderTarget();
            }
        }

        if (Vector2.Distance(rb.position, wanderTarget) <= reachDistance)
        {
            PickNewWanderTarget();
        }

        MoveWithAvoidance(wanderTarget, wanderSpeed);
    }

    void DoChaseImproved()
    {
        Vector2 directionToPlayer = ((Vector2)player.position - rb.position).normalized;

        // Verificar línea de visión directa
        RaycastHit2D lineOfSight = Physics2D.Raycast(rb.position, directionToPlayer,
            Vector2.Distance(rb.position, player.position), wallsMask);

        // Si no hay pared, perseguir directamente
        if (lineOfSight.collider == null)
        {
            MoveWithAvoidance(player.position, chaseSpeed);
            lastChaseDirection = directionToPlayer;
            return;
        }

        // Si hay pared, rodearla inteligentemente
        Vector2 moveDirection = CalculateWallNavigationDirection(directionToPlayer, lineOfSight);

        Vector2 newPos = rb.position + moveDirection * chaseSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPos);

        lastChaseDirection = moveDirection;

        Debug.DrawLine(rb.position, (Vector2)rb.position + moveDirection * 2f, Color.cyan);
    }

    Vector2 CalculateWallNavigationDirection(Vector2 toPlayer, RaycastHit2D wallHit)
    {
        // Detectar pared adelante
        RaycastHit2D forwardCheck = Physics2D.Raycast(rb.position, toPlayer, wallCheckDistance, wallsMask);

        if (forwardCheck.collider != null)
        {
            // Calcular normal de la pared
            Vector2 wallNormal = forwardCheck.normal;

            // Dos direcciones posibles para deslizarse
            Vector2 slideRight = Vector2.Perpendicular(wallNormal);
            Vector2 slideLeft = -slideRight;

            // Evaluar cuál dirección nos acerca más al jugador
            float rightScore = Vector2.Dot(slideRight, toPlayer);
            float leftScore = Vector2.Dot(slideLeft, toPlayer);

            Vector2 slideDirection = rightScore > leftScore ? slideRight : slideLeft;

            // Verificar que no haya pared en la dirección elegida
            RaycastHit2D slideCheck = Physics2D.Raycast(rb.position, slideDirection, cornerCheckDistance, wallsMask);

            if (slideCheck.collider != null)
            {
                // Probar la otra dirección
                slideDirection = rightScore > leftScore ? slideLeft : slideRight;
                slideCheck = Physics2D.Raycast(rb.position, slideDirection, cornerCheckDistance, wallsMask);

                if (slideCheck.collider != null)
                {
                    // Esquina detectada - retroceder y girar
                    return (-forwardCheck.normal * 0.5f + slideDirection * 0.5f).normalized;
                }
            }

            // Combinar deslizamiento con objetivo
            Vector2 finalDirection = (slideDirection * slideAlongWallForce + toPlayer * 0.3f).normalized;

            // Agregar avoidance adicional
            Vector2 avoidance = CalculateAvoidanceVector(finalDirection);

            return (finalDirection + avoidance * 0.5f).normalized;
        }
        else
        {
            // No hay pared inmediata, usar avoidance normal
            Vector2 avoidance = CalculateAvoidanceVector(toPlayer);
            return (toPlayer + avoidance).normalized;
        }
    }

    void DoSearch()
    {
        MoveWithAvoidance(lastKnownPlayerPos, chaseSpeed);

        if (Vector2.Distance(rb.position, lastKnownPlayerPos) <= 0.5f)
        {
            searchTimer -= Time.fixedDeltaTime;
            if (searchTimer <= 0f)
            {
                state = State.Wander;
                PickNewWanderTarget();

                if (showDebugLogs)
                    Debug.Log($"[Enemy AI] Búsqueda terminada. Volviendo a deambular.");
            }
        }
    }

    void MoveWithAvoidance(Vector2 target, float speed)
    {
        Vector2 directionToTarget = (target - rb.position).normalized;
        Vector2 avoidanceVector = CalculateAvoidanceVector(directionToTarget);
        Vector2 finalDirection = (directionToTarget + avoidanceVector).normalized;
        Vector2 newPos = rb.position + finalDirection * speed * Time.fixedDeltaTime;
        rb.MovePosition(newPos);
    }

    Vector2 CalculateAvoidanceVector(Vector2 forward)
    {
        Vector2 avoidance = Vector2.zero;
        int hitCount = 0;

        for (int i = 0; i < avoidanceRayCount; i++)
        {
            float angleOffset = -avoidanceRaySpread / 2 + (avoidanceRaySpread / (avoidanceRayCount - 1)) * i;
            Vector2 rayDirection = Rotate(forward, angleOffset);

            RaycastHit2D hit = Physics2D.Raycast(rb.position, rayDirection, avoidanceRayDistance, wallsMask);

            Debug.DrawRay(rb.position, rayDirection * avoidanceRayDistance,
                hit.collider != null ? Color.red : Color.green);

            if (hit.collider != null)
            {
                float strength = 1f - (hit.distance / avoidanceRayDistance);
                Vector2 avoidDir = new Vector2(-rayDirection.y, rayDirection.x);

                if (angleOffset < 0) avoidDir = -avoidDir;

                avoidance += avoidDir * strength * avoidanceForce;
                hitCount++;
            }
        }

        if (hitCount > 0)
        {
            avoidance /= hitCount;
        }

        return avoidance;
    }

    bool IsPathClear(Vector2 from, Vector2 to)
    {
        Vector2 direction = (to - from);
        float distance = direction.magnitude;
        direction.Normalize();

        int steps = Mathf.Min(Mathf.CeilToInt(distance / pathCheckDistance), maxPathCheckSteps);

        for (int i = 1; i <= steps; i++)
        {
            float checkDist = (distance / steps) * i;
            Vector2 checkPoint = from + direction * checkDist;

            Collider2D hitCollider = Physics2D.OverlapCircle(checkPoint, 0.3f, wallsMask);
            if (hitCollider != null)
            {
                return false;
            }
        }

        return true;
    }

    void PickNewWanderTarget()
    {
        for (int attempt = 0; attempt < 30; attempt++)
        {
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            if (randomDir == Vector2.zero) continue;

            float distance = Random.Range(stepDistanceMin, stepDistanceMax);

            RaycastHit2D wallHit = Physics2D.Raycast(rb.position, randomDir, distance, wallsMask);

            Vector2 candidate;
            if (wallHit.collider != null)
            {
                candidate = wallHit.point - randomDir * wallMargin;
            }
            else
            {
                candidate = rb.position + randomDir * distance;
            }

            if (roamArea != null && !roamArea.OverlapPoint(candidate))
                continue;

            if (Physics2D.OverlapCircle(candidate, 0.3f, wallsMask) != null)
                continue;

            if (!IsPathClear(rb.position, candidate))
                continue;

            wanderTarget = candidate;
            return;
        }

        wanderTarget = rb.position + Random.insideUnitCircle * 2f;
    }

    void PickEmergencyWanderTarget()
    {
        if (showDebugLogs)
            Debug.Log($"[Enemy AI] Usando objetivo de emergencia - muy atascado!");

        float[] angles = { 0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f };

        foreach (float angle in angles)
        {
            Vector2 direction = Rotate(Vector2.right, angle);
            float maxDist = stepDistanceMax * 2f;

            RaycastHit2D hit = Physics2D.Raycast(rb.position, direction, maxDist, wallsMask);

            Vector2 candidate;
            if (hit.collider != null)
            {
                candidate = hit.point - direction * (wallMargin * 2f);
            }
            else
            {
                candidate = rb.position + direction * maxDist;
            }

            if (roamArea != null && !roamArea.OverlapPoint(candidate))
                continue;

            if (Physics2D.OverlapCircle(candidate, 0.3f, wallsMask) != null)
                continue;

            wanderTarget = candidate;
            return;
        }

        if (roamArea != null)
        {
            wanderTarget = roamArea.bounds.center;
        }
    }

    Vector2 Rotate(Vector2 v, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);
        return new Vector2(
            v.x * cos - v.y * sin,
            v.x * sin + v.y * cos
        );
    }

    bool IsPlayerHidden(Transform p)
    {
        if (!p.gameObject.activeInHierarchy) return true;

        var r = p.GetComponent<SpriteRenderer>();
        if (r != null && !r.enabled) return true;

        var c = p.GetComponent<Collider2D>();
        if (c != null && !c.enabled) return true;

        var hs = p.GetComponent<PlayerHideState>();
        if (hs != null && hs.IsHidden) return true;

        return false;
    }

    void OnDrawGizmosSelected()
    {
        if (rb == null) return;

        // Objetivo de wander
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere((Vector3)wanderTarget, 0.5f);
        Gizmos.DrawLine((Vector3)rb.position, (Vector3)wanderTarget);

        // Rango de aggro
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere((Vector3)rb.position, aggroRange);

        // Rango de lose
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere((Vector3)rb.position, loseRange);

        // Línea hacia el jugador en chase
        if (state == State.Chase && player != null)
        {
            Vector2 dirToPlayer = ((Vector2)player.position - rb.position).normalized;
            Gizmos.color = Color.blue;
            Gizmos.DrawLine((Vector3)rb.position, (Vector3)rb.position + (Vector3)dirToPlayer * wallCheckDistance);
        }
    }
}
