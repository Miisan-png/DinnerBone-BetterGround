using UnityEngine;
using TMPro;

public class Debug_ShowFps : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI fpsText;
    
    [Header("FPS Settings")]
    [SerializeField] private float updateInterval = 0.5f;
    [SerializeField] private bool showAverage = true;
    [SerializeField] private bool showMinMax = false;
    
    [Header("Display Colors")]
    [SerializeField] private Color goodFpsColor = Color.green;
    [SerializeField] private Color okayFpsColor = Color.yellow;
    [SerializeField] private Color badFpsColor = Color.red;
    [SerializeField] private float goodFpsThreshold = 50f;
    [SerializeField] private float okayFpsThreshold = 30f;
    
    private float deltaTime = 0f;
    private float updateTimer = 0f;
    private float minFps = float.MaxValue;
    private float maxFps = 0f;
    private int frameCount = 0;
    private float fpsSum = 0f;
    
    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float currentFps = 1f / Time.unscaledDeltaTime;
        
        fpsSum += currentFps;
        frameCount++;
        
        if (currentFps < minFps) minFps = currentFps;
        if (currentFps > maxFps) maxFps = currentFps;
        
        updateTimer += Time.unscaledDeltaTime;
        
        if (updateTimer >= updateInterval)
        {
            UpdateFpsDisplay();
            updateTimer = 0f;
            
            if (showAverage)
            {
                fpsSum = 0f;
                frameCount = 0;
            }
            
            if (showMinMax)
            {
                minFps = float.MaxValue;
                maxFps = 0f;
            }
        }
    }
    
    void UpdateFpsDisplay()
    {
        if (fpsText == null) return;
        
        float displayFps = showAverage && frameCount > 0 ? fpsSum / frameCount : 1f / deltaTime;
        
        string fpsString = $"FPS: {displayFps:F0}";
        
        if (showMinMax)
        {
            fpsString += $"\nMin: {minFps:F0} | Max: {maxFps:F0}";
        }
        
        if (showAverage && frameCount > 0)
        {
            fpsString += $" (Avg)";
        }
        
        fpsText.text = fpsString;
        
        if (displayFps >= goodFpsThreshold)
        {
            fpsText.color = goodFpsColor;
        }
        else if (displayFps >= okayFpsThreshold)
        {
            fpsText.color = okayFpsColor;
        }
        else
        {
            fpsText.color = badFpsColor;
        }
    }
}