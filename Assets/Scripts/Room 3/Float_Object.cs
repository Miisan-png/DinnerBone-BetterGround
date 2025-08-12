using UnityEngine;
using DG.Tweening;

public class Float_Object : MonoBehaviour
{
    [Header("Floating Settings")]
    [SerializeField] private float floatDistance = 0.3f;
    [SerializeField] private float floatDuration = 2f;
    [SerializeField] private Ease easeType = Ease.InOutSine;
    
    [Header("Rotation Settings")]
    [SerializeField] private float maxRotationAngle = 5f; // Max degrees of rotation
    [SerializeField] private float rotationDuration = 3f; // Time for rotation cycle
    
    [Header("Randomization")]
    [SerializeField] private bool randomizeValues = true;
    [SerializeField] private float floatDurationRandomRange = 0.5f;
    [SerializeField] private float rotationDurationRandomRange = 1f;
    [SerializeField] private float startDelayRandomRange = 2f;

    private Vector3 startPos;
    private Vector3 startRot;
    private float randomizedFloatDuration;
    private float randomizedRotationDuration;
    private float randomizedStartDelay;

    void Start()
    {
        // Store initial position and rotation
        startPos = transform.position;
        startRot = transform.eulerAngles;
        
        // Apply randomization if enabled
        if (randomizeValues)
        {
            randomizedFloatDuration = floatDuration + Random.Range(-floatDurationRandomRange, floatDurationRandomRange);
            randomizedRotationDuration = rotationDuration + Random.Range(-rotationDurationRandomRange, rotationDurationRandomRange);
            randomizedStartDelay = Random.Range(0f, startDelayRandomRange);
        }
        else
        {
            randomizedFloatDuration = floatDuration;
            randomizedRotationDuration = rotationDuration;
            randomizedStartDelay = 0f;
        }
        
        // Start floating animation after delay
        Invoke("StartFloating", randomizedStartDelay);
    }

    void StartFloating()
    {
        // Floating up/down movement
        transform.DOMoveY(startPos.y + floatDistance, randomizedFloatDuration)
            .SetEase(easeType)
            .SetLoops(-1, LoopType.Yoyo);
        
        // Rotation animation - slightly different duration for more organic movement
        float rotDuration = randomizedRotationDuration * 1.3f;
        
        // Random small rotations on all axes
        Vector3 randomRot = new Vector3(
            Random.Range(-maxRotationAngle, maxRotationAngle),
            Random.Range(-maxRotationAngle, maxRotationAngle),
            Random.Range(-maxRotationAngle, maxRotationAngle)
        );
        
        transform.DORotate(startRot + randomRot, rotDuration)
            .SetEase(Ease.InOutQuad)
            .SetLoops(-1, LoopType.Yoyo);
    }
}