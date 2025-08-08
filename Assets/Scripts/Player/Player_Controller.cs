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

    private void OnExit(InputAction.CallbackContext context)
    {
        if (IsCorrectDevice(context.control.device))
            exitPressed = true;
    }

    private bool IsCorrectDevice(InputDevice device)
    {
        if (device is Gamepad gamepad)
        {
            return IsCorrectGamepad(gamepad);
        }
        
        return false;
    }

    private bool IsCorrectGamepad(Gamepad gamepad)
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

    void Update()
    {
        HandleDirectInput();
        
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

    private void HandleDirectInput()
    {
        Keyboard keyboard = Keyboard.current;
        var gamepads = Gamepad.all;
        
        Vector2 directInput = Vector2.zero;
        bool directSprint = false;
        bool directJump = false;
        bool directInteract = false;
        bool directInteractHeld = false;
        bool directExit = false;

        if (player_type == Player_Type.Luthe)
        {
            if (gamepads.Count > 0)
            {
                var gamepad = gamepads[0];
                directInput = gamepad.leftStick.ReadValue();
                directSprint = gamepad.leftShoulder.isPressed;
                directJump = gamepad.buttonSouth.wasPressedThisFrame;
                directInteract = gamepad.buttonWest.wasPressedThisFrame;
                directInteractHeld = gamepad.buttonWest.isPressed;
                directExit = gamepad.buttonEast.wasPressedThisFrame;
            }
            
            if (keyboard != null)
            {
                Vector2 kbInput = Vector2.zero;
                if (keyboard.wKey.isPressed) kbInput.y += 1;
                if (keyboard.sKey.isPressed) kbInput.y -= 1;
                if (keyboard.aKey.isPressed) kbInput.x -= 1;
                if (keyboard.dKey.isPressed) kbInput.x += 1;
                
                if (kbInput.magnitude > 0.1f) directInput = kbInput;
                
                if (keyboard.leftShiftKey.isPressed) directSprint = true;
                if (keyboard.spaceKey.wasPressedThisFrame) directJump = true;
                if (keyboard.eKey.wasPressedThisFrame) directInteract = true;
                if (keyboard.eKey.isPressed) directInteractHeld = true;
                if (keyboard.escapeKey.wasPressedThisFrame) directExit = true;
            }
        }
        else
        {
            if (gamepads.Count > 1)
            {
                var gamepad = gamepads[1];
                directInput = gamepad.leftStick.ReadValue();
                directSprint = gamepad.leftShoulder.isPressed;
                directJump = gamepad.buttonSouth.wasPressedThisFrame;
                directInteract = gamepad.buttonWest.wasPressedThisFrame;
                directInteractHeld = gamepad.buttonWest.isPressed;
                directExit = gamepad.buttonEast.wasPressedThisFrame;
            }
            
            if (keyboard != null)
            {
                Vector2 kbInput = Vector2.zero;
                if (keyboard.upArrowKey.isPressed) kbInput.y += 1;
                if (keyboard.downArrowKey.isPressed) kbInput.y -= 1;
                if (keyboard.leftArrowKey.isPressed) kbInput.x -= 1;
                if (keyboard.rightArrowKey.isPressed) kbInput.x += 1;
                
                if (kbInput.magnitude > 0.1f) directInput = kbInput;
                
                if (keyboard.rightShiftKey.isPressed) directSprint = true;
                if (keyboard.enterKey.wasPressedThisFrame) directJump = true;
                if (keyboard.commaKey.wasPressedThisFrame) directInteract = true;
                if (keyboard.commaKey.isPressed) directInteractHeld = true;
                if (keyboard.periodKey.wasPressedThisFrame) directExit = true;
            }
        }

        moveInput = directInput;
        sprintHeld = directSprint;
        if (directJump) jumpPressed = true;
        if (directInteract) interactPressed = true;
        interactHeld = directInteractHeld;
        if (directExit) exitPressed = true;
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
    
    public bool Get_Exit_Input()
    {
        bool result = exitPressed;
        exitPressed = false;
        return result;
    }
}