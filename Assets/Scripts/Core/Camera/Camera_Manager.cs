using UnityEngine;

public enum Camera_Mode { Follow, Split_Screen, Transitioning }

public class Camera_Manager : MonoBehaviour
{
    [Header("Camera References")]
    [SerializeField] private Camera main_camera;
    [SerializeField] private Camera split_camera_1;
    [SerializeField] private Camera split_camera_2;
    [SerializeField] private Transform player_1;
    [SerializeField] private Transform player_2;
    
    [Header("Transition Settings")]
    [SerializeField] private float transition_speed = 2f;
    [SerializeField] private float screen_border_buffer = 0.1f;
    [SerializeField] private bool show_debug_info = false;
    [SerializeField] private bool force_split_screen = false;
    
    private Camera_Mode current_mode = Camera_Mode.Follow;
    private Camera_Follow follow_script;
    private Split_Screen_Controller split_script;
    private bool is_transitioning = false;
    [SerializeField] private bool force_mode = false;
    private Vector3 main_cam_target_pos;
    private Quaternion main_cam_target_rot;
    private float main_cam_target_fov;
    
    void Start()
    {
        follow_script = GetComponent<Camera_Follow>();
        split_script = GetComponent<Split_Screen_Controller>();
        
        follow_script.Initialize(main_camera, player_1, player_2);
        split_script.Initialize(split_camera_1, split_camera_2, player_1, player_2);
        
        if (force_split_screen)
        {
            Force_Split_Screen();
        }
        else
        {
            SetCameraMode(Camera_Mode.Follow);
        }
    }
    
    void Update()
    {
        if (is_transitioning)
        {
            UpdateTransition();
            return;
        }
        
        if (force_mode) return;
        
        bool player_outside_view = IsAnyPlayerOutsideView();
        
        if (player_outside_view && current_mode == Camera_Mode.Follow)
        {
            StartTransitionToSplit();
        }
        else if (!player_outside_view && current_mode == Camera_Mode.Split_Screen)
        {
            StartTransitionToFollow();
        }
        
        if (show_debug_info)
        {
            Debug.Log($"Players Outside View: {player_outside_view} | Mode: {current_mode}");
        }
    }
    
    private bool IsAnyPlayerOutsideView()
    {
        Vector3 p1_viewport = main_camera.WorldToViewportPoint(player_1.position);
        Vector3 p2_viewport = main_camera.WorldToViewportPoint(player_2.position);
        
        bool p1_outside = p1_viewport.x < screen_border_buffer || p1_viewport.x > (1f - screen_border_buffer) ||
                         p1_viewport.y < screen_border_buffer || p1_viewport.y > (1f - screen_border_buffer) ||
                         p1_viewport.z < 0;
        
        bool p2_outside = p2_viewport.x < screen_border_buffer || p2_viewport.x > (1f - screen_border_buffer) ||
                         p2_viewport.y < screen_border_buffer || p2_viewport.y > (1f - screen_border_buffer) ||
                         p2_viewport.z < 0;
        
        return p1_outside || p2_outside;
    }
    
    private void StartTransitionToSplit()
    {
        current_mode = Camera_Mode.Transitioning;
        is_transitioning = true;
        
        Vector3 center = (player_1.position + player_2.position) / 2f;
        main_cam_target_pos = center + new Vector3(-5f, 8f, -6f);
        main_cam_target_rot = Quaternion.LookRotation((center - main_cam_target_pos).normalized);
        main_cam_target_fov = 50f;
        
        follow_script.enabled = false;
    }
    
    private void StartTransitionToFollow()
    {
        current_mode = Camera_Mode.Transitioning;
        is_transitioning = true;
        
        split_camera_1.gameObject.SetActive(false);
        split_camera_2.gameObject.SetActive(false);
        main_camera.gameObject.SetActive(true);
        split_script.enabled = false;
    }
    
    private void UpdateTransition()
    {
        if (current_mode == Camera_Mode.Transitioning)
        {
            if (split_camera_1.gameObject.activeInHierarchy)
            {
                TransitionToSplitScreen();
            }
            else
            {
                TransitionToFollowMode();
            }
        }
    }
    
    private void TransitionToSplitScreen()
    {
        main_camera.transform.position = Vector3.Lerp(main_camera.transform.position, main_cam_target_pos, transition_speed * Time.deltaTime);
        main_camera.transform.rotation = Quaternion.Lerp(main_camera.transform.rotation, main_cam_target_rot, transition_speed * Time.deltaTime);
        main_camera.fieldOfView = Mathf.Lerp(main_camera.fieldOfView, main_cam_target_fov, transition_speed * Time.deltaTime);
        
        float distance_to_target = Vector3.Distance(main_camera.transform.position, main_cam_target_pos);
        float rotation_diff = Quaternion.Angle(main_camera.transform.rotation, main_cam_target_rot);
        
        if (distance_to_target < 0.5f && rotation_diff < 5f)
        {
            main_camera.gameObject.SetActive(false);
            split_camera_1.gameObject.SetActive(true);
            split_camera_2.gameObject.SetActive(true);
            split_script.enabled = true;
            
            current_mode = Camera_Mode.Split_Screen;
            is_transitioning = false;
        }
    }
    
    private void TransitionToFollowMode()
    {
        Vector3 center = (player_1.position + player_2.position) / 2f;
        Vector3 follow_target_pos = center + new Vector3(0, 10, -8);
        Quaternion follow_target_rot = Quaternion.LookRotation((center - follow_target_pos).normalized);
        
        main_camera.transform.position = Vector3.Lerp(main_camera.transform.position, follow_target_pos, transition_speed * Time.deltaTime);
        main_camera.transform.rotation = Quaternion.Lerp(main_camera.transform.rotation, follow_target_rot, transition_speed * Time.deltaTime);
        main_camera.fieldOfView = Mathf.Lerp(main_camera.fieldOfView, 40f, transition_speed * Time.deltaTime);
        
        float distance_to_target = Vector3.Distance(main_camera.transform.position, follow_target_pos);
        float rotation_diff = Quaternion.Angle(main_camera.transform.rotation, follow_target_rot);
        
        if (distance_to_target < 0.5f && rotation_diff < 5f)
        {
            follow_script.enabled = true;
            current_mode = Camera_Mode.Follow;
            is_transitioning = false;
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
        else if (mode == Camera_Mode.Split_Screen)
        {
            main_camera.gameObject.SetActive(false);
            split_camera_1.gameObject.SetActive(true);
            split_camera_2.gameObject.SetActive(true);
            follow_script.enabled = false;
            split_script.enabled = true;
        }
    }
    
    public Camera_Mode Get_Current_Mode()
    {
        return current_mode;
    }
    
    public Camera Get_Split_Camera_1()
    {
        return split_camera_1;
    }
    
    public Camera Get_Split_Camera_2()
    {
        return split_camera_2;
    }
    
    [ContextMenu("Force Split Screen")]
    public void Force_Split_Screen()
    {
        force_mode = true;
        SetCameraMode(Camera_Mode.Split_Screen);
    }
    
    [ContextMenu("Force Follow Mode")]
    public void Force_Follow_Mode()
    {
        force_mode = true;
        SetCameraMode(Camera_Mode.Follow);
    }
    
    [ContextMenu("Enable Auto Switch")]
    public void Enable_Auto_Switch()
    {
        force_mode = false;
    }
    
    public void Set_Split_Distance(float distance)
    {
        
    }
}