using UnityEngine;
using DG.Tweening;

public class SpiderLegEvents : MonoBehaviour
{
    [Header("Impact VFX")]
    [SerializeField] private GameObject impactVFX;
    [SerializeField] private Transform impactPoint;
    [SerializeField] private float vfxAutoDestroyAfter = 3f;

    [Header("Camera Shake")]
    [SerializeField] private Camera cam;
    [SerializeField] private Vector3 shakeStrength = new Vector3(0.12f, 0.12f, 0f);
    [SerializeField] private float shakeDuration = 0.15f;
    [SerializeField] private int shakeVibrato = 10;
    [SerializeField] private float shakeRandomness = 90f;
    [SerializeField] private bool unscaledTime = true;

    [Header("Player Detection")]
    [SerializeField] private GameObject detectionRadius;
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private LayerMask playerLayerMask = -1;

    [Header("Player Hit Detection")]
    [SerializeField] private GameObject legTrigger;
    [SerializeField] private Player_Controller player1;
    [SerializeField] private Player_Controller player2;

    [Header("Attack Timing")]
    [SerializeField] private bool enableAutoAttack = true;
    [SerializeField] private float minAttackInterval = 2f;
    [SerializeField] private float maxAttackInterval = 5f;
    [SerializeField] private bool useRandomInterval = true;
    [SerializeField] private float fixedAttackInterval = 3f;

    [Header("Lock-on Settings")]
    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private Transform legPivot;
    [SerializeField] private Transform legRoot;
    [SerializeField] private bool enableRootRotation = true;
    [SerializeField] private float rootRotationSpeed = 1f;

    [Header("Hit Detection")]
    [SerializeField] private float hitRadius = 1f;
    [SerializeField] private Transform hitPoint;

    [Header("Debug")]
    [SerializeField] private bool debugAttackTiming = false;

    private bool isAttacking = false;
    private bool animationFinished = false;
    private Transform targetPlayer;
    private Quaternion originalRotation;
    private Vector3 originalPosition;
    private Quaternion originalRootRotation;
    private Spider_Boss_Player_State playerStateManager;
    
    private float nextAttackTime = 0f;
    private float currentAttackInterval;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (!impactPoint) impactPoint = transform;
        if (!legPivot) legPivot = transform;
        if (!legRoot) legRoot = transform.parent != null ? transform.parent : transform;
        
        originalRotation = legPivot.rotation;
        originalPosition = legRoot.position;
        originalRootRotation = legRoot.rotation;
        
        playerStateManager = FindObjectOfType<Spider_Boss_Player_State>();
        if (playerStateManager == null)
        {
            Debug.LogWarning("[SpiderLegEvents] No Spider_Boss_Player_State found in scene. Player hit effects will not work.");
        }
        
        if (legTrigger != null)
        {
            Collider triggerCollider = legTrigger.GetComponent<Collider>();
            if (triggerCollider == null)
            {
                triggerCollider = legTrigger.AddComponent<BoxCollider>();
            }
            triggerCollider.isTrigger = true;
            
            Spider_Leg_Trigger triggerScript = legTrigger.GetComponent<Spider_Leg_Trigger>();
            if (triggerScript == null)
            {
                triggerScript = legTrigger.AddComponent<Spider_Leg_Trigger>();
            }
            triggerScript.SetParentLeg(this);
        }
        
        if (detectionRadius != null)
        {
            Collider detectionCollider = detectionRadius.GetComponent<Collider>();
            if (detectionCollider == null)
            {
                detectionCollider = detectionRadius.AddComponent<SphereCollider>();
                ((SphereCollider)detectionCollider).radius = detectionRange;
            }
            detectionCollider.isTrigger = true;
        }
        
