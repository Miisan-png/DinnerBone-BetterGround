using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System.Collections;

public class Demo_Over_Scene : MonoBehaviour
{
    [Header("Intro Image")]
    [SerializeField] private Image introImage;                 // intro logo/splash
    [SerializeField] private float introDuration = 3f;         // how long intro stays

    [Header("Fade Canvas")]
    [SerializeField] private CanvasGroup fadeCanvas;
    [SerializeField] private float fadeDuration = 1f;

    [Header("Menu Canvas Group (Image + Text Holder)")]
    [SerializeField] private CanvasGroup menuGroup;            // parent group of menuImage + text

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
        // Show intro immediately
        if (introImage) introImage.gameObject.SetActive(true);

        // Menu stuff hidden until intro finishes
        if (menuGroup) menuGroup.alpha = 0f;
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
        // Wait for intro duration
        yield return new WaitForSeconds(introDuration);

        // Hide intro image
        if (introImage) introImage.gameObject.SetActive(false);

        // --- Fade overlay canvas ---
        if (fadeCanvas)
        {
            fadeCanvas.alpha = 1f;
            yield return fadeCanvas.DOFade(0f, fadeDuration).WaitForCompletion();
        }

        // --- Reveal menu group ---
        if (menuGroup)
        {
            yield return menuGroup.DOFade(1f, 0.5f).WaitForCompletion();
        }

        // --- Transition background color ---
        if (menuImage) menuImage.DOColor(endColor, colorLerpDuration).SetEase(colorEase);

        // --- Typewriter text ---
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
