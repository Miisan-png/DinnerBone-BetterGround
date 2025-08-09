using UnityEngine;
using DG.Tweening;

[DisallowMultipleComponent]
public class Level_4_Camera : MonoBehaviour
{
    public enum ZoomMode { OrthographicSize, PerspectiveDistance, PerspectiveFOV }

    [Header("Players")]
    [SerializeField] private Transform player1;
    [SerializeField] private Transform player2;

    [Header("Camera")]
    [SerializeField] private Camera targetCamera; // If null, will use Camera.main
    [SerializeField] private float fixedY = 0f;   // Side scroller height
    [SerializeField] private float fixedZ = -10f; // Used for Ortho & FOV modes

    [Header("Follow (X only)")]
    [SerializeField] private float followDuration = 0.20f;
    [SerializeField] private float lookAheadMultiplier = 0.50f;
    [SerializeField] private float maxLookAhead = 2.0f;

    [Header("Dynamic Zoom")]
    [SerializeField] private bool useDynamicZoom = true;
    [SerializeField] private ZoomMode zoomMode = ZoomMode.OrthographicSize;
    [SerializeField] private float horizontalPadding = 3f; // extra width around players
    [SerializeField] private float zoomDuration = 0.25f;

    // Orthographic limits
    [SerializeField] private float minOrthoSize = 5f;
    [SerializeField] private float maxOrthoSize = 9f;

    // Perspective (distance) limits — camera slides on Z to zoom
    [SerializeField] private float minDistance = 8f;   // closer (more zoomed-in)
    [SerializeField] private float maxDistance = 20f;  // farther (zoomed-out)

    // Perspective (FOV) limits — camera stays at fixedZ, adjusts FOV
    [SerializeField] private float minFOV = 35f; // more zoomed-in
    [SerializeField] private float maxFOV = 60f; // more zoomed-out

    [Header("Room Bounds (X)")]
    [SerializeField] private bool clampToBounds = true;
    [SerializeField] private float leftBoundX = -50f;
    [SerializeField] private float rightBoundX = 50f;

    // Internals
    private Vector3 p1Prev, p2Prev;
    private Tweener moveTween;
    private Tweener zoomTween;

    void Awake()
    {
        if (!targetCamera) targetCamera = GetComponent<Camera>();
        if (!targetCamera) targetCamera = Camera.main;

        if (!targetCamera)
        {
            Debug.LogWarning("[Level_4_Camera] No Camera found! Assign one to 'targetCamera' in the inspector.");
        }
    }

    void Start()
    {
        if (player1) p1Prev = player1.position;
        if (player2) p2Prev = player2.position;

        // Ensure reasonable default position
        if (targetCamera && zoomMode != ZoomMode.PerspectiveDistance)
        {
            // For Ortho and FOV modes Z stays fixed
            var p = transform.position;
            transform.position = new Vector3(p.x, fixedY, fixedZ);
        }
    }

