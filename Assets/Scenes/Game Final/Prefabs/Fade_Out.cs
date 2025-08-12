using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Fade_Out : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 1f;

    void Start()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.DOFade(0f, fadeDuration);
        }
    }
}
