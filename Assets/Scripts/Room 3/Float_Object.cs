using UnityEngine;
using DG.Tweening;

public class Float_Object : MonoBehaviour
{
    [Header("Floating Settings")]
    [SerializeField] private float floatDistance = 0.3f; // How far up/down it moves
    [SerializeField] private float floatDuration = 2f;   // Time for one up/down cycle
    [SerializeField] private Ease easeType = Ease.InOutSine; // Smooth movement

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;

        transform.DOMoveY(startPos.y + floatDistance, floatDuration)
            .SetEase(easeType)
            .SetLoops(-1, LoopType.Yoyo);
    }
}
