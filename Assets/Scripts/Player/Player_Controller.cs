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
        if (player_type == Player_Type.Luthe)
        {
            float horizontal = 0f;
            float vertical = 0f;
            
            if (Input.GetKey(KeyCode.A)) horizontal = -1f;
            if (Input.GetKey(KeyCode.D)) horizontal = 1f;
            if (Input.GetKey(KeyCode.W)) vertical = 1f;
            if (Input.GetKey(KeyCode.S)) vertical = -1f;
            
            return new Vector2(horizontal, vertical);
        }
        else
        {
            float horizontal = 0f;
            float vertical = 0f;
            
            if (Input.GetKey(KeyCode.LeftArrow)) horizontal = -1f;
            if (Input.GetKey(KeyCode.RightArrow)) horizontal = 1f;
            if (Input.GetKey(KeyCode.UpArrow)) vertical = 1f;
            if (Input.GetKey(KeyCode.DownArrow)) vertical = -1f;
            
            return new Vector2(horizontal, vertical);
        }
    }
    
    private bool GetJumpInput()
    {
        if (player_type == Player_Type.Luthe)
            return Input.GetKeyDown(KeyCode.Space);
        else
            return Input.GetKeyDown(KeyCode.RightShift);
    }
    
    private bool GetInteractInput()
    {
        if (player_type == Player_Type.Luthe)
            return Input.GetKeyDown(KeyCode.E);
        else
            return Input.GetKeyDown(KeyCode.Return);
    }
}