    void LateUpdate()
    {
        if (!player1 && !player2) return;
        if (!targetCamera) return;

        // ---------------- X target (midpoint + lookahead) ----------------
        float x1 = player1 ? player1.position.x : (player2 ? player2.position.x : transform.position.x);
        float x2 = player2 ? player2.position.x : x1;
        float midX = (x1 + x2) * 0.5f;

        float v1 = 0f, v2 = 0f;
        float dt = Mathf.Max(Time.deltaTime, 1e-5f);

        if (player1) { v1 = (player1.position.x - p1Prev.x) / dt; p1Prev = player1.position; }
        if (player2) { v2 = (player2.position.x - p2Prev.x) / dt; p2Prev = player2.position; }

        float avgV = (v1 + v2) * 0.5f;
        float lookAhead = Mathf.Clamp(avgV * lookAheadMultiplier, -maxLookAhead, maxLookAhead);

        float targetX = midX + lookAhead;

        // ---------------- Dynamic Zoom ----------------
        if (useDynamicZoom)
        {
            float playersDistX = Mathf.Abs(x1 - x2);
            float desiredHalfWidth = playersDistX * 0.5f + horizontalPadding;

            switch (zoomMode)
            {
                case ZoomMode.OrthographicSize:
                    if (targetCamera.orthographic)
                    {
                        float desiredOrtho = desiredHalfWidth / Mathf.Max(targetCamera.aspect, 0.01f);
                        desiredOrtho = Mathf.Clamp(desiredOrtho, minOrthoSize, maxOrthoSize);
                        zoomTween?.Kill();
                        zoomTween = targetCamera.DOOrthoSize(desiredOrtho, zoomDuration).SetEase(Ease.OutQuad);
                    }
                    else
                    {
                        // Helpful warning if camera isn't set to Orthographic
                        // (You can switch to PerspectiveDistance or PerspectiveFOV instead)
                        // Only log occasionally to avoid spam:
                        // Debug.LogWarning("[Level_4_Camera] Camera is Perspective; switch zoomMode or set camera to Orthographic.");
                    }
                    break;

                case ZoomMode.PerspectiveDistance:
                {
                    // Slide camera on Z to fit desired width (no FOV distortion)
                    // Compute horizontal FOV from current vertical FOV
                    float vFOV = targetCamera.fieldOfView * Mathf.Deg2Rad;
                    float hFOV = 2f * Mathf.Atan(Mathf.Tan(vFOV * 0.5f) * targetCamera.aspect);
                    float requiredDistance = desiredHalfWidth / Mathf.Max(Mathf.Tan(hFOV * 0.5f), 0.001f);

                    float clamped = Mathf.Clamp(requiredDistance, minDistance, maxDistance);

                    // Move along world Z (typical side scroller). If your camera is rotated,
                    // you can move along -transform.forward instead.
                    zoomTween?.Kill();
                    zoomTween = transform.DOMoveZ(-clamped, zoomDuration).SetEase(Ease.OutQuad);
                    break;
                }

                case ZoomMode.PerspectiveFOV:
                {
                    // Keep Z fixed, change FOV
                    float distance = Mathf.Abs(transform.position.z); // assumes camera faces +Z from negative Z
                    if (distance < 0.001f) distance = Mathf.Abs(fixedZ);

                    // Compute needed hFOV to fit desiredHalfWidth at this distance
                    float desiredHFOV = 2f * Mathf.Atan(desiredHalfWidth / Mathf.Max(distance, 0.001f));
                    // Convert desired hFOV back to vertical FOV using aspect
                    float desiredVFOV = 2f * Mathf.Atan(Mathf.Tan(desiredHFOV * 0.5f) / Mathf.Max(targetCamera.aspect, 0.001f));
                    float desiredFOVDeg = Mathf.Clamp(desiredVFOV * Mathf.Rad2Deg, minFOV, maxFOV);

                    zoomTween?.Kill();
                    zoomTween = targetCamera.DOFieldOfView(desiredFOVDeg, zoomDuration).SetEase(Ease.OutQuad);

                    // Ensure Z is our fixedZ in this mode
                    var p = transform.position;
                    transform.position = new Vector3(p.x, fixedY, fixedZ);
                    break;
                }
            }
        }

        // ---------------- Clamp X to bounds ----------------
        if (clampToBounds)
        {
            if (targetCamera.orthographic && zoomMode == ZoomMode.OrthographicSize)
            {
                float halfWidth = targetCamera.orthographicSize * targetCamera.aspect;
                float minX = leftBoundX + halfWidth;
                float maxX = rightBoundX - halfWidth;
                targetX = (minX > maxX) ? (leftBoundX + rightBoundX) * 0.5f : Mathf.Clamp(targetX, minX, maxX);
            }
            else
            {
                // Perspective: simpler clamp (doesn't account for width)
                targetX = Mathf.Clamp(targetX, leftBoundX, rightBoundX);
            }
        }

        // ---------------- Smooth X move ----------------
        moveTween?.Kill();
        moveTween = transform.DOMoveX(targetX, followDuration).SetEase(Ease.OutQuad);

        // Keep Y fixed (and Z if mode expects it)
        var pos = transform.position;
        if (zoomMode == ZoomMode.PerspectiveDistance)
            transform.position = new Vector3(pos.x, fixedY, pos.z);
        else
            transform.position = new Vector3(pos.x, fixedY, fixedZ);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (clampToBounds)
        {
            Gizmos.color = Color.cyan;
            Vector3 a = new Vector3(leftBoundX, fixedY, 0f);
            Vector3 b = new Vector3(rightBoundX, fixedY, 0f);
            Gizmos.DrawLine(a + Vector3.up * 10f, a + Vector3.down * 10f);
            Gizmos.DrawLine(b + Vector3.up * 10f, b + Vector3.down * 10f);
        }
    }
#endif
}
