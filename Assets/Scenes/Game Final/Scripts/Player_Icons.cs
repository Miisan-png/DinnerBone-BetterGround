using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Player_Icons : MonoBehaviour
{
    [Header("Icon References")]
    [SerializeField] private Image icon1;
    [SerializeField] private Image icon2;

    [Header("Settings")]
    [SerializeField] private float startDelay = 3f; // Time before fade-out starts
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float floatUpDistance = 50f; // How much they float up on fade-out
    [SerializeField] private float idleFloatDistance = 5f; // Idle float distance
    [SerializeField] private float idleFloatTime = 1f; // Idle float cycle time

    private CanvasGroup cg1;
    private CanvasGroup cg2;

    private Vector3 icon1StartPos;
    private Vector3 icon2StartPos;

    private void Start()
    {
        // Get CanvasGroups
        cg1 = icon1.GetComponent<CanvasGroup>();
        cg2 = icon2.GetComponent<CanvasGroup>();

        // Store starting positions
        icon1StartPos = icon1.rectTransform.anchoredPosition;
        icon2StartPos = icon2.rectTransform.anchoredPosition;

        // Set default alpha to 1
        cg1.alpha = 1f;
        cg2.alpha = 1f;

        // Start idle float
        StartIdleFloat(icon1.rectTransform);
        StartIdleFloat(icon2.rectTransform);

        // Start fade-out after delay
        Invoke(nameof(StartFadeOut), startDelay);
    }

    private void StartIdleFloat(RectTransform target)
    {
        // Ping-pong float idle loop
        target.DOAnchorPosY(target.anchoredPosition.y + idleFloatDistance, idleFloatTime)
              .SetEase(Ease.InOutSine)
              .SetLoops(-1, LoopType.Yoyo);
    }

    private void StartFadeOut()
    {
        // Kill idle loops so we can animate fade-out
        icon1.rectTransform.DOKill();
        icon2.rectTransform.DOKill();

        // Fade and float up
        icon1.rectTransform.DOAnchorPosY(icon1StartPos.y + floatUpDistance, fadeDuration).SetEase(Ease.OutQuad);
        icon2.rectTransform.DOAnchorPosY(icon2StartPos.y + floatUpDistance, fadeDuration).SetEase(Ease.OutQuad);

        cg1.DOFade(0f, fadeDuration);
        cg2.DOFade(0f, fadeDuration);
    }
}
