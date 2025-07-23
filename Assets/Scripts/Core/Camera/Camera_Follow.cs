using UnityEngine;

public class Camera_Follow : MonoBehaviour
{
    [SerializeField] private float follow_speed = 5f;
    [SerializeField] private float zoom_speed = 2f;
    [SerializeField] private float min_zoom = 5f;
    [SerializeField] private float max_zoom = 15f;
    [SerializeField] private Vector3 offset = new Vector3(0, 10, -8);
    [SerializeField] private float rotation_speed = 2f;
    [SerializeField] private float max_rotation_angle = 15f;
    [SerializeField] private float look_ahead_distance = 3f;
    [SerializeField] private float tilt_sensitivity = 0.5f;
    
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
        if (player_1 == null || player_2 == null) return;
        
        Vector3 center = GetCenterPosition();
        UpdateVelocityDirection(center);
        
        Vector3 look_ahead = center + velocity_direction * look_ahead_distance;
        Vector3 target_position = look_ahead + offset;
        
        transform.position = Vector3.Lerp(transform.position, target_position, follow_speed * Time.deltaTime);
        
        UpdateCameraRotation(center);
        UpdateCameraZoom();
        
        last_center_position = center;
    }
    
    private Vector3 GetCenterPosition()
    {
        return (player_1.position + player_2.position) / 2f;
    }
    
    private void UpdateVelocityDirection(Vector3 current_center)
    {
        Vector3 movement = (current_center - last_center_position) / Time.deltaTime;
        velocity_direction = Vector3.Lerp(velocity_direction, movement.normalized, Time.deltaTime * 2f);
    }
    
    private void UpdateCameraRotation(Vector3 center)
    {
        Vector3 direction_to_players = (center - transform.position).normalized;
        Vector3 player_separation = (player_2.position - player_1.position);
        
        float horizontal_tilt = player_separation.x * tilt_sensitivity;
        float vertical_tilt = velocity_direction.magnitude * 5f;
        
        horizontal_tilt = Mathf.Clamp(horizontal_tilt, -max_rotation_angle, max_rotation_angle);
        vertical_tilt = Mathf.Clamp(vertical_tilt, -max_rotation_angle * 0.5f, max_rotation_angle * 0.5f);
        
        Quaternion target_rotation = Quaternion.LookRotation(direction_to_players) * 
                                   Quaternion.Euler(vertical_tilt, horizontal_tilt, -horizontal_tilt * 0.3f);
        
        transform.rotation = Quaternion.Lerp(transform.rotation, target_rotation, rotation_speed * Time.deltaTime);
    }
    
    private void UpdateCameraZoom()
    {
        float distance = Vector3.Distance(player_1.position, player_2.position);
        float velocity_factor = velocity_direction.magnitude * 0.5f;
        
        if (target_camera.orthographic)
        {
            float target_size = Mathf.Clamp(distance * 0.5f + min_zoom + velocity_factor, min_zoom, max_zoom);
            target_camera.orthographicSize = Mathf.Lerp(target_camera.orthographicSize, target_size, zoom_speed * Time.deltaTime);
        }
        else
        {
            float target_fov = Mathf.Clamp(distance * 2f + (min_zoom * 4f) + velocity_factor, min_zoom * 4f, max_zoom * 4f);
            target_camera.fieldOfView = Mathf.Lerp(target_camera.fieldOfView, target_fov, zoom_speed * Time.deltaTime);
        }
    }
}