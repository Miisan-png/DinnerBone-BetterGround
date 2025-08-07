using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class Intro_Splash_Logic : MonoBehaviour
{
    [Header("Scene Settings")]
    public string nextSceneName = "MainMenu";
    public float waitTime = 3f;
    
    [Header("Fade Settings")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeOutDuration = 1f;

    void Start()
    {
        fadeCanvasGroup.alpha = 1f;
        
        fadeCanvasGroup.DOFade(0f, fadeOutDuration).OnComplete(() =>
        {
            DOVirtual.DelayedCall(waitTime, () =>
            {
                fadeCanvasGroup.DOFade(1f, fadeOutDuration).OnComplete(() =>
                {
                    SceneManager.LoadScene(nextSceneName);
                });
            });
        });
    }

    void OnDestroy()
    {
        fadeCanvasGroup.DOKill();
    }
}