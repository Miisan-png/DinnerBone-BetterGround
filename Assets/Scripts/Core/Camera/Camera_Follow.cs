using UnityEngine;

public class Camera_Follow : MonoBehaviour
{
    [SerializeField] private float horizontal_follow_speed = 8f;
    [SerializeField] private float distance_offset = 2f;
    [SerializeField] private float movement_threshold = 0.1f;
    
    private Camera target_camera;
    private Transform player_1;
    private Transform player_2;
    private Vector3 base_position;
    private Quaternion base_rotation;
    private Vector3 base_offset;
    private bool split_mode_enabled = false;
    private Vector3 last_p1_pos;
    private Vector3 last_p2_pos;
    private Transform active_player;
    
    public void Initialize(Camera camera, Transform p1, Transform p2)
    {
        target_camera = camera;
        player_1 = p1;
        player_2 = p2;
        base_position = target_camera.transform.position;
        base_rotation = target_camera.transform.rotation;
        base_offset = base_position - GetCenterPosition();
        target_camera.orthographic = false;
        last_p1_pos = player_1.position;
        last_p2_pos = player_2.position;
        active_player = player_1;
    }
    
    void LateUpdate()
    {
        if (player_1 == null || player_2 == null || target_camera == null) return;
        
        Vector3 target_position;
        
        if (split_mode_enabled)
        {
            UpdateActivePlayer();
            target_position = active_player.position + base_offset;
        }
        else
        {
            Vector3 center = GetCenterPosition();
            target_position = center + base_offset;
            
            float player_distance = Vector3.Distance(player_1.position, player_2.position);
            Vector3 distance_adjustment = base_rotation * Vector3.back * (player_distance * distance_offset);
            target_position += distance_adjustment;
        }
        
        target_position.x = Mathf.Lerp(target_camera.transform.position.x, target_position.x, horizontal_follow_speed * Time.deltaTime);
        target_position.y = base_position.y;
        target_position.z = Mathf.Lerp(target_camera.transform.position.z, target_position.z, horizontal_follow_speed * Time.deltaTime);
        
        target_camera.transform.position = target_position;
        target_camera.transform.rotation = base_rotation;
        
        last_p1_pos = player_1.position;
        last_p2_pos = player_2.position;
    }
    
    private void UpdateActivePlayer()
    {
        float p1_movement = Vector3.Distance(player_1.position, last_p1_pos);
        float p2_movement = Vector3.Distance(player_2.position, last_p2_pos);
        
        if (p1_movement > movement_threshold && p2_movement <= movement_threshold)
        {
            active_player = player_1;
        }
        else if (p2_movement > movement_threshold && p1_movement <= movement_threshold)
        {
            active_player = player_2;
        }
    }
    
    public void SetSplitModeEnabled(bool enabled)
    {
        split_mode_enabled = enabled;
    }
    
    private Vector3 GetCenterPosition()
    {
        return (player_1.position + player_2.position) / 2f;
    }
}