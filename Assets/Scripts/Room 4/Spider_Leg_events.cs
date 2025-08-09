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

    [Header("Lock-on Settings")]
    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private Transform legPivot;

    private bool isAttacking = false;
    private Transform targetPlayer;
    private Quaternion originalRotation;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (!impactPoint) impactPoint = transform;
        if (!legPivot) legPivot = transform;
        
        originalRotation = legPivot.rotation;
        
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
    }

    public void StartAttack()
    {
        isAttacking = true;
        targetPlayer = FindClosestPlayer();
        
        if (targetPlayer != null)
        {
            RotateTowardsPlayer();
        }
    }

    public void EndAttack()
    {
        isAttacking = false;
        targetPlayer = null;
        legPivot.DORotateQuaternion(originalRotation, 0.5f);
    }

    public void OnImpact()
    {
        PlayImpactVFX();
        ShakeCamera();
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

    void Update()
    {
        if (isAttacking && targetPlayer != null)
        {
            RotateTowardsPlayer();
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

    void OnDrawGizmosSelected()
    {
        if (detectionRadius != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(detectionRadius.transform.position, detectionRange);
        }
    }
}