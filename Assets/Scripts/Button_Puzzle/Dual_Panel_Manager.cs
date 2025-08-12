using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Dual_Panel_Manager : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private Panel_Interaction panel_1;
    [SerializeField] private Panel_Interaction panel_2;
    
    [Header("Lighting")]
    [SerializeField] private LightFlickerManager[] light_managers;
    
    [Header("Success Effects")]
    [SerializeField] private GameObject[] success_effects;
    [SerializeField] private AudioSource success_audio;
    [SerializeField] private float celebration_delay = 1f;

    [Header("Button Activation")]
    [SerializeField] private GameObject[] activation_buttons;
    [SerializeField] private Image[] button_icons;
    [SerializeField] private Color activated_color = Color.white;
    [SerializeField] private Color deactivated_color = Color.gray;
    [SerializeField] private float icon_fade_duration = 0.5f;
    
    private static Dual_Panel_Manager instance;
    public static Dual_Panel_Manager Instance => instance;
    
    private bool panel_1_complete = false;
    private bool panel_2_complete = false;
    private bool overall_puzzle_complete = false;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        if (panel_1 != null)
        {
            panel_1.SetDualPanelManager(this);
            panel_1.SetPanelID(1);
        }
        
        if (panel_2 != null)
        {
            panel_2.SetDualPanelManager(this);
            panel_2.SetPanelID(2);
        }
        
        // Initialize buttons as deactivated
        InitializeButtons();
    }
    
    private void InitializeButtons()
    {
        // Deactivate all button objects
        if (activation_buttons != null)
        {
            foreach (var button in activation_buttons)
            {
                if (button != null)
                {
                    button.SetActive(false);
                }
            }
        }
        
        // Set all icons to deactivated state
        if (button_icons != null)
        {
            foreach (var icon in button_icons)
            {
                if (icon != null)
                {
                    icon.color = deactivated_color;
                    icon.gameObject.SetActive(false);
                }
            }
        }
    }
    
    public void NotifyPanelComplete(int panel_id, bool is_complete)
    {
        if (panel_id == 1)
        {
            panel_1_complete = is_complete;
        }
        else if (panel_id == 2)
        {
            panel_2_complete = is_complete;
        }
        
        Debug.Log($"Panel {panel_id} complete: {is_complete}. Panel 1: {panel_1_complete}, Panel 2: {panel_2_complete}");
        
        CheckOverallCompletion();
    }
    
    private void CheckOverallCompletion()
    {
        bool both_complete = panel_1_complete && panel_2_complete;
        
        if (both_complete && !overall_puzzle_complete)
        {
            CompleteOverallPuzzle();
        }
        else if (!both_complete && overall_puzzle_complete)
        {
            ResetOverallPuzzle();
        }
    }
    
    private void CompleteOverallPuzzle()
{
    overall_puzzle_complete = true;

    if (Objective_Global.Instance != null)
    {
        Objective_Global.Instance.SetObjective("Emergency Power Restored");
    }

    if (panel_1 != null) panel_1.TriggerPanelCompletion();
    if (panel_2 != null) panel_2.TriggerPanelCompletion();

    DOTween.Sequence()
        .AppendInterval(celebration_delay)
        .AppendCallback(() => {
            ActivateLights();
            PlaySuccessEffects();
            ActivateButtons();
        })
        .AppendInterval(2f)
        .AppendCallback(() => {
            if (panel_1 != null) panel_1.ExitPuzzleFromManager();
            if (panel_2 != null) panel_2.ExitPuzzleFromManager();
        });
}

    
    private void ResetOverallPuzzle()
    {
        overall_puzzle_complete = false;
        Debug.Log("Puzzle reset - not both panels complete");
        
        DeactivateLights();
        DeactivateButtons();
    }
    
    private void ActivateButtons()
    {
        // Activate button objects
        if (activation_buttons != null)
        {
            foreach (var button in activation_buttons)
            {
                if (button != null)
                {
                    button.SetActive(true);
                }
            }
        }
        
        // Show and animate icons
        if (button_icons != null)
        {
            for (int i = 0; i < button_icons.Length; i++)
            {
                var icon = button_icons[i];
                if (icon != null)
                {
                    icon.gameObject.SetActive(true);
                    
                    // Animate icon appearance with a slight delay for each
                    DOTween.Sequence()
                        .AppendInterval(i * 0.1f) // Stagger the animations
                        .AppendCallback(() => {
                            icon.color = deactivated_color;
                            icon.transform.localScale = Vector3.zero;
                        })
                        .Append(icon.transform.DOScale(1f, icon_fade_duration).SetEase(Ease.OutBack))
                        .Join(icon.DOColor(activated_color, icon_fade_duration));
                }
            }
        }
    }
    
    private void DeactivateButtons()
    {
        // Deactivate button objects
        if (activation_buttons != null)
        {
            foreach (var button in activation_buttons)
            {
                if (button != null)
                {
                    button.SetActive(false);
                }
            }
        }
        
        // Hide and reset icons
        if (button_icons != null)
        {
            foreach (var icon in button_icons)
            {
                if (icon != null)
                {
                    icon.DOKill(); // Stop any ongoing animations
                    icon.color = deactivated_color;
                    icon.transform.localScale = Vector3.one;
                    icon.gameObject.SetActive(false);
                }
            }
        }
    }
    
    private void ActivateLights()
    {
        if (light_managers != null)
        {
            foreach (var light_manager in light_managers)
            {
                if (light_manager != null)
                {
                    light_manager.ToggleFullBrightness(true);
                }
            }
        }
    }
    
    private void DeactivateLights()
    {
        if (light_managers != null)
        {
            foreach (var light_manager in light_managers)
            {
                if (light_manager != null)
                {
                    light_manager.ToggleFullBrightness(false);
                }
            }
        }
    }
    
    private void PlaySuccessEffects()
    {
        if (success_effects != null)
        {
            foreach (var effect in success_effects)
            {
                if (effect != null)
                {
                    effect.SetActive(true);
                }
            }
        }
        
        if (success_audio != null)
        {
            success_audio.Play();
        }
    }
    
    public bool IsPanelComplete(int panel_id)
    {
        if (panel_id == 1) return panel_1_complete;
        if (panel_id == 2) return panel_2_complete;
        return false;
    }
    
    public bool IsOverallPuzzleComplete()
    {
        return overall_puzzle_complete;
    }
    
    public void ForceReset()
    {
        panel_1_complete = false;
        panel_2_complete = false;
        overall_puzzle_complete = false;
        
        if (panel_1 != null) panel_1.ForceReset();
        if (panel_2 != null) panel_2.ForceReset();
        
        DeactivateLights();
        DeactivateButtons();
    }
    
    // Public methods for external button interaction
    public bool AreButtonsActivated()
    {
        return overall_puzzle_complete;
    }
    
    public void OnButtonPressed(int button_index)
    {
        if (!overall_puzzle_complete)
        {
            Debug.Log($"Button {button_index} pressed but puzzle not complete!");
            return;
        }
        
        Debug.Log($"Button {button_index} successfully activated!");
        // Add your button functionality here
    }
}