using UnityEngine;
using UnityEngine.InputSystem;

public enum Player_Type { Luthe, Cherie }

public class Player_Controller : MonoBehaviour
{
    [SerializeField] private Player_Type player_type;
    [SerializeField] private InputSystem_Actions inputActions;
    
    private Player_Movement movement_script;
    private bool is_in_push_mode = false;
    
    private Vector2 moveInput;
    private bool jumpPressed;
    private bool interactPressed;
    private bool interactHeld;
    private float rotationInput;

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
        }
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        if (IsCorrectDevice(context.control.device))
        {
            moveInput = context.ReadValue<Vector2>();
        }
        else if (context.canceled)
        {
            if (IsPlayerKeyboard(context.control.device))
            {
                moveInput = Vector2.zero;
            }
        }
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (IsCorrectDevice(context.control.device))
            jumpPressed = true;
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

    private bool IsCorrectDevice(InputDevice device)
    {
        if (device is Keyboard)
        {
            return IsCorrectKeyboardInput(device as Keyboard);
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

    private bool IsPlayerKeyboard(InputDevice device)
    {
        return device is Keyboard;
    }

    private bool IsCorrectKeyboardInput(Keyboard keyboard)
    {
        if (player_type == Player_Type.Luthe)
        {
            return keyboard.wKey.isPressed || keyboard.aKey.isPressed || 
                   keyboard.sKey.isPressed || keyboard.dKey.isPressed ||
                   keyboard.spaceKey.isPressed || keyboard.eKey.isPressed ||
                   keyboard.qKey.isPressed || keyboard.rKey.isPressed;
        }
        else
        {
            return keyboard.upArrowKey.isPressed || keyboard.downArrowKey.isPressed ||
                   keyboard.leftArrowKey.isPressed || keyboard.rightArrowKey.isPressed ||
                   keyboard.rightShiftKey.isPressed || keyboard.enterKey.isPressed ||
                   keyboard.commaKey.isPressed || keyboard.periodKey.isPressed;
        }
    }

    void Update()
    {
        if (!is_in_push_mode)
        {
            movement_script.Move(moveInput);

            if (jumpPressed)
            {
                movement_script.Jump();
                jumpPressed = false;
            }
        }
        else
        {
            movement_script.Move(Vector2.zero);
        }

        if (interactPressed)
        {
            interactPressed = false;
        }
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
}