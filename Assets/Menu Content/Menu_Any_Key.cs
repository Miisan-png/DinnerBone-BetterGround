using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;

public class Menu_Press_Any_Key : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI pressAnyKeyText;
    public CanvasGroup fadeCanvasGroup;
    
    [Header("Scene Settings")]
    public string nextSceneName = "GameScene";
    
    [Header("Animation Settings")]
    public float fadeInDuration = 1f;
    public float fadeOutDuration = 1f;
    public float floatDistance = 20f;
    public float floatDuration = 2f;

    private Vector3 originalTextPosition;
    private bool canPressKey = false;

    void Start()
    {
        fadeCanvasGroup.alpha = 1f;
        originalTextPosition = pressAnyKeyText.transform.localPosition;
        
        fadeCanvasGroup.DOFade(0f, fadeInDuration).OnComplete(() =>
        {
            canPressKey = true;
            StartFloatingAnimation();
        });
    }


    void Update()
    {
        if (canPressKey && Input.anyKeyDown)
        {
            canPressKey = false;
            pressAnyKeyText.transform.DOKill();
            
            fadeCanvasGroup.DOFade(1f, fadeOutDuration).OnComplete(() =>
            {
                SceneManager.LoadScene(nextSceneName);
            });
        }
    }

    void StartFloatingAnimation()
    {
        pressAnyKeyText.transform.DOLocalMoveY(originalTextPosition.y + floatDistance, floatDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    void OnDestroy()
    {
        if (pressAnyKeyText != null)
            pressAnyKeyText.transform.DOKill();
        if (fadeCanvasGroup != null)
            fadeCanvasGroup.DOKill();
    }
}