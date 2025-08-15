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

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip uiSfx;
    [Range(0f,1f)] public float sfxVolume = 1f;

    void Start()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
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

    void PlaySfx()
    {
        if (audioSource != null && uiSfx != null) audioSource.PlayOneShot(uiSfx, sfxVolume);
    }

    public void ShowMainMenu()
    {
        PlaySfx();
        if (mainMenuObject != null) mainMenuObject.SetActive(true);
        if (newGameMenuObject != null) newGameMenuObject.SetActive(false);
        if (settingsMenuObject != null) settingsMenuObject.SetActive(false);
        if (creditsMenuObject != null) creditsMenuObject.SetActive(false);
    }

    public void ShowNewGameMenu()
    {
        PlaySfx();
        if (mainMenuObject != null) mainMenuObject.SetActive(false);
        if (newGameMenuObject != null) newGameMenuObject.SetActive(true);
        if (settingsMenuObject != null) settingsMenuObject.SetActive(false);
        if (creditsMenuObject != null) creditsMenuObject.SetActive(false);
    }

    public void ShowSettingsMenu()
    {
        PlaySfx();
        if (mainMenuObject != null) mainMenuObject.SetActive(false);
        if (newGameMenuObject != null) newGameMenuObject.SetActive(false);
        if (settingsMenuObject != null) settingsMenuObject.SetActive(true);
        if (creditsMenuObject != null) creditsMenuObject.SetActive(false);
    }

    public void ShowCreditsMenu()
    {
        PlaySfx();
        if (mainMenuObject != null) mainMenuObject.SetActive(false);
        if (newGameMenuObject != null) newGameMenuObject.SetActive(false);
        if (settingsMenuObject != null) settingsMenuObject.SetActive(false);
        if (creditsMenuObject != null) creditsMenuObject.SetActive(true);
    }

    public void ChangeScene()
    {
        PlaySfx();
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
        PlaySfx();
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

    public void PlayClickSfx()
    {
        PlaySfx();
    }
}
