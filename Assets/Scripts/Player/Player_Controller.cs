using UnityEngine;

public enum Player_Type { Luthe, Cherie }

public class Player_Controller : MonoBehaviour
{
    [SerializeField] private Player_Type player_type;
    private Player_Movement movement_script;
    
    void Start()
    {
        movement_script = GetComponent<Player_Movement>();
    }
    
    void Update()
    {
        Vector2 input = Get_Input();
        movement_script.Move(input);
        
        if (GetJumpInput())
            movement_script.Jump();
        
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
}