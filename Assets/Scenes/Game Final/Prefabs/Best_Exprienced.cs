using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Best_Exprienced : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float delayBeforeFadeIn = 2f;
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float visibleDuration = 3f;
    [SerializeField] private float fadeOutDuration = 1f;

    void Start()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        Invoke(nameof(FadeIn), delayBeforeFadeIn);
    }

    void FadeIn()
    {
        canvasGroup.DOFade(1f, fadeInDuration).OnComplete(() =>
        {
            Invoke(nameof(FadeOut), visibleDuration);
        });
    }

    void FadeOut()
    {
        canvasGroup.DOFade(0f, fadeOutDuration);
    }
}
