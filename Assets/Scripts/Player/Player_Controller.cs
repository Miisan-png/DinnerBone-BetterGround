using UnityEngine;

public enum Player_Type { Luthe, Cherie }

public class Player_Controller : MonoBehaviour
{
    [SerializeField] private Player_Type player_type;
    [SerializeField] private bool use_controller = false; 
    private Player_Movement movement_script;

    private bool is_in_push_mode = false;


    void Start()
    {
        movement_script = GetComponent<Player_Movement>();
    }

    void Update()
    {
        Vector2 input = Get_Input();

        if (!is_in_push_mode)
        {
            movement_script.Move(input);

            if (GetJumpInput())
                movement_script.Jump();
        }
        else
        {
            movement_script.Move(Vector2.zero);
        }

        if (GetInteractInput())
        {

        }
    }

    private Vector2 Get_Input()
    {
        Vector2 controllerInput = Vector2.zero;
        bool hasControllerInput = false;
        
        try
        {
            if (player_type == Player_Type.Luthe)
            {
                float h = Input.GetAxis("Joy1_Horizontal");
                float v = Input.GetAxis("Joy1_Vertical");

                if (Mathf.Abs(h) < 0.3f) h = 0f;
                if (Mathf.Abs(v) < 0.3f) v = 0f;

                if (Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f)
                {
                    controllerInput = new Vector2(h, v);
                    hasControllerInput = true;
                }
            }
            else
            {
                float h = Input.GetAxis("Joy2_Horizontal");
                float v = Input.GetAxis("Joy2_Vertical");

                if (Mathf.Abs(h) < 0.3f) h = 0f;
                if (Mathf.Abs(v) < 0.3f) v = 0f;

                if (Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f)
                {
                    controllerInput = new Vector2(h, v);
                    hasControllerInput = true;
                }
            }
        }
        catch
        {
            // Controller input  // Imma keep
        }

        Vector2 keyboardInput = Get_Keyboard_Input();
        bool hasKeyboardInput = keyboardInput != Vector2.zero;

        if (hasControllerInput)
        {
            use_controller = true;
            return controllerInput;
        }
        else if (hasKeyboardInput)
        {
            use_controller = false;
            return keyboardInput;
        }
        
        // No input detected, return based on current mode
        return use_controller ? controllerInput : keyboardInput;
    }
    private Vector2 Get_Keyboard_Input()
    {
        float horizontal = 0f;
        float vertical = 0f;

        if (player_type == Player_Type.Luthe)
        {
            if (Input.GetKey(KeyCode.A)) horizontal = -1f;
            if (Input.GetKey(KeyCode.D)) horizontal = 1f;
            if (Input.GetKey(KeyCode.W)) vertical = 1f;
            if (Input.GetKey(KeyCode.S)) vertical = -1f;
        }
        else
        {
            if (Input.GetKey(KeyCode.LeftArrow)) horizontal = -1f;
            if (Input.GetKey(KeyCode.RightArrow)) horizontal = 1f;
            if (Input.GetKey(KeyCode.UpArrow)) vertical = 1f;
            if (Input.GetKey(KeyCode.DownArrow)) vertical = -1f;
        }

        return new Vector2(horizontal, vertical);
    }

    private bool GetJumpInput()
    {
        if (player_type == Player_Type.Luthe)
        {
            if (Input.GetKeyDown("joystick 1 button 0")) return true;
            return Input.GetKeyDown(KeyCode.Space);
        }
        else
        {
            if (Input.GetKeyDown("joystick 2 button 0")) return true;
            return Input.GetKeyDown(KeyCode.RightShift);
        }
    }

    private bool GetInteractInput()
    {
        if (player_type == Player_Type.Luthe)
        {
            if (Input.GetKeyDown("joystick 1 button 2")) return true;
            return Input.GetKeyDown(KeyCode.E);
        }
        else
        {
            if (Input.GetKeyDown("joystick 2 button 2")) return true;
            return Input.GetKeyDown(KeyCode.Return);
        }
    }

    // ----------- INTERACTION SYSTEM UTILS -------------   
    public Player_Type Get_Player_Type()
    {
        return player_type;
    }

    public Vector3 Get_Movement_Input()
    {
        Vector2 input = Get_Input();
        return new Vector3(input.x, 0, input.y);
    }

    public bool Get_Interact_Input()
    {
        return GetInteractInput();
    }

    public void Set_Push_Mode(bool pushing)
    {
        is_in_push_mode = pushing;
    }

    public bool Get_Interact_Held()
    {
        if (player_type == Player_Type.Luthe)
        {
            if (Input.GetKey("joystick 1 button 2")) return true;
            return Input.GetKey(KeyCode.E);
        }
        else
        {
            if (Input.GetKey("joystick 2 button 2")) return true;
            return Input.GetKey(KeyCode.Return);
        }
    }
    
    public float Get_Rotation_Input()
{
    float rotation = 0f;
    
    if (player_type == Player_Type.Luthe)
    {
        // Controller 1 - L2/R2 buttons
        if (Input.GetKey("joystick 1 button 6")) rotation = -1f;  // L2 = rotate left
        if (Input.GetKey("joystick 1 button 7")) rotation = 1f;   // R2 = rotate right
        
        // Keyboard fallback
        if (Input.GetKey(KeyCode.Q)) rotation = -1f;  // Q = rotate left
        if (Input.GetKey(KeyCode.R)) rotation = 1f;   // R = rotate right
    }
    else
    {
        // Controller 2 - L2/R2 buttons
        if (Input.GetKey("joystick 2 button 6")) rotation = -1f;  // L2 = rotate left
        if (Input.GetKey("joystick 2 button 7")) rotation = 1f;   // R2 = rotate right
        
        // Keyboard fallback
        if (Input.GetKey(KeyCode.Comma)) rotation = -1f;   // , = rotate left
        if (Input.GetKey(KeyCode.Period)) rotation = 1f;   // . = rotate right
    }
    
    return rotation;
}
}