using UnityEngine;

public class Vent_Camera : MonoBehaviour
{
    [SerializeField] private float follow_speed = 5f;
    [SerializeField] private float fixed_height = 8f;
    [SerializeField] private float fixed_distance = 6f;
    [SerializeField] private float fixed_fov = 45f;
    [SerializeField] private Vector2 movement_bounds = new Vector2(-20f, 20f);
    
    private Camera target_camera;
    private Transform target_player;
    private Vector3 initial_position;
    
    public void Initialize(Camera camera, Transform player)
    {
        target_camera = camera;
        target_player = player;
        
        if (target_camera != null)
        {
            target_camera.orthographic = false;
            target_camera.fieldOfView = fixed_fov;
            
            initial_position = new Vector3(
                target_player.position.x,
                fixed_height,
                target_player.position.z - fixed_distance
            );
            
            target_camera.transform.position = initial_position;
            target_camera.transform.LookAt(new Vector3(target_player.position.x, target_player.position.y, target_player.position.z));
        }
    }
    
    void LateUpdate()
    {
        if (target_camera == null || target_player == null) return;
        
        float target_x = Mathf.Clamp(target_player.position.x, movement_bounds.x, movement_bounds.y);
        
        Vector3 target_position = new Vector3(
            target_x,
            fixed_height,
            target_player.position.z - fixed_distance
        );
        
        target_camera.transform.position = Vector3.Lerp(
            target_camera.transform.position,
            target_position,
            follow_speed * Time.deltaTime
        );
        
        Vector3 look_target = new Vector3(target_player.position.x, target_player.position.y, target_player.position.z);
        target_camera.transform.LookAt(look_target);
    }
    
    public void Set_Movement_Bounds(Vector2 bounds)
    {
        movement_bounds = bounds;
    }
    
    void OnDrawGizmos()
    {
        if (target_player != null)
        {
            Gizmos.color = Color.blue;
            Vector3 left_bound = new Vector3(movement_bounds.x, fixed_height, target_player.position.z - fixed_distance);
            Vector3 right_bound = new Vector3(movement_bounds.y, fixed_height, target_player.position.z - fixed_distance);
            
            Gizmos.DrawLine(left_bound, right_bound);
            Gizmos.DrawWireSphere(left_bound, 0.5f);
            Gizmos.DrawWireSphere(right_bound, 0.5f);
        }
    }
}