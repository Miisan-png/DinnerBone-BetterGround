using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class SpiderPathAI : MonoBehaviour
{
    public Animator animator;
    public Transform[] pivots;
    public bool loop = true;
    public float runSpeed = 3f;
    public float rotationSpeed = 360f;
    public float pivotReachDistance = 0.25f;

    public float idlePauseChance = 0.2f;
    public Vector2 idleDurationRange = new Vector2(0.6f, 1.4f);

    public Transform luthe;
    public Transform cherie;
    public float detectRadius = 6f;
    public float loseRadius = 9f;
    public bool requireLineOfSight = true;
    public LayerMask obstacleMask;

    public SpiderDeathZone[] deathZones;
    public float spawnGraceTime = 0.25f;

    public LayerMask groundMask;
    public float probeHeight = 1.5f;
    public float probeExtra = 0.5f;
    public float groundStickForce = 25f;
    public float hoverHeight = 0.02f;

    Rigidbody rb;
    CapsuleCollider col;
    int pivotIndex;
    bool isChasing;
    Transform chaseTarget;
    bool isIdle;
    float idleTimer;
    float spawnTime;
    bool grounded;
    Vector3 groundNormal = Vector3.up;
    float groundY;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void Start()
    {
        pivotIndex = 0;
        spawnTime = Time.time;
        if (animator) animator.SetBool("run", true);
        ProbeGround(true);
    }

    void Update()
    {
        ProbeGround(false);

        SelectChaseTarget();

        if (!isChasing && grounded && !isIdle && Random.value < idlePauseChance * Time.deltaTime)
        {
            isIdle = true;
            idleTimer = Random.Range(idleDurationRange.x, idleDurationRange.y);
            if (animator) animator.SetBool("run", false);
        }

        if (isIdle)
        {
            idleTimer -= Time.deltaTime;
            if (idleTimer <= 0f)
            {
                isIdle = false;
                if (animator) animator.SetBool("run", true);
            }
            return;
        }

        Vector3 tgt = isChasing && chaseTarget ? chaseTarget.position : GetPivotPosition();
        Vector3 dir = tgt - transform.position; dir.y = 0f;

        if (!isChasing && dir.magnitude <= pivotReachDistance) AdvancePivot();

        if (dir.sqrMagnitude > 0.0001f)
        {
            Quaternion look = Quaternion.LookRotation(dir.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, look, rotationSpeed * Time.deltaTime);
        }
    }

    void FixedUpdate()
    {
        if (isIdle)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            return;
        }

        Vector3 forward = transform.forward;
        if (grounded) forward = Vector3.ProjectOnPlane(forward, groundNormal).normalized;

        Vector3 horizVel = forward * runSpeed;
        rb.linearVelocity = new Vector3(horizVel.x, rb.linearVelocity.y, horizVel.z);

        if (grounded)
        {
            rb.AddForce(Vector3.down * groundStickForce, ForceMode.Acceleration);
            float targetY = groundY + hoverHeight;
            if (rb.position.y > targetY)
            {
                Vector3 p = rb.position;
                p.y = Mathf.Lerp(p.y, targetY, 0.35f);
                rb.MovePosition(p);
            }
        }
    }

    void ProbeGround(bool snapNow)
    {
        Vector3 top = transform.position + Vector3.up * probeHeight;
        float castDist = probeHeight + probeExtra + col.height * 0.5f;
        float radius = Mathf.Max(0.01f, col.radius * 0.95f);
        if (Physics.SphereCast(top, radius, Vector3.down, out var hit, castDist, groundMask))
        {
            grounded = true;
            groundNormal = hit.normal;
            groundY = hit.point.y;
            if (snapNow)
            {
                Vector3 p = rb.position; p.y = groundY + hoverHeight; rb.position = p;
            }
        }
        else
        {
            grounded = false;
            groundNormal = Vector3.up;
        }
    }

    void SelectChaseTarget()
    {
        Transform best = null; float bestDist = float.MaxValue;
        TryCandidate(luthe, ref best, ref bestDist);
        TryCandidate(cherie, ref best, ref bestDist);
        if (best != null) { isChasing = true; chaseTarget = best; } else if (isChasing) { isChasing = false; chaseTarget = null; }
    }

    void TryCandidate(Transform t, ref Transform best, ref float bestDist)
    {
        if (!t) return;
        float d = Vector3.Distance(transform.position, t.position);
        if (isChasing && chaseTarget == t)
        {
            if (d <= loseRadius && (!requireLineOfSight || HasLOS(t))) { if (d < bestDist) { best = t; bestDist = d; } }
            return;
        }
        if (d <= detectRadius && (!requireLineOfSight || HasLOS(t))) { if (d < bestDist) { best = t; bestDist = d; } }
    }

    bool HasLOS(Transform t)
    {
        Vector3 a = transform.position + Vector3.up * 0.5f;
        Vector3 b = t.position + Vector3.up * 0.5f;
        if (Physics.Linecast(a, b, out var hit, obstacleMask))
            return hit.transform == t || hit.transform.IsChildOf(t);
        return true;
    }

    Vector3 GetPivotPosition()
    {
        if (pivots == null || pivots.Length == 0) return transform.position;
        Transform p = pivots[Mathf.Clamp(pivotIndex, 0, pivots.Length - 1)];
        return p ? p.position : transform.position;
    }

    void AdvancePivot()
    {
        if (pivots == null || pivots.Length == 0) return;
        pivotIndex++;
        if (pivotIndex >= pivots.Length) pivotIndex = loop ? 0 : pivots.Length - 1;
    }

    void OnTriggerEnter(Collider other)
    {
        if (Time.time - spawnTime < spawnGraceTime) return;
        var dz = other.GetComponent<SpiderDeathZone>();
        if (!dz) return;
        for (int i = 0; i < deathZones.Length; i++)
        {
            if (deathZones[i] == dz) { Destroy(gameObject); return; }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        if (pivots != null && pivots.Length > 0)
        {
            for (int i = 0; i < pivots.Length; i++)
            {
                if (!pivots[i]) continue;
                Gizmos.DrawWireSphere(pivots[i].position, 0.2f);
                int j = (i + 1) % pivots.Length;
                if (loop && pivots.Length > 1 && pivots[j]) Gizmos.DrawLine(pivots[i].position, pivots[j].position);
                else if (i + 1 < pivots.Length && pivots[i + 1]) Gizmos.DrawLine(pivots[i].position, pivots[i + 1].position);
            }
        }
    }
}
