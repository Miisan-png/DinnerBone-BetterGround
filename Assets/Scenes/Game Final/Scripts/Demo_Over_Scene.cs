using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System.Collections;

public class Demo_Over_Scene : MonoBehaviour
{
    [Header("Fade Canvas")]
    [SerializeField] private CanvasGroup fadeCanvas;
    [SerializeField] private float fadeDuration = 1f;

    [Header("Menu Image Color")]
    [SerializeField] private RawImage menuImage;
    [SerializeField] private Color startColor = Color.black;
    [SerializeField] private Color endColor = Color.white;
    [SerializeField] private float colorLerpDuration = 2f;
    [SerializeField] private Ease colorEase = Ease.InOutSine;

    [Header("Typewriter")]
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private float charInterval = 0.03f;

    void Start()
    {
        if (menuImage) menuImage.color = startColor;
        if (typeText)
        {
            typeText.ForceMeshUpdate();
            typeText.maxVisibleCharacters = 0;
        }
        StartCoroutine(RunSequence());
    }

    IEnumerator RunSequence()
    {
        if (fadeCanvas)
        {
            fadeCanvas.alpha = 1f;
            yield return fadeCanvas.DOFade(0f, fadeDuration).WaitForCompletion();
        }

        if (menuImage) menuImage.DOColor(endColor, colorLerpDuration).SetEase(colorEase);
        if (typeText) StartCoroutine(TypewriterRoutine(typeText, charInterval));
    }

    IEnumerator TypewriterRoutine(TextMeshProUGUI tmp, float interval)
    {
        tmp.ForceMeshUpdate();
        int total = tmp.textInfo.characterCount;
        tmp.maxVisibleCharacters = 0;
        for (int i = 1; i <= total; i++)
        {
            tmp.maxVisibleCharacters = i;
            yield return new WaitForSeconds(interval);
        }
    }
}
