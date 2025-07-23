using UnityEngine;
using UnityEngine.InputSystem;

public class Input_Detector : MonoBehaviour
{
    private static Input_Detector instance;
    public static Input_Detector Instance => instance;
    
    [SerializeField] private InputDeviceType current_device_type = InputDeviceType.Keyboard;
    [SerializeField] private bool debug_input_changes = false;
    
    private InputDeviceType player_1_device = InputDeviceType.Keyboard;
    private InputDeviceType player_2_device = InputDeviceType.Keyboard;
    
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
        CheckForPlayerSpecificInput();
    }
    
    void OnDestroy()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }
    
    private void CheckForDeviceChanges()
    {
        var gamepads = Gamepad.all;
        InputDeviceType previous_type = current_device_type;
        
        bool gamepad_active = false;
        if (gamepads.Count > 0)
        {
            foreach (var gamepad in gamepads)
            {
                if (HasAnyInput(gamepad))
                {
                    gamepad_active = true;
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
                    break;
                }
            }
        }
        
        if (!gamepad_active && (Input.anyKeyDown && !Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1) && !Input.GetMouseButtonDown(2)))
        {
            current_device_type = InputDeviceType.Keyboard;
        }
        
        if (previous_type != current_device_type && debug_input_changes)
        {
            Debug.Log($"Global input device changed to: {current_device_type}");
        }
    }
    
    private void CheckForPlayerSpecificInput()
    {
        var gamepads = Gamepad.all;
        
        // Check Player 1 (Luthe) - First controller or keyboard
        InputDeviceType previous_p1 = player_1_device;
        if (gamepads.Count > 0 && HasAnyInput(gamepads[0]))
        {
            player_1_device = GetControllerType(gamepads[0]);
        }
        else if (HasKeyboardInput())
        {
            player_1_device = InputDeviceType.Keyboard;
        }
        
        // Check Player 2 (Cherie) - Second controller or keyboard
        InputDeviceType previous_p2 = player_2_device;
        if (gamepads.Count > 1 && HasAnyInput(gamepads[1]))
        {
            player_2_device = GetControllerType(gamepads[1]);
        }
        else if (HasKeyboardInput())
        {
            player_2_device = InputDeviceType.Keyboard;
        }
        
        if (debug_input_changes)
        {
            if (previous_p1 != player_1_device)
            {
                Debug.Log($"Player 1 (Luthe) device changed to: {player_1_device}");
            }
            if (previous_p2 != player_2_device)
            {
                Debug.Log($"Player 2 (Cherie) device changed to: {player_2_device}");
            }
        }
    }
    
    private bool HasAnyInput(Gamepad gamepad)
    {
        if (gamepad == null) return false;
        
        // Check if any button or stick has been pressed/moved recently // COOKED LOLx
        return gamepad.leftStick.ReadValue().magnitude > 0.1f ||
               gamepad.rightStick.ReadValue().magnitude > 0.1f ||
               gamepad.buttonSouth.wasPressedThisFrame ||
               gamepad.buttonEast.wasPressedThisFrame ||
               gamepad.buttonWest.wasPressedThisFrame ||
               gamepad.buttonNorth.wasPressedThisFrame ||
               gamepad.leftShoulder.wasPressedThisFrame ||
               gamepad.rightShoulder.wasPressedThisFrame ||
               gamepad.leftTrigger.ReadValue() > 0.1f ||
               gamepad.rightTrigger.ReadValue() > 0.1f ||
               gamepad.dpad.ReadValue().magnitude > 0.1f ||
               gamepad.startButton.wasPressedThisFrame ||
               gamepad.selectButton.wasPressedThisFrame;
    }
    
    private bool HasKeyboardInput()
    {
        return Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || 
               Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D) ||
               Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.R) ||
               Input.GetKeyDown(KeyCode.Space) || Input.anyKeyDown;
    }
    
    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (change == InputDeviceChange.Added || change == InputDeviceChange.Reconnected)
        {
            DetectInputDevice();
            if (debug_input_changes)
            {
                Debug.Log($"Device {change}: {device.displayName}");
            }
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
        
        player_1_device = current_device_type;
        player_2_device = gamepads.Count > 1 ? GetControllerType(gamepads[1]) : InputDeviceType.Keyboard;
    }
    
    public InputDeviceType GetCurrentDeviceType()
    {
        return current_device_type;
    }
    
    public InputDeviceType GetPlayerDeviceType(Player_Type player_type)
    {
        return player_type == Player_Type.Luthe ? player_1_device : player_2_device;
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