using UnityEngine;
using UnityEngine.InputSystem;

public enum Player_Type { Luthe, Cherie }

public class Player_Controller : MonoBehaviour
{
    [SerializeField] private Player_Type player_type;
    [SerializeField] private InputSystem_Actions inputActions;
    [SerializeField] private bool debugInput = false;
    
    private Player_Movement movement_script;
    private bool is_in_push_mode = false;
    
    private Vector2 moveInput;
    private bool jumpPressed;
    private bool interactPressed;
    private bool interactHeld;
    private bool sprintHeld;
    private float rotationInput;

    private bool exitPressed;


    void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    void Start()
    {
        movement_script = GetComponent<Player_Movement>();
        SetupInputCallbacks();
    }

    void OnEnable()
    {
        inputActions?.PlayerInputActions.Enable();
    }

    void OnDisable()
    {
        inputActions?.PlayerInputActions.Disable();
    }

    void OnDestroy()
    {
        CleanupInputCallbacks();
        inputActions?.Dispose();
    }

    private void SetupInputCallbacks()
    {
        inputActions.PlayerInputActions.Move.performed += OnMove;
        inputActions.PlayerInputActions.Move.canceled += OnMove;
        inputActions.PlayerInputActions.Jump.performed += OnJump;
        inputActions.PlayerInputActions.Interact.performed += OnInteract;
        inputActions.PlayerInputActions.InteractHeld.performed += OnInteractHeld;
        inputActions.PlayerInputActions.InteractHeld.canceled += OnInteractHeld;
        inputActions.PlayerInputActions.Rotate.performed += OnRotate;
        inputActions.PlayerInputActions.Rotate.canceled += OnRotate;
        inputActions.PlayerInputActions.Sprint.performed += OnSprint;
        inputActions.PlayerInputActions.Sprint.canceled += OnSprint;
        inputActions.PlayerInputActions.Exit.performed += OnExit;

    }

    private void CleanupInputCallbacks()
    {
        if (inputActions != null)
        {
            inputActions.PlayerInputActions.Move.performed -= OnMove;
            inputActions.PlayerInputActions.Move.canceled -= OnMove;
            inputActions.PlayerInputActions.Jump.performed -= OnJump;
            inputActions.PlayerInputActions.Interact.performed -= OnInteract;
            inputActions.PlayerInputActions.InteractHeld.performed -= OnInteractHeld;
            inputActions.PlayerInputActions.InteractHeld.canceled -= OnInteractHeld;
            inputActions.PlayerInputActions.Rotate.performed -= OnRotate;
            inputActions.PlayerInputActions.Rotate.canceled -= OnRotate;
            inputActions.PlayerInputActions.Sprint.performed -= OnSprint;
            inputActions.PlayerInputActions.Sprint.canceled -= OnSprint;
            inputActions.PlayerInputActions.Exit.performed -= OnExit;

        }
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        if (IsCorrectDevice(context.control.device))
        {
            moveInput = context.ReadValue<Vector2>();
            if (debugInput && moveInput.magnitude > 0.1f) 
                Debug.Log($"{player_type} moveInput: {moveInput}");
        }
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (IsCorrectDevice(context.control.device))
        {
            jumpPressed = true;
            if (debugInput) Debug.Log($"{player_type} jump pressed");
        }
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (IsCorrectDevice(context.control.device))
            interactPressed = true;
    }

    private void OnInteractHeld(InputAction.CallbackContext context)
    {
        if (IsCorrectDevice(context.control.device))
            interactHeld = context.ReadValueAsButton();
    }

    private void OnRotate(InputAction.CallbackContext context)
    {
        if (IsCorrectDevice(context.control.device))
            rotationInput = context.ReadValue<float>();
    }
    
    private void OnSprint(InputAction.CallbackContext context)
    {
        if (IsCorrectDevice(context.control.device))
        {
            sprintHeld = context.ReadValueAsButton();
            if (debugInput) Debug.Log($"{player_type} sprint: {sprintHeld}");
        }
    }

    private bool IsCorrectDevice(InputDevice device)
    {
        if (device is Keyboard keyboard)
        {
            return IsCorrectKeyboardPlayer(keyboard);
        }
        else if (device is Gamepad gamepad)
        {
            var gamepads = Gamepad.all;
            
            if (player_type == Player_Type.Luthe)
            {
                return gamepads.Count > 0 && gamepad == gamepads[0];
            }
            else
            {
                return gamepads.Count > 1 && gamepad == gamepads[1];
            }
        }
        
        return false;
    }

