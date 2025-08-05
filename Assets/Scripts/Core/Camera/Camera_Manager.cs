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
    
    [Header("Split Screen Settings")]
    [SerializeField] private float split_distance = 6f;
    [SerializeField] private bool force_split_screen = false;
    [SerializeField] private bool enable_split_on_distance = true;
    [SerializeField] private bool force_split_screen_only = false;
    
    private Camera_Mode current_mode = Camera_Mode.Follow;
    private Camera_Follow follow_script;
    private Split_Screen_Controller split_script;
    private bool is_transitioning = false;
    
    void Start()
    {
        follow_script = GetComponent<Camera_Follow>();
        split_script = GetComponent<Split_Screen_Controller>();
        
        follow_script.Initialize(main_camera, player_1, player_2);
        split_script.Initialize(split_camera_1, split_camera_2, player_1, player_2);
        
        follow_script.SetSplitModeEnabled(enable_split_on_distance);
        
        if (force_split_screen_only)
        {
            SetCameraMode(Camera_Mode.Split_Screen);
        }
        else if (enable_split_on_distance)
        {
            SetCameraMode(Camera_Mode.Follow);
        }
        else
        {
            SetCameraMode(Camera_Mode.Follow);
        }
    }
    
    void Update()
    {
        if (is_transitioning || force_split_screen_only) return;
        
        if (enable_split_on_distance)
        {
            float player_distance = Vector3.Distance(player_1.position, player_2.position);
            
            if (player_distance > split_distance && current_mode == Camera_Mode.Follow)
            {
                StartTransitionToSplit();
            }
            else if (player_distance < split_distance * 0.8f && current_mode == Camera_Mode.Split_Screen)
            {
                StartTransitionToFollow();
            }
        }
    }
    
    private void UpdateTransition()
    {
        
    }
    
    private void StartTransitionToSplit()
    {
        current_mode = Camera_Mode.Split_Screen;
        main_camera.gameObject.SetActive(false);
        split_camera_1.gameObject.SetActive(true);
        split_camera_2.gameObject.SetActive(true);
        follow_script.enabled = false;
        split_script.enabled = true;
    }
    
    private void StartTransitionToFollow()
    {
        current_mode = Camera_Mode.Follow;
        split_camera_1.gameObject.SetActive(false);
        split_camera_2.gameObject.SetActive(false);
        main_camera.gameObject.SetActive(true);
        split_script.enabled = false;
        follow_script.enabled = true;
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
        SetCameraMode(Camera_Mode.Split_Screen);
    }
    
    [ContextMenu("Force Follow Mode")]
    public void Force_Follow_Mode()
    {
        SetCameraMode(Camera_Mode.Follow);
    }
    
    [ContextMenu("Enable Auto Switch")]
    public void Enable_Auto_Switch()
    {
        force_split_screen_only = false;
        enable_split_on_distance = true;
    }
    
    [ContextMenu("Toggle Split Screen Only")]
    public void Toggle_Split_Screen_Only()
    {
        force_split_screen_only = !force_split_screen_only;
        
        if (force_split_screen_only)
        {
            SetCameraMode(Camera_Mode.Split_Screen);
        }
        else
        {
            enable_split_on_distance = true;
        }
    }
    
    public void Set_Force_Split_Screen_Only(bool enabled)
    {
        force_split_screen_only = enabled;
        
        if (enabled)
        {
            SetCameraMode(Camera_Mode.Split_Screen);
        }
    }
    
    public bool Is_Force_Split_Screen_Only()
    {
        return force_split_screen_only;
    }
    
    public void Set_Split_Distance(float distance)
    {
        split_distance = distance;
    }
}