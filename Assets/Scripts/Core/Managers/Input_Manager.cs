using UnityEngine;

public class Input_Manager : MonoBehaviour
{
    public enum Input_Type { Keyboard, Controller }
    public static Input_Type current_input_type;
    public static event System.Action<Input_Type> On_Input_Type_Changed;
    
    void Update()
    {
        Detect_Input_Type();
    }
    
    void Detect_Input_Type()
    {
        Input_Type previous_type = current_input_type;
        
        // Check for controller input
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            current_input_type = Input_Type.Controller;
        
        // Check for keyboard input
        if (Input.anyKeyDown)
            current_input_type = Input_Type.Keyboard;
            
        // Trigger event if changed
        if (previous_type != current_input_type)
            On_Input_Type_Changed?.Invoke(current_input_type);
    }
}