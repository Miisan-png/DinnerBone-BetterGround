using UnityEngine;

public class Player_Movement : MonoBehaviour
{
    [SerializeField] private float move_speed = 5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float jump_force = 8f;
    [SerializeField] private float gravity = -20f;
    
    private CharacterController controller;
    private Vector3 velocity;
    private Vector3 target_velocity;
    private bool is_grounded;
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }
    
    void Update()
    {
        is_grounded = controller.isGrounded;
        
        if (is_grounded && velocity.y < 0)
            velocity.y = -2f;
        
        Vector3 horizontal_velocity = Vector3.Lerp(
            new Vector3(velocity.x, 0, velocity.z), 
            target_velocity, 
            acceleration * Time.deltaTime
        );
        
        velocity.x = horizontal_velocity.x;
        velocity.z = horizontal_velocity.z;
        velocity.y += gravity * Time.deltaTime;
        
        controller.Move(velocity * Time.deltaTime);
    }
    
    public void Move(Vector2 input)
    {
        target_velocity = new Vector3(input.x, 0, input.y) * move_speed;
    }
    
    public void Jump()
    {
        if (is_grounded)
            velocity.y = jump_force;
    }
}