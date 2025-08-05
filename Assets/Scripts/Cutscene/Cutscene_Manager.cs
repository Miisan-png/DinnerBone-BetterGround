using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;

public class CutsceneManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI skipText;
    public Image fadePanel;
    public Camera cameraToShake;
    
    [Header("Scene Settings")]
    public string nextSceneName;
    public string defaultSkipText = "Hold any key to skip";
    
    [Header("Skip Settings")]
    public float holdTimeRequired = 2f;
    public float fadeInSpeed = 1f;
    public float fadeOutSpeed = 2f;
    public float breathingSpeed = 2f;
    public float breathingIntensity = 0.3f;
    
    [Header("Visual Effects")]
    public Color startColor = Color.white;
    public Color endColor = Color.red;
    public float shakeIntensity = 0.5f;
    public float shakeDuration = 0.3f;
    
    private float currentHoldTime = 0f;
    private bool isHolding = false;
    private bool isSkipping = false;
    private Tween breathingTween;
    private Tween skipFadeTween;
    private Tween colorTween;
    private Vector3 originalCameraPosition;
    
    void Start()
    {
        InitializeComponents();
        StartBreathingEffect();
    }
    
    void InitializeComponents()
    {
        if (skipText != null && string.IsNullOrEmpty(skipText.text))
        {
            skipText.text = defaultSkipText;
        }
        
        if (skipText != null)
        {
            skipText.color = startColor;
        }
        
        if (fadePanel != null)
        {
            Color panelColor = fadePanel.color;
            panelColor.a = 0f;
            fadePanel.color = panelColor;
        }
        
        if (cameraToShake != null)
        {
            originalCameraPosition = cameraToShake.transform.position;
        }
    }
    
    void StartBreathingEffect()
    {
        if (skipText != null)
        {
            float baseAlpha = skipText.color.a;
            breathingTween = skipText.DOFade(baseAlpha - breathingIntensity, breathingSpeed)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }
    }
    
    void Update()
    {
        if (isSkipping) return;
        
        bool anyKeyPressed = Input.anyKey;
        
        if (anyKeyPressed && !isHolding)
        {
            StartHolding();
        }
        else if (!anyKeyPressed && isHolding)
        {
            StopHolding();
        }
        
        if (isHolding)
        {
            currentHoldTime += Time.deltaTime;
            UpdateSkipProgress();
            
            if (currentHoldTime >= holdTimeRequired)
            {
                StartSkipSequence();
            }
        }
    }
    
    void StartHolding()
    {
        isHolding = true;
        
        if (breathingTween != null)
        {
            breathingTween.Kill();
        }
        
        if (skipText != null)
        {
            skipFadeTween?.Kill();
            colorTween?.Kill();
            
            skipFadeTween = skipText.DOFade(1f, 1f / fadeInSpeed);
            colorTween = skipText.DOColor(endColor, holdTimeRequired).SetEase(Ease.InOutQuad);
        }
    }
    
    void StopHolding()
    {
        isHolding = false;
        currentHoldTime = 0f;
        
        if (skipText != null)
        {
            skipFadeTween?.Kill();
            colorTween?.Kill();
            
            skipText.color = startColor;
            
            skipFadeTween = skipText.DOFade(0f, 1f / fadeOutSpeed)
                .OnComplete(() => {
                    if (!isHolding)
                    {
                        StartBreathingEffect();
                    }
                });
        }
    }
    
    void UpdateSkipProgress()
    {
        if (skipText != null)
        {
            float progress = Mathf.Clamp01(currentHoldTime / holdTimeRequired);
            
            Color currentColor = Color.Lerp(startColor, endColor, progress);
            currentColor.a = Mathf.Lerp(0.5f, 1f, progress);
            skipText.color = currentColor;
        }
    }
    
    void StartSkipSequence()
    {
        if (isSkipping) return;
        
        isSkipping = true;
        
        if (breathingTween != null)
        {
            breathingTween.Kill();
        }
        
        if (skipFadeTween != null)
        {
            skipFadeTween.Kill();
        }
        
        if (colorTween != null)
        {
            colorTween.Kill();
        }
        
        if (cameraToShake != null)
        {
            cameraToShake.DOShakePosition(shakeDuration, shakeIntensity);
        }
        
        Sequence skipSequence = DOTween.Sequence();
        
        if (skipText != null)
        {
            skipSequence.Append(skipText.DOFade(0f, 0.5f));
        }
        
        if (fadePanel != null)
        {
            skipSequence.Append(fadePanel.DOFade(1f, 1f));
        }
        
        skipSequence.AppendInterval(0.5f);
        skipSequence.OnComplete(LoadNextScene);
    }
    
    public void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(currentSceneIndex + 1);
        }
    }
    
    void OnDestroy()
    {
        breathingTween?.Kill();
        skipFadeTween?.Kill();
        colorTween?.Kill();
        
        if (cameraToShake != null)
        {
            cameraToShake.transform.position = originalCameraPosition;
        }
    }
}