        SetNextAttackTime();
    }

    void Update()
    {
        if (enableAutoAttack && !isAttacking && Time.time >= nextAttackTime)
        {
            if (IsPlayerInRange())
            {
                StartAttack();
            }
            else
            {
                SetNextAttackTime();
            }
        }

        if (isAttacking && targetPlayer != null && !animationFinished)
        {
            RotateTowardsPlayer();
            if (enableRootRotation)
            {
                RotateRootTowardsPlayer();
            }
        }
    }

    private bool IsPlayerInRange()
    {
        if (detectionRadius == null) return false;

        Collider[] playersInRange = Physics.OverlapSphere(detectionRadius.transform.position, detectionRange, playerLayerMask);
        
        foreach (Collider col in playersInRange)
        {
            Player_Controller player = col.GetComponent<Player_Controller>();
            if (player != null)
            {
                return true;
            }
        }
        
        return false;
    }

    private void SetNextAttackTime()
    {
        currentAttackInterval = useRandomInterval
            ? Random.Range(minAttackInterval, maxAttackInterval)
            : fixedAttackInterval;
        
        nextAttackTime = Time.time + currentAttackInterval;
    }

    public void StartAttack()
    {
        if (isAttacking) return;
        
        isAttacking = true;
        animationFinished = false;
        targetPlayer = FindClosestPlayer();
        
        if (targetPlayer != null)
        {
            RotateTowardsPlayer();
            if (enableRootRotation)
            {
                RotateRootTowardsPlayer();
            }
        }
        
        Animator legAnimator = GetComponent<Animator>();
        if (legAnimator != null)
        {
            legAnimator.SetTrigger("Attack");
        }
    }

    public void OnAnimationFinished()
    {
        animationFinished = true;
    }

    public void EndAttack()
    {
        isAttacking = false;
        animationFinished = false;
        targetPlayer = null;
        
        legPivot.DORotateQuaternion(originalRotation, 0.5f);
        if (enableRootRotation)
        {
            legRoot.DORotateQuaternion(originalRootRotation, 0.8f).SetEase(Ease.OutQuad);
        }
        
        SetNextAttackTime();
    }

    public void OnImpact()
    {
        PlayImpactVFX();
        ShakeCamera();
        CheckForPlayerHit();
    }

    public void OnPlayerHitByTrigger(Player_Controller hitPlayer)
    {
        if (playerStateManager != null && hitPlayer != null)
        {
            playerStateManager.OnPlayerHit(hitPlayer);
        }
    }

    private Transform FindClosestPlayer()
    {
        if (detectionRadius == null) return null;

        Collider[] playersInRange = Physics.OverlapSphere(detectionRadius.transform.position, detectionRange, playerLayerMask);
        
        Transform closestPlayer = null;
        float closestDistance = float.MaxValue;

        foreach (Collider col in playersInRange)
        {
            Player_Controller player = col.GetComponent<Player_Controller>();
            if (player != null)
            {
                float distance = Vector3.Distance(detectionRadius.transform.position, col.transform.position);
                
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPlayer = col.transform;
                }
            }
        }

        return closestPlayer;
    }

    private void RotateTowardsPlayer()
    {
        if (targetPlayer == null || legPivot == null) return;

        Vector3 directionToPlayer = (targetPlayer.position - legPivot.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
        
        legPivot.DORotateQuaternion(lookRotation, 1f / rotationSpeed).SetEase(Ease.OutQuad);
    }

    private void RotateRootTowardsPlayer()
    {
        if (targetPlayer == null || legRoot == null) return;

        Vector3 directionToPlayer = (targetPlayer.position - legRoot.position).normalized;
        directionToPlayer.y = 0;
        
        Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
        legRoot.DORotateQuaternion(lookRotation, 1f / rootRotationSpeed).SetEase(Ease.OutQuad);
    }

    private void CheckForPlayerHit()
    {
        if (hitPoint == null) return;

        Collider[] hitColliders = Physics.OverlapSphere(hitPoint.position, hitRadius, playerLayerMask);
        
        foreach (Collider col in hitColliders)
        {
            Player_Controller player = col.GetComponent<Player_Controller>();
            if (player != null && playerStateManager != null)
            {
                playerStateManager.OnPlayerHit(player);
            }
        }
    }

    public void PlayImpactVFX()
    {
        if (!impactVFX || !impactPoint) return;

        var vfx = Instantiate(impactVFX, impactPoint.position, impactPoint.rotation);
        if (vfxAutoDestroyAfter > 0f) Destroy(vfx, vfxAutoDestroyAfter);
    }

    public void ShakeCamera()
    {
        if (!cam) return;

        cam.transform.DOKill();

        var t = cam.transform.DOShakePosition(
            shakeDuration,
            shakeStrength,
            shakeVibrato,
            shakeRandomness,
            fadeOut: true,
            snapping: false
        );

        if (unscaledTime) t.SetUpdate(true);
    }

    public void SetAttackInterval(float min, float max)
    {
        minAttackInterval = min;
        maxAttackInterval = max;
        SetNextAttackTime();
    }

    public void SetFixedAttackInterval(float interval)
    {
        fixedAttackInterval = interval;
        useRandomInterval = false;
        SetNextAttackTime();
    }

    public void EnableAutoAttack(bool enable)
    {
        enableAutoAttack = enable;
        if (enable)
        {
            SetNextAttackTime();
        }
    }

    [ContextMenu("Force Attack Now")]
    public void ForceAttackNow()
    {
        if (!isAttacking)
        {
            StartAttack();
        }
    }

    [ContextMenu("Reset Attack Timer")]
    public void ResetAttackTimer()
    {
        SetNextAttackTime();
    }

    void OnDrawGizmosSelected()
    {
        if (detectionRadius != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(detectionRadius.transform.position, detectionRange);
        }
        
        if (hitPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(hitPoint.position, hitRadius);
        }

        if (legTrigger != null)
        {
            Gizmos.color = Color.magenta;
            Collider triggerCollider = legTrigger.GetComponent<Collider>();
            if (triggerCollider != null)
            {
                Gizmos.matrix = legTrigger.transform.localToWorldMatrix;
                if (triggerCollider is BoxCollider box)
                {
                    Gizmos.DrawWireCube(box.center, box.size);
                }
                else if (triggerCollider is SphereCollider sphere)
                {
                    Gizmos.DrawWireSphere(sphere.center, sphere.radius);
                }
            }
        }
    }
}
