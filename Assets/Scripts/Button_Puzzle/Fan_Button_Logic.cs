using UnityEngine;
using DG.Tweening;

public class Fan_Button_Logic : MonoBehaviour, I_Interactable, IInteractionIdentifier
{
    [Header("Button Settings")]
    [SerializeField] private string interaction_id = "fan_button";
    [SerializeField] private int button_id = 0; // 0, 1, or 2
    [SerializeField] private GameObject button_icon;
    [SerializeField] private Color button_glow_color = Color.cyan;
    [SerializeField] private float click_scale_amount = 1.2f;
    [SerializeField] private float click_animation_duration = 0.2f;
    
    [Header("Fan Connection")]
    [SerializeField] private Fan_Rotate_Logic connected_fan;
    
    [Header("Audio (Optional)")]
    [SerializeField] private AudioSource audio_source;
    [SerializeField] private AudioClip button_click_sound;
    
    private Material icon_material;
    private Color original_glow_color;
    private Vector3 original_icon_scale;
    private bool is_animating = false;
    
    void Start()
    {
        SetupButtonIcon();
        
        if (audio_source == null)
        {
            audio_source = GetComponent<AudioSource>();
        }
    }
    
    private void SetupButtonIcon()
    {
        if (button_icon != null)
        {
            // Get the renderer and create a material instance
            Renderer icon_renderer = button_icon.GetComponent<Renderer>();
            if (icon_renderer != null)
            {
                icon_material = new Material(icon_renderer.material);
                icon_renderer.material = icon_material;
                
                // Store original glow color
                if (icon_material.HasProperty("_GlowColor"))
                {
                    original_glow_color = icon_material.GetColor("_GlowColor");
                }
                else if (icon_material.HasProperty("_Color"))
                {
                    original_glow_color = icon_material.GetColor("_Color");
                }
                
                // Store original scale
                original_icon_scale = button_icon.transform.localScale;
            }
            else
            {
                Debug.LogWarning($"Fan_Button_Logic: Button icon {button_icon.name} doesn't have a Renderer component!");
            }
        }
        else
        {
            Debug.LogWarning("Fan_Button_Logic: Button icon not assigned!");
        }
    }
    
    public bool Can_Interact(Player_Type player_type)
    {
        // Anyone can interact with the fan button
        return !is_animating;
    }
    
    public void Start_Interaction(Player_Controller player)
    {
        if (is_animating) return;
        
        PlayClickAnimation();
        PlayClickSound();
        
        // Check if this button matches the fan's current index
        if (connected_fan != null)
        {
            if (connected_fan.GetCurrentIconId() == button_id)
            {
                // Correct button pressed! Slow down the fan
                connected_fan.SlowDownFan();
                Debug.Log($"Correct! Button {button_id} pressed when fan at index {connected_fan.GetCurrentIconId()}");
            }
            else
            {
                Debug.Log($"Wrong button! Button {button_id} pressed but fan at index {connected_fan.GetCurrentIconId()}");
            }
        }
        else
        {
            Debug.Log($"Fan button {button_id} clicked by {player.Get_Player_Type()}!");
        }
    }
    
    public void End_Interaction(Player_Controller player)
    {
        // Not needed for this button type
    }
    
    public string Get_Interaction_Text()
    {
        return "Press fan button";
    }
    
    public Vector3 Get_Interaction_Position()
    {
        return transform.position;
    }
    
    public string GetInteractionID()
    {
        return interaction_id;
    }
    
    private void PlayClickAnimation()
    {
        if (button_icon == null || icon_material == null) return;
        
        is_animating = true;
        
        // Create animation sequence
        Sequence click_sequence = DOTween.Sequence();
        
        // Change glow color immediately
        ChangeGlowColor(button_glow_color);
        
        // Scale animation: scale up, then back down
        click_sequence.Append(
            button_icon.transform.DOScale(original_icon_scale * click_scale_amount, click_animation_duration * 0.5f)
                .SetEase(Ease.OutQuad)
        );
        
        click_sequence.Append(
            button_icon.transform.DOScale(original_icon_scale, click_animation_duration * 0.5f)
                .SetEase(Ease.InQuad)
        );
        
        // Revert color and reset animation flag when complete
        click_sequence.OnComplete(() => {
            ChangeGlowColor(original_glow_color);
            is_animating = false;
        });
    }
    
    private void ChangeGlowColor(Color target_color)
    {
        if (icon_material == null) return;
        
        // Try different property names that might exist
        if (icon_material.HasProperty("_GlowColor"))
        {
            icon_material.SetColor("_GlowColor", target_color);
        }
        else if (icon_material.HasProperty("_Color"))
        {
            icon_material.SetColor("_Color", target_color);
        }
        else if (icon_material.HasProperty("_BaseColor"))
        {
            icon_material.SetColor("_BaseColor", target_color);
        }
    }
    
    private void PlayClickSound()
    {
        if (audio_source != null && button_click_sound != null)
        {
            audio_source.PlayOneShot(button_click_sound);
        }
    }
    
    // Public getters
    public int GetButtonId()
    {
        return button_id;
    }
    
    public void SetConnectedFan(Fan_Rotate_Logic fan)
    {
        connected_fan = fan;
    }
    
    // Public method to manually trigger the button (for testing or external triggers)
    public void TriggerButton()
    {
        if (!is_animating)
        {
            PlayClickAnimation();
            PlayClickSound();
        }
    }
    
    void OnDrawGizmos()
    {
        // Draw interaction range indicator
        Gizmos.color = button_glow_color;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
        
        // Draw line to button icon if assigned
        if (button_icon != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, button_icon.transform.position);
            Gizmos.DrawWireSphere(button_icon.transform.position, 0.3f);
        }
    }
}