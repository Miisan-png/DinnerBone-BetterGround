using UnityEngine;
using UnityEngine.InputSystem;

public class Input_Detector : MonoBehaviour
{
    private static Input_Detector instance;
    public static Input_Detector Instance => instance;
    
    [SerializeField] private InputDeviceType current_device_type = InputDeviceType.Keyboard;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        DetectInputDevice();
        InputSystem.onDeviceChange += OnDeviceChange;
    }
    
    void Update()
    {
        CheckForDeviceChanges();
    }
    
    void OnDestroy()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }
    
    private void CheckForDeviceChanges()
    {
        var gamepads = Gamepad.all;
        InputDeviceType previous_type = current_device_type;
        
        if (gamepads.Count > 0)
        {
            var gamepad = gamepads[0];
            string deviceName = gamepad.displayName.ToLower();
            
            if (deviceName.Contains("xbox") || deviceName.Contains("microsoft"))
            {
                current_device_type = InputDeviceType.Xbox;
            }
            else if (deviceName.Contains("playstation") || deviceName.Contains("dualshock") || deviceName.Contains("dualsense"))
            {
                current_device_type = InputDeviceType.PlayStation;
            }
            else
            {
                current_device_type = InputDeviceType.Generic;
            }
        }
        else if (Input.anyKeyDown && !Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1) && !Input.GetMouseButtonDown(2))
        {
            current_device_type = InputDeviceType.Keyboard;
        }
        
        if (previous_type != current_device_type)
        {
            Debug.Log($"Input device changed to: {current_device_type}");
        }
    }
    
    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (change == InputDeviceChange.Added || change == InputDeviceChange.Reconnected)
        {
            DetectInputDevice();
        }
    }
    
    private void DetectInputDevice()
    {
        var gamepads = Gamepad.all;
        
        if (gamepads.Count > 0)
        {
            var gamepad = gamepads[0];
            string deviceName = gamepad.displayName.ToLower();
            
            if (deviceName.Contains("xbox") || deviceName.Contains("microsoft"))
            {
                current_device_type = InputDeviceType.Xbox;
            }
            else if (deviceName.Contains("playstation") || deviceName.Contains("dualshock") || deviceName.Contains("dualsense"))
            {
                current_device_type = InputDeviceType.PlayStation;
            }
            else
            {
                current_device_type = InputDeviceType.Generic;
            }
        }
        else
        {
            current_device_type = InputDeviceType.Keyboard;
        }
    }
    
    public InputDeviceType GetCurrentDeviceType()
    {
        return current_device_type;
    }
    
    public InputDeviceType GetPlayerDeviceType(Player_Type player_type)
    {
        var gamepads = Gamepad.all;
        
        if (player_type == Player_Type.Luthe)
        {
            if (gamepads.Count > 0 && IsControllerActive(gamepads[0]))
            {
                return GetControllerType(gamepads[0]);
            }
            else
            {
                return InputDeviceType.Keyboard;
            }
        }
        else
        {
            if (gamepads.Count > 1 && IsControllerActive(gamepads[1]))
            {
                return GetControllerType(gamepads[1]);
            }
            else
            {
                return InputDeviceType.Keyboard;
            }
        }
    }
    
    private bool IsControllerActive(Gamepad gamepad)
    {
        return gamepad != null && gamepad.wasUpdatedThisFrame;
    }
    
    private InputDeviceType GetControllerType(Gamepad gamepad)
    {
        string deviceName = gamepad.displayName.ToLower();
        
        if (deviceName.Contains("xbox") || deviceName.Contains("microsoft"))
        {
            return InputDeviceType.Xbox;
        }
        else if (deviceName.Contains("playstation") || deviceName.Contains("dualshock") || deviceName.Contains("dualsense"))
        {
            return InputDeviceType.PlayStation;
        }
        else
        {
            return InputDeviceType.Generic;
        }
    }
}