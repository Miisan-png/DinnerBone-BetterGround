using UnityEngine;

public class Camera_Follow : MonoBehaviour
{
    [SerializeField] private float follow_speed = 5f;
    [SerializeField] private float zoom_speed = 2f;
    [SerializeField] private float min_zoom = 5f;
    [SerializeField] private float max_zoom = 15f;
    [SerializeField] private Vector3 offset = new Vector3(0, 10, -8);
    [SerializeField] private float rotation_damping = 3f;
    [SerializeField] private float tilt_strength = 0.3f;
    
    private Camera target_camera;
    private Transform player_1;
    private Transform player_2;
    private Vector3 velocity_direction;
    private Vector3 last_center_position;
    
    public void Initialize(Camera camera, Transform p1, Transform p2)
    {
        target_camera = camera;
        player_1 = p1;
        player_2 = p2;
        last_center_position = GetCenterPosition();
        target_camera.orthographic = false;
        target_camera.fieldOfView = 40f;
    }
    
    void LateUpdate()
    {
        if (player_1 == null || player_2 == null || target_camera == null) return;
        
        Vector3 center = GetCenterPosition();
        UpdateMovementDirection(center);
        
        Vector3 target_position = center + offset;
        target_camera.transform.position = Vector3.Lerp(target_camera.transform.position, target_position, follow_speed * Time.deltaTime);
        
        Vector3 look_target = center + velocity_direction * 2f;
        Vector3 look_direction = (look_target - target_camera.transform.position).normalized;
        
        float player_separation = (player_2.position - player_1.position).x;
        float tilt = player_separation * tilt_strength;
        
        Quaternion base_rotation = Quaternion.LookRotation(look_direction);
        Quaternion tilted_rotation = base_rotation * Quaternion.Euler(0, 0, -tilt);
        
        target_camera.transform.rotation = Quaternion.Lerp(target_camera.transform.rotation, tilted_rotation, rotation_damping * Time.deltaTime);
        
        UpdateZoom();
        
        last_center_position = center;
    }
    
    private Vector3 GetCenterPosition()
    {
        return (player_1.position + player_2.position) / 2f;
    }
    
    private void UpdateMovementDirection(Vector3 current_center)
    {
        Vector3 movement = (current_center - last_center_position) / Time.deltaTime;
        velocity_direction = Vector3.Lerp(velocity_direction, movement.normalized, Time.deltaTime);
    }
    
    private void UpdateZoom()
    {
        float distance = Vector3.Distance(player_1.position, player_2.position);
        float target_fov = Mathf.Clamp(35f + (distance * 2f), 30f, 60f);
        target_camera.fieldOfView = Mathf.Lerp(target_camera.fieldOfView, target_fov, zoom_speed * Time.deltaTime);
    }
}