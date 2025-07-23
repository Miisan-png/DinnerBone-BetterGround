using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Input_Icon_Database", menuName = "UI/Input Icon Database")]
public class Input_Icon_Database : ScriptableObject
{
    [System.Serializable]
    public class InputIconSet
    {
        [Header("Icons")]
        public Sprite xbox_icon;
        public Sprite playstation_icon;
        public Sprite keyboard_luthe_icon;
        public Sprite keyboard_cherie_icon;
        public Sprite generic_controller_icon;
        
        [Header("Input Method Text")]
        public string xbox_text = "Hold X";
        public string playstation_text = "Hold X";
        public string keyboard_luthe_text = "Hold E";
        public string keyboard_cherie_text = "Hold R";
        public string generic_controller_text = "Hold A";
    }
    
    [System.Serializable]
    public class InteractionTypeData
    {
        [Header("Interaction Info")]
        public string interaction_id;
        public string display_name;
        
        [Header("Normal Interaction")]
        public string normal_action_text = "interact with";
        public Color normal_text_color = Color.white;
        
        [Header("Restricted Interaction")]
        public string restricted_action_text = "Cannot access";
        public Color restricted_text_color = Color.red;
        
        [Header("Custom Templates (Optional)")]
        [TextArea(2, 4)]
        public string custom_normal_template = ""; // e.g., "{input} to {action}"
        [TextArea(2, 4)]
        public string custom_restricted_template = ""; // e.g., "{action} - Access Denied"
    }
    
    [Header("Input Icons & Text")]
    public InputIconSet interact_icons;
    public InputIconSet restricted_icons;
    
    [Header("Interaction Types")]
    public List<InteractionTypeData> interaction_types = new List<InteractionTypeData>();
    
    [Header("Default Settings")]
    public string default_template = "{input} to {action}";
    public string default_restricted_template = "{restricted_text}";
    public Color default_normal_color = Color.white;
    public Color default_restricted_color = Color.red;
    
    // Dictionary for fast lookup (built at runtime)
    private Dictionary<string, InteractionTypeData> interaction_lookup;
    
    void OnEnable()
    {
        BuildLookupDictionary();
    }
    
    private void BuildLookupDictionary()
    {
        interaction_lookup = new Dictionary<string, InteractionTypeData>();
        foreach (var interaction in interaction_types)
        {
            if (!string.IsNullOrEmpty(interaction.interaction_id))
            {
                interaction_lookup[interaction.interaction_id] = interaction;
            }
        }
    }
    
    public Sprite GetIcon(InputDeviceType device_type, Player_Type player_type, bool is_restricted = false)
    {
        InputIconSet icon_set = is_restricted ? restricted_icons : interact_icons;
        
        switch (device_type)
        {
            case InputDeviceType.Xbox:
                return icon_set.xbox_icon;
            case InputDeviceType.PlayStation:
                return icon_set.playstation_icon;
            case InputDeviceType.Keyboard:
                return player_type == Player_Type.Luthe ? icon_set.keyboard_luthe_icon : icon_set.keyboard_cherie_icon;
            case InputDeviceType.Generic:
                return icon_set.generic_controller_icon;
            default:
                return icon_set.generic_controller_icon;
        }
    }
    
    public string GetInputMethodText(InputDeviceType device_type, Player_Type player_type, bool is_restricted = false)
    {
        InputIconSet icon_set = is_restricted ? restricted_icons : interact_icons;
        
        switch (device_type)
        {
            case InputDeviceType.Xbox:
                return icon_set.xbox_text;
            case InputDeviceType.PlayStation:
                return icon_set.playstation_text;
            case InputDeviceType.Keyboard:
                return player_type == Player_Type.Luthe ? icon_set.keyboard_luthe_text : icon_set.keyboard_cherie_text;
            case InputDeviceType.Generic:
                return icon_set.generic_controller_text;
            default:
                return icon_set.generic_controller_text;
        }
    }
    
    public InteractionData GetInteractionData(string interaction_id, InputDeviceType device_type, Player_Type player_type, bool is_restricted = false)
    {
        if (interaction_lookup == null) BuildLookupDictionary();
        
        // Get interaction type data
        InteractionTypeData type_data = null;
        if (!string.IsNullOrEmpty(interaction_id) && interaction_lookup.ContainsKey(interaction_id))
        {
            type_data = interaction_lookup[interaction_id];
        }
        
        // Get input method text
        string input_text = GetInputMethodText(device_type, player_type, is_restricted);
        
        // Build final text and get color
        string final_text;
        Color text_color;
        
        if (is_restricted)
        {
            if (type_data != null)
            {
                // Use custom template if available, otherwise default
                string template = !string.IsNullOrEmpty(type_data.custom_restricted_template) 
                    ? type_data.custom_restricted_template 
                    : default_restricted_template;
                    
                final_text = template
                    .Replace("{input}", input_text)
                    .Replace("{action}", type_data.normal_action_text)
                    .Replace("{restricted_text}", type_data.restricted_action_text);
                    
                text_color = type_data.restricted_text_color;
            }
            else
            {
                final_text = "Cannot interact";
                text_color = default_restricted_color;
            }
        }
        else
        {
            if (type_data != null)
            {
                // Use custom template if available, otherwise default
                string template = !string.IsNullOrEmpty(type_data.custom_normal_template) 
                    ? type_data.custom_normal_template 
                    : default_template;
                    
                final_text = template
                    .Replace("{input}", input_text)
                    .Replace("{action}", type_data.normal_action_text);
                    
                text_color = type_data.normal_text_color;
            }
            else
            {
                final_text = $"{input_text} to interact";
                text_color = default_normal_color;
            }
        }
        
        return new InteractionData
        {
            text = final_text,
            color = text_color,
            icon = GetIcon(device_type, player_type, is_restricted)
        };
    }
    
    // Helper method for backward compatibility
    public string GetInteractionText(InputDeviceType device_type, Player_Type player_type, bool is_restricted = false, string interaction_id = "")
    {
        return GetInteractionData(interaction_id, device_type, player_type, is_restricted).text;
    }
    
    // Validation method for editor
    public bool ValidateInteractionID(string interaction_id)
    {
        if (interaction_lookup == null) BuildLookupDictionary();
        return !string.IsNullOrEmpty(interaction_id) && interaction_lookup.ContainsKey(interaction_id);
    }
    
    public List<string> GetAllInteractionIDs()
    {
        List<string> ids = new List<string>();
        foreach (var interaction in interaction_types)
        {
            if (!string.IsNullOrEmpty(interaction.interaction_id))
            {
                ids.Add(interaction.interaction_id);
            }
        }
        return ids;
    }
}

[System.Serializable]
public struct InteractionData
{
    public string text;
    public Color color;
    public Sprite icon;
}

public enum InputDeviceType
{
    Xbox,
    PlayStation,
    Keyboard,
    Generic
}