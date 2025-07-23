using UnityEngine;

[CreateAssetMenu(fileName = "Input_Icon_Database", menuName = "UI/Input Icon Database")]
public class Input_Icon_Database : ScriptableObject
{
    [System.Serializable]
    public class InputIconSet
    {
        public Sprite xbox_icon;
        public Sprite playstation_icon;
        public Sprite keyboard_luthe_icon;
        public Sprite keyboard_cherie_icon;
        public Sprite generic_controller_icon;
    }
    
    [Header("Interact Icons")]
    public InputIconSet interact_icons;
    
    [Header("Restricted Access Icons")]
    public InputIconSet restricted_icons;
    
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
}

public enum InputDeviceType
{
    Xbox,
    PlayStation,
    Keyboard,
    Generic
}