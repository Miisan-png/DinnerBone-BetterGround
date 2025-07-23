using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Vent_Fade : MonoBehaviour
{
    [SerializeField] private Image fade_image;
    [SerializeField] private float fade_duration = 0.5f;
    
    private static Vent_Fade instance;
    public static Vent_Fade Instance => instance;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            if (fade_image == null)
            {
                CreateFadeImage();
            }
            
            fade_image.color = new Color(0, 0, 0, 0);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void CreateFadeImage()
    {
        GameObject canvas = new GameObject("Fade_Canvas");
        Canvas canvasComp = canvas.AddComponent<Canvas>();
        canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasComp.sortingOrder = 1000;
        
        canvas.AddComponent<CanvasScaler>();
        canvas.AddComponent<GraphicRaycaster>();
        
        GameObject fadeObj = new GameObject("Fade_Image");
        fadeObj.transform.SetParent(canvas.transform);
        
        fade_image = fadeObj.AddComponent<Image>();
        fade_image.color = new Color(0, 0, 0, 0);
        
        RectTransform rectTransform = fade_image.rectTransform;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        
        DontDestroyOnLoad(canvas);
    }
    
    public void Fade_In_Out(System.Action on_fade_complete = null)
    {
        StartCoroutine(FadeInOutCoroutine(on_fade_complete));
    }
    
    private IEnumerator FadeInOutCoroutine(System.Action on_fade_complete = null)
    {
        yield return StartCoroutine(FadeToBlack());
        
        on_fade_complete?.Invoke();
        
        yield return StartCoroutine(FadeFromBlack());
    }
    
    private IEnumerator FadeToBlack()
    {
        float elapsed = 0f;
        Color start_color = fade_image.color;
        Color target_color = new Color(0, 0, 0, 1);
        
        while (elapsed < fade_duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fade_duration;
            fade_image.color = Color.Lerp(start_color, target_color, t);
            yield return null;
        }
        
        fade_image.color = target_color;
    }
    
    private IEnumerator FadeFromBlack()
    {
        float elapsed = 0f;
        Color start_color = fade_image.color;
        Color target_color = new Color(0, 0, 0, 0);
        
        while (elapsed < fade_duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fade_duration;
            fade_image.color = Color.Lerp(start_color, target_color, t);
            yield return null;
        }
        
        fade_image.color = target_color;
    }
}