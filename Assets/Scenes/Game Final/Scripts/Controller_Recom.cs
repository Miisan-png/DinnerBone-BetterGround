using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DG.Tweening;

public class Controller_Recom : MonoBehaviour
{
    [System.Serializable]
    public class ShowcaseSet
    {
        public string title;
        public Sprite[] controllerSprites;
        public Sprite[] keyboardSprites;
    }

    [Header("UI References")]
    [SerializeField] private CanvasGroup rootCanvas;
    [SerializeField] private TextMeshProUGUI headTitleLabel;
    [SerializeField] private TextMeshProUGUI titleLabel;
    [SerializeField] private Image controllerImage;
    [SerializeField] private Image keyboardImage;

    [Header("Showcase Settings")]
    [SerializeField] private string headTitle = "Recommended to Play with a Controller";
    [SerializeField] private ShowcaseSet[] showcases;
    [SerializeField] private float showcaseDuration = 4f;
    [SerializeField] private float flipInterval = 1f;
    [SerializeField] private float fadeDuration = 0.5f;

    private int currentShowcaseIndex = 0;
    private int controllerIndex = 0;
    private int keyboardIndex = 0;
    private bool flipping = false;

    void Start()
    {
        if (rootCanvas != null) rootCanvas.alpha = 0;
        StartCoroutine(PresentationSequence());
    }

    IEnumerator PresentationSequence()
    {
        if (rootCanvas != null)
            yield return rootCanvas.DOFade(1f, fadeDuration).WaitForCompletion();

        if (headTitleLabel != null)
        {
            headTitleLabel.text = headTitle;
            yield return new WaitForSeconds(1f);
        }

        for (currentShowcaseIndex = 0; currentShowcaseIndex < showcases.Length; currentShowcaseIndex++)
        {
            var set = showcases[currentShowcaseIndex];
            if (titleLabel != null) titleLabel.text = set.title;

            controllerIndex = 0;
            keyboardIndex = 0;

            if (set.controllerSprites.Length > 0 && controllerImage != null)
                controllerImage.sprite = set.controllerSprites[0];
            if (set.keyboardSprites.Length > 0 && keyboardImage != null)
                keyboardImage.sprite = set.keyboardSprites[0];

            flipping = true;
            StartCoroutine(FlipImages(set));

            yield return new WaitForSeconds(showcaseDuration);

            flipping = false;
            yield return new WaitForSeconds(0.1f);
        }

        if (rootCanvas != null)
            yield return rootCanvas.DOFade(0f, fadeDuration).WaitForCompletion();
    }

    IEnumerator FlipImages(ShowcaseSet set)
    {
        while (flipping)
        {
            yield return new WaitForSeconds(flipInterval);

            if (set.controllerSprites.Length > 0 && controllerImage != null)
            {
                controllerIndex = (controllerIndex + 1) % set.controllerSprites.Length;
                controllerImage.DOFade(0f, fadeDuration / 2).OnComplete(() =>
                {
                    controllerImage.sprite = set.controllerSprites[controllerIndex];
                    controllerImage.DOFade(1f, fadeDuration / 2);
                });
            }

            if (set.keyboardSprites.Length > 0 && keyboardImage != null)
            {
                keyboardIndex = (keyboardIndex + 1) % set.keyboardSprites.Length;
                keyboardImage.DOFade(0f, fadeDuration / 2).OnComplete(() =>
                {
                    keyboardImage.sprite = set.keyboardSprites[keyboardIndex];
                    keyboardImage.DOFade(1f, fadeDuration / 2);
                });
            }
        }
    }
}