    private bool IsCorrectKeyboardPlayer(Keyboard keyboard)
    {
        if (player_type == Player_Type.Luthe)
        {
            return keyboard.wKey.isPressed || keyboard.aKey.isPressed || 
                   keyboard.sKey.isPressed || keyboard.dKey.isPressed ||
                   keyboard.spaceKey.isPressed || keyboard.eKey.isPressed ||
                   keyboard.qKey.isPressed || keyboard.rKey.isPressed ||
                   keyboard.leftShiftKey.isPressed;
        }
        else
        {
            return keyboard.upArrowKey.isPressed || keyboard.downArrowKey.isPressed ||
                   keyboard.leftArrowKey.isPressed || keyboard.rightArrowKey.isPressed ||
                   keyboard.rightShiftKey.isPressed || keyboard.enterKey.isPressed ||
                   keyboard.commaKey.isPressed || keyboard.periodKey.isPressed ||
                   keyboard.rightCtrlKey.isPressed;
        }
    }

    void Update()
    {
        HandleKeyboardInputDirect();
        
        if (!is_in_push_mode)
        {
            movement_script.Move(moveInput);
            movement_script.SetSprintInput(sprintHeld);

            if (jumpPressed)
            {
                movement_script.Jump();
                jumpPressed = false;
            }
        }
        else
        {
            movement_script.Move(Vector2.zero);
            movement_script.SetSprintInput(false);
        }

        if (interactPressed)
        {
            interactPressed = false;
        }
    }

    private void HandleKeyboardInputDirect()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        Vector2 keyboardInput = Vector2.zero;
        bool keyboardSprint = false;
        bool keyboardJump = false;
        bool keyboardInteract = false;
        bool keyboardInteractHeld = false;

        if (player_type == Player_Type.Luthe)
        {
            if (keyboard.wKey.isPressed) keyboardInput.y += 1;
            if (keyboard.sKey.isPressed) keyboardInput.y -= 1;
            if (keyboard.aKey.isPressed) keyboardInput.x -= 1;
            if (keyboard.dKey.isPressed) keyboardInput.x += 1;
            
            keyboardSprint = keyboard.leftShiftKey.isPressed;
            keyboardJump = keyboard.spaceKey.wasPressedThisFrame;
            keyboardInteract = keyboard.eKey.wasPressedThisFrame;
            keyboardInteractHeld = keyboard.eKey.isPressed;
        }
        else
        {
            if (keyboard.upArrowKey.isPressed) keyboardInput.y += 1;
            if (keyboard.downArrowKey.isPressed) keyboardInput.y -= 1;
            if (keyboard.leftArrowKey.isPressed) keyboardInput.x -= 1;
            if (keyboard.rightArrowKey.isPressed) keyboardInput.x += 1;
            
            keyboardSprint = keyboard.rightShiftKey.isPressed;
            keyboardJump = keyboard.enterKey.wasPressedThisFrame;
            keyboardInteract = keyboard.commaKey.wasPressedThisFrame;
            keyboardInteractHeld = keyboard.commaKey.isPressed;
        }

        if (keyboardInput.magnitude > 0.1f)
        {
            moveInput = keyboardInput;
        }
        else if (Gamepad.all.Count == 0 || (player_type == Player_Type.Luthe && Gamepad.all.Count < 1) || (player_type == Player_Type.Cherie && Gamepad.all.Count < 2))
        {
            moveInput = Vector2.zero;
        }

        if (keyboardSprint) sprintHeld = true;
        if (keyboardJump) jumpPressed = true;
        if (keyboardInteract) interactPressed = true;
        if (keyboardInteractHeld) interactHeld = true;
    }

    private void OnExit(InputAction.CallbackContext context)
    {
        if (IsCorrectDevice(context.control.device))
            exitPressed = true;
    }
    public bool Get_Exit_Input()
    {
        bool result = exitPressed;
        exitPressed = false;
        return result;
    }

    public Player_Type Get_Player_Type()
    {
        return player_type;
    }

    public Vector3 Get_Movement_Input()
    {
        return new Vector3(moveInput.x, 0, moveInput.y);
    }

    public bool Get_Interact_Input()
    {
        bool result = interactPressed;
        interactPressed = false;
        return result;
    }

    public void Set_Push_Mode(bool pushing)
    {
        is_in_push_mode = pushing;
    }

    public bool Get_Interact_Held()
    {
        return interactHeld;
    }
    
    public float Get_Rotation_Input()
    {
        return rotationInput;
    }
    
    public bool Get_Sprint_Input()
    {
        return sprintHeld;
    }
}