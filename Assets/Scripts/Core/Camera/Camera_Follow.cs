using UnityEngine;

public class Camera_Follow : MonoBehaviour
{
    [SerializeField] private float follow_speed = 5f;
    [SerializeField] private float zoom_speed = 2f;
    [SerializeField] private float min_zoom = 5f;
    [SerializeField] private float max_zoom = 15f;
    [SerializeField] private Vector3 offset = new Vector3(0, 10, -8);
    
    private Camera target_camera;
    private Transform player_1;
    private Transform player_2;
    
    public void Initialize(Camera camera, Transform p1, Transform p2)
    {
        target_camera = camera;
        player_1 = p1;
        player_2 = p2;
    }
    
    void LateUpdate()
    {
        if (player_1 == null || player_2 == null) return;
        
        Vector3 center = (player_1.position + player_2.position) / 2f;
        Vector3 target_position = center + offset;
        
        transform.position = Vector3.Lerp(transform.position, target_position, follow_speed * Time.deltaTime);
        
        float distance = Vector3.Distance(player_1.position, player_2.position);
        float target_size = Mathf.Clamp(distance * 0.5f + min_zoom, min_zoom, max_zoom);
        target_camera.orthographicSize = Mathf.Lerp(target_camera.orthographicSize, target_size, zoom_speed * Time.deltaTime);
    }
}