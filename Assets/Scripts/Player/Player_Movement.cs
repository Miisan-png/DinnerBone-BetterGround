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
    
    private CharacterController controller;
    private Vector3 velocity;
    private Vector3 target_velocity;
    private Vector3 last_movement_direction;
    private bool is_grounded;
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (player_mesh == null)
            player_mesh = transform.GetChild(0);
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
        
        Vector3 horizontal_velocity = new Vector3(velocity.x, 0, velocity.z);
        
        if (horizontal_velocity.magnitude > 0.1f)
        {
            last_movement_direction = horizontal_velocity.normalized;
            
            Quaternion target_rotation = Quaternion.LookRotation(last_movement_direction);
            player_mesh.rotation = Quaternion.Lerp(player_mesh.rotation, target_rotation, rotation_speed * Time.deltaTime);
            
            float lean_amount = Vector3.Dot(transform.right, last_movement_direction) * lean_angle;
            Vector3 lean_euler = new Vector3(0, player_mesh.eulerAngles.y, -lean_amount);
            player_mesh.rotation = Quaternion.Lerp(player_mesh.rotation, Quaternion.Euler(lean_euler), rotation_speed * Time.deltaTime);
        }
        else
        {
            Vector3 neutral_euler = new Vector3(0, player_mesh.eulerAngles.y, 0);
            player_mesh.rotation = Quaternion.Lerp(player_mesh.rotation, Quaternion.Euler(neutral_euler), rotation_speed * Time.deltaTime);
        }
    }
}