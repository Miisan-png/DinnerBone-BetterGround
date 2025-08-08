using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class Menu_Logic : MonoBehaviour
{
    [Header("Menu GameObjects")]
    public GameObject mainMenuObject;
    public GameObject newGameMenuObject;
    public GameObject settingsMenuObject;
    public GameObject creditsMenuObject;
    
    [Header("Scene Management")]
    public string sceneToLoad = "GameScene";
    public Canvas fadeCanvas;
    public CanvasGroup fadeCanvasGroup;
    
    [Header("Animation Settings")]
    public float fadeDuration = 0.5f;
    
    void Start()
    {
        InitializeFade();
        ShowMainMenu();
    }
    
    void InitializeFade()
    {
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.interactable = false;
            fadeCanvasGroup.blocksRaycasts = false;
        }
    }
    
    public void ShowMainMenu()
    {
        if (mainMenuObject != null)
            mainMenuObject.SetActive(true);
            
        if (newGameMenuObject != null)
            newGameMenuObject.SetActive(false);
            
        if (settingsMenuObject != null)
            settingsMenuObject.SetActive(false);
            
        if (creditsMenuObject != null)
            creditsMenuObject.SetActive(false);
    }
    
    public void ShowNewGameMenu()
    {
        if (mainMenuObject != null)
            mainMenuObject.SetActive(false);
            
        if (newGameMenuObject != null)
            newGameMenuObject.SetActive(true);
            
        if (settingsMenuObject != null)
            settingsMenuObject.SetActive(false);
            
        if (creditsMenuObject != null)
            creditsMenuObject.SetActive(false);
    }
    
    public void ShowSettingsMenu()
    {
        if (mainMenuObject != null)
            mainMenuObject.SetActive(false);
            
        if (newGameMenuObject != null)
            newGameMenuObject.SetActive(false);
            
        if (settingsMenuObject != null)
            settingsMenuObject.SetActive(true);
            
        if (creditsMenuObject != null)
            creditsMenuObject.SetActive(false);
    }
    
    public void ShowCreditsMenu()
    {
        if (mainMenuObject != null)
            mainMenuObject.SetActive(false);
            
        if (newGameMenuObject != null)
            newGameMenuObject.SetActive(false);
            
        if (settingsMenuObject != null)
            settingsMenuObject.SetActive(false);
            
        if (creditsMenuObject != null)
            creditsMenuObject.SetActive(true);
    }
    
    public void ChangeScene()
    {
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.DOFade(1f, fadeDuration).OnComplete(() =>
            {
                SceneManager.LoadScene(sceneToLoad);
            });
        }
        else
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
    
    public void ChangeSceneWithName(string sceneName)
    {
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.DOFade(1f, fadeDuration).OnComplete(() =>
            {
                SceneManager.LoadScene(sceneName);
            });
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}