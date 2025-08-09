using UnityEngine;
using DG.Tweening;

public class SpiderLegEvents : MonoBehaviour
{
    [Header("Impact VFX")]
    [SerializeField] private GameObject impactVFX;   // Particle prefab
    [SerializeField] private Transform impactPoint;  // Where to spawn (optional)
    [SerializeField] private float vfxAutoDestroyAfter = 3f;

    [Header("Camera Shake")]
    [SerializeField] private Camera cam;                     
    [SerializeField] private Vector3 shakeStrength = new Vector3(0.12f, 0.12f, 0f);
    [SerializeField] private float shakeDuration = 0.15f;
    [SerializeField] private int shakeVibrato = 10;
    [SerializeField] private float shakeRandomness = 90f;
    [SerializeField] private bool unscaledTime = true;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (!impactPoint) impactPoint = transform; // fallback
    }

    /// <summary>
    /// Call this from Animation Event at exact impact frame
    /// </summary>
    public void OnImpact()
    {
        PlayImpactVFX();
        ShakeCamera();
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

        // Cancel any existing shake to prevent stacking
        cam.transform.DOKill();

        // Smooth positional shake
        var t = cam.transform.DOShakePosition(
            shakeDuration,
            shakeStrength,
            shakeVibrato,
            shakeRandomness,
            fadeOut: true,  // Smooth return to normal
            snapping: false
        );

        if (unscaledTime) t.SetUpdate(true);
    }
}
