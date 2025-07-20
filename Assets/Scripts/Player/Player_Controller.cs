using UnityEngine;

public enum Player_Type { Luthe, Cherie }

public class Player_Controller : MonoBehaviour
{
    [SerializeField] private Player_Type player_type;
    private Player_Movement movement_script;

    private Pickable_Object held_object = null;
    private Transform hold_point;

    private bool is_in_push_mode = false;


    void Start()
    {
        movement_script = GetComponent<Player_Movement>();
        Create_Hold_Point();
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
        try
        {
            if (player_type == Player_Type.Luthe)
            {
                float h = Input.GetAxis("Joy1_Horizontal");
                float v = Input.GetAxis("Joy1_Vertical");

                if (Mathf.Abs(h) < 0.3f) h = 0f;
                if (Mathf.Abs(v) < 0.3f) v = 0f;

                if (Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f)
                    return new Vector2(h, v);
            }
            else
            {
                float h = Input.GetAxis("Joy2_Horizontal");
                float v = Input.GetAxis("Joy2_Vertical");

                if (Mathf.Abs(h) < 0.2f) h = 0f;
                if (Mathf.Abs(v) < 0.2f) v = 0f;

                if (Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f)
                    return new Vector2(h, v);
            }
        }
        catch
        {
        }

        return Get_Keyboard_Input();
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
        bool interact_input = false;
        
        if (player_type == Player_Type.Luthe)
        {
            if (Input.GetKeyDown("joystick 1 button 2")) interact_input = true;
            if (Input.GetKeyDown(KeyCode.E)) interact_input = true;
        }
        else
        {
            if (Input.GetKeyDown("joystick 2 button 2")) interact_input = true;
            if (Input.GetKeyDown(KeyCode.Return)) interact_input = true;
        }
        
        return interact_input;
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

    private void Create_Hold_Point()
    {
        GameObject hold_point_obj = new GameObject("Hold_Point");
        hold_point_obj.transform.SetParent(transform);
        hold_point_obj.transform.localPosition = new Vector3(0, 1.5f, 0.8f);
        hold_point = hold_point_obj.transform;
    }

        public Transform Get_Hold_Point()
    {
        return hold_point;
    }

    public Pickable_Object Get_Held_Object()
    {
        return held_object;
    }

    public void Set_Held_Object(Pickable_Object obj)
    {
        held_object = obj;
    }
    }