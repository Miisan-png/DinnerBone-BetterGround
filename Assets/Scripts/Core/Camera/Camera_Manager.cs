using UnityEngine;

public enum Camera_Mode { Follow, Split_Screen }

public class Camera_Manager : MonoBehaviour
{
    [Header("Camera References")]
    [SerializeField] private Camera main_camera;
    [SerializeField] private Camera split_camera_1;
    [SerializeField] private Camera split_camera_2;
    [SerializeField] private Transform player_1;
    [SerializeField] private Transform player_2;
    
    [Header("Camera Settings")]
    [SerializeField] private float split_distance = 15f;
    [SerializeField] private float transition_speed = 2f;
    [SerializeField] private bool force_split_screen = false;
    [SerializeField] private bool force_follow_mode = false;
    
    [Header("Debug")]
    [SerializeField] private bool show_debug_info = false;
    
    private Camera_Mode current_mode = Camera_Mode.Follow;
    private Camera_Follow follow_script;
    private Split_Screen_Controller split_script;
    
    void Start()
    {
        follow_script = GetComponent<Camera_Follow>();
        split_script = GetComponent<Split_Screen_Controller>();
        
        follow_script.Initialize(main_camera, player_1, player_2);
        split_script.Initialize(split_camera_1, split_camera_2, player_1, player_2);
        
        SetCameraMode(force_split_screen ? Camera_Mode.Split_Screen : Camera_Mode.Follow);
    }
    
    void Update()
    {
        // Skip automatic switching if forced modes are enabled
        if (force_split_screen || force_follow_mode) return;
        
        float distance = Vector3.Distance(player_1.position, player_2.position);
        
        if (distance > split_distance && current_mode == Camera_Mode.Follow)
        {
            SetCameraMode(Camera_Mode.Split_Screen);
        }
        else if (distance <= split_distance && current_mode == Camera_Mode.Split_Screen)
        {
            SetCameraMode(Camera_Mode.Follow);
        }
        
        if (show_debug_info)
        {
            Debug.Log($"Player Distance: {distance:F1} | Mode: {current_mode} | Split Threshold: {split_distance}");
        }
    }
    
    private void SetCameraMode(Camera_Mode mode)
    {
        current_mode = mode;
        
        if (mode == Camera_Mode.Follow)
        {
            main_camera.gameObject.SetActive(true);
            split_camera_1.gameObject.SetActive(false);
            split_camera_2.gameObject.SetActive(false);
            follow_script.enabled = true;
            split_script.enabled = false;
        }
        else
        {
            main_camera.gameObject.SetActive(false);
            split_camera_1.gameObject.SetActive(true);
            split_camera_2.gameObject.SetActive(true);
            follow_script.enabled = false;
            split_script.enabled = true;
        }
        
        if (show_debug_info)
        {
            Debug.Log($"Camera switched to: {mode}");
        }
    }
    
    [ContextMenu("Force Split Screen")]
    public void Force_Split_Screen()
    {
        force_split_screen = true;
        force_follow_mode = false;
        SetCameraMode(Camera_Mode.Split_Screen);
    }
    
    [ContextMenu("Force Follow Mode")]
    public void Force_Follow_Mode()
    {
        force_follow_mode = true;
        force_split_screen = false;
        SetCameraMode(Camera_Mode.Follow);
    }
    
    [ContextMenu("Enable Auto Switch")]
    public void Enable_Auto_Switch()
    {
        force_split_screen = false;
        force_follow_mode = false;
    }
    
    public void Set_Split_Distance(float distance)
    {
        split_distance = distance;
    }
    
    public Camera_Mode Get_Current_Mode()
    {
        return current_mode;
    }
    
    void OnDrawGizmos()
    {
        if (show_debug_info && player_1 != null && player_2 != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(player_1.position, player_2.position);
            
            Vector3 center = (player_1.position + player_2.position) / 2f;
            Gizmos.color = current_mode == Camera_Mode.Split_Screen ? Color.red : Color.green;
            Gizmos.DrawWireSphere(center, split_distance / 2f);
        }
    }
}