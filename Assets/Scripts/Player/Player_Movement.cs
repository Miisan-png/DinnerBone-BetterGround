using UnityEngine;

public class Player_Movement : MonoBehaviour
{
    [SerializeField] private float move_speed = 5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float jump_force = 8f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float rotation_speed = 10f;
    [SerializeField] private float lean_angle = 15f;
    [SerializeField] private Transform player_mesh;
    [SerializeField] private float rotation_threshold = 0.1f; // Minimum input to trigger rotation
    
    private CharacterController controller;
    private Vector3 velocity;
    private Vector3 target_velocity;
    private Vector3 last_movement_direction;
    private Vector2 current_input; // Store the raw input
    private bool is_grounded;
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (player_mesh == null)
        {
            // Try to find the mesh in children
            player_mesh = transform.GetChild(0);
            
            // If still null, use the main transform
            if (player_mesh == null)
            {
                Debug.LogWarning($"Player mesh not found for {gameObject.name}. Using main transform for rotation.");
                player_mesh = transform;
            }
        }
        
        Debug.Log($"Player mesh assigned to: {player_mesh.name}");
    }
    
    void Update()
    {
        if (controller == null || !controller.enabled) return;
        
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
        
        UpdateMeshRotation();
    }
    
    public void Move(Vector2 input)
    {
        current_input = input; // Store the input for rotation
        target_velocity = new Vector3(input.x, 0, input.y) * move_speed;
    }
    
    public void Jump()
    {
        if (controller != null && controller.enabled && is_grounded)
            velocity.y = jump_force;
    }
    
    private void UpdateMeshRotation()
    {
        if (player_mesh == null) return;
        
        // Use input direction instead of velocity for more responsive rotation
        Vector3 input_direction = new Vector3(current_input.x, 0, current_input.y);
        
        // Check if there's enough input to warrant rotation
        if (input_direction.magnitude > rotation_threshold)
        {
            // Store this as the last movement direction for future reference
            last_movement_direction = input_direction.normalized;
            
            // Calculate target rotation based on input direction
            Quaternion target_rotation = Quaternion.LookRotation(last_movement_direction);
            
            // Apply rotation smoothly
            player_mesh.rotation = Quaternion.Lerp(
                player_mesh.rotation, 
                target_rotation, 
                rotation_speed * Time.deltaTime
            );
            
            // Apply lean effect (optional - you can comment this out if you don't want leaning)
            float lean_amount = Vector3.Dot(transform.right, last_movement_direction) * lean_angle;
            Vector3 lean_euler = new Vector3(0, player_mesh.eulerAngles.y, -lean_amount);
            player_mesh.rotation = Quaternion.Lerp(
                player_mesh.rotation, 
                Quaternion.Euler(lean_euler), 
                rotation_speed * Time.deltaTime
            );
        }
        else
        {
            // When not moving, straighten out the lean but keep the forward direction
            if (last_movement_direction != Vector3.zero)
            {
                Quaternion target_rotation = Quaternion.LookRotation(last_movement_direction);
                Vector3 neutral_euler = new Vector3(0, target_rotation.eulerAngles.y, 0);
                player_mesh.rotation = Quaternion.Lerp(
                    player_mesh.rotation, 
                    Quaternion.Euler(neutral_euler), 
                    rotation_speed * Time.deltaTime
                );
            }
        }
    }
    
    // Debug method to help troubleshoot
    void OnDrawGizmos()
    {
        if (player_mesh != null && last_movement_direction != Vector3.zero)
        {
            // Draw forward direction
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(player_mesh.position, player_mesh.forward * 2f);
            
            // Draw intended direction
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, last_movement_direction * 2f);
        }
    }
}