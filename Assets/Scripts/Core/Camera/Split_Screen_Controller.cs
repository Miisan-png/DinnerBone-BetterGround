using UnityEngine;

public class Split_Screen_Controller : MonoBehaviour
{
    [Header("Camera Movement")]
    [SerializeField] private float follow_speed = 8f;
    
    [Header("Camera 1 Offset")]
    [SerializeField] private Vector3 camera_1_offset = Vector3.zero;
    [SerializeField] private Vector3 camera_1_rotation_offset = Vector3.zero;
    
    [Header("Camera 2 Offset")]
    [SerializeField] private Vector3 camera_2_offset = Vector3.zero;
    [SerializeField] private Vector3 camera_2_rotation_offset = Vector3.zero;
    
    [Header("Shared Offset (Applied to Both)")]
    [SerializeField] private Vector3 shared_base_offset = Vector3.zero;
    [SerializeField] private Vector3 shared_rotation_offset = Vector3.zero;
    
    private Camera camera_1;
    private Camera camera_2;
    private Transform player_1;
    private Transform player_2;
    private Quaternion base_rotation;
    private Camera main_camera_ref;
    
    public void Initialize(Camera cam1, Camera cam2, Transform p1, Transform p2)
    {
        camera_1 = cam1;
        camera_2 = cam2;
        player_1 = p1;
        player_2 = p2;
        
        main_camera_ref = Camera.main;
        if (main_camera_ref == null)
        {
            main_camera_ref = FindObjectOfType<Camera>();
        }
        
        // Use the shared base offset if individual offsets aren't set
        if (camera_1_offset == Vector3.zero && camera_2_offset == Vector3.zero && main_camera_ref != null)
        {
            Vector3 center = (player_1.position + player_2.position) / 2f;
            shared_base_offset = main_camera_ref.transform.position - center;
        }
        
        base_rotation = main_camera_ref != null ? main_camera_ref.transform.rotation : Quaternion.identity;
        
        SetupSplitScreen();
    }
    
    void LateUpdate()
    {
        if (player_1 != null && camera_1 != null)
        {
            // Calculate final offset for camera 1 (shared + individual)
            Vector3 final_offset_1 = shared_base_offset + camera_1_offset;
            Vector3 target_pos_1 = player_1.position + final_offset_1;
            
            camera_1.transform.position = Vector3.Lerp(
                camera_1.transform.position, 
                target_pos_1, 
                follow_speed * Time.deltaTime
            );
            
            // Calculate final rotation for camera 1 (base + shared + individual)
            Vector3 final_rotation_1 = base_rotation.eulerAngles + shared_rotation_offset + camera_1_rotation_offset;
            Quaternion target_rotation_1 = Quaternion.Euler(final_rotation_1);
            
            camera_1.transform.rotation = Quaternion.Lerp(
                camera_1.transform.rotation,
                target_rotation_1,
                follow_speed * Time.deltaTime
            );
        }
        
        if (player_2 != null && camera_2 != null)
        {
            // Calculate final offset for camera 2 (shared + individual)
            Vector3 final_offset_2 = shared_base_offset + camera_2_offset;
            Vector3 target_pos_2 = player_2.position + final_offset_2;
            
            camera_2.transform.position = Vector3.Lerp(
                camera_2.transform.position, 
                target_pos_2, 
                follow_speed * Time.deltaTime
            );
            
            // Calculate final rotation for camera 2 (base + shared + individual)
            Vector3 final_rotation_2 = base_rotation.eulerAngles + shared_rotation_offset + camera_2_rotation_offset;
            Quaternion target_rotation_2 = Quaternion.Euler(final_rotation_2);
            
            camera_2.transform.rotation = Quaternion.Lerp(
                camera_2.transform.rotation,
                target_rotation_2,
                follow_speed * Time.deltaTime
            );
        }
    }
    
    private void SetupSplitScreen()
    {
        if (camera_1 != null)
        {
            camera_1.rect = new Rect(0, 0, 0.5f, 1);
            camera_1.orthographic = false;
        }
        
        if (camera_2 != null)
        {
            camera_2.rect = new Rect(0.5f, 0, 0.5f, 1);
            camera_2.orthographic = false;
        }
        
        if (main_camera_ref != null)
        {
            if (camera_1 != null) camera_1.fieldOfView = main_camera_ref.fieldOfView;
            if (camera_2 != null) camera_2.fieldOfView = main_camera_ref.fieldOfView;
        }
        else
        {
            if (camera_1 != null) camera_1.fieldOfView = 60f;
            if (camera_2 != null) camera_2.fieldOfView = 60f;
        }
    }
    
    // Public methods to modify offsets at runtime
    public void SetCamera1Offset(Vector3 offset)
    {
        camera_1_offset = offset;
    }
    
    public void SetCamera2Offset(Vector3 offset)
    {
        camera_2_offset = offset;
    }
    
    public void SetCamera1RotationOffset(Vector3 rotationOffset)
    {
        camera_1_rotation_offset = rotationOffset;
    }
    
    public void SetCamera2RotationOffset(Vector3 rotationOffset)
    {
        camera_2_rotation_offset = rotationOffset;
    }
    
    public void SetSharedOffset(Vector3 offset)
    {
        shared_base_offset = offset;
    }
    
    public void SetSharedRotationOffset(Vector3 rotationOffset)
    {
        shared_rotation_offset = rotationOffset;
    }
    
    public void SetCameraOffsets(Vector3 cam1Offset, Vector3 cam2Offset)
    {
        camera_1_offset = cam1Offset;
        camera_2_offset = cam2Offset;
    }
    
    public void SetCameraRotationOffsets(Vector3 cam1RotationOffset, Vector3 cam2RotationOffset)
    {
        camera_1_rotation_offset = cam1RotationOffset;
        camera_2_rotation_offset = cam2RotationOffset;
    }
    
    public void SetCameraOffsetsAndRotations(Vector3 cam1Offset, Vector3 cam1Rotation, Vector3 cam2Offset, Vector3 cam2Rotation)
    {
        camera_1_offset = cam1Offset;
        camera_1_rotation_offset = cam1Rotation;
        camera_2_offset = cam2Offset;
        camera_2_rotation_offset = cam2Rotation;
    }
    
    // Public getters for current offsets
    public Vector3 GetCamera1Offset()
    {
        return camera_1_offset;
    }
    
    public Vector3 GetCamera2Offset()
    {
        return camera_2_offset;
    }
    
    public Vector3 GetCamera1RotationOffset()
    {
        return camera_1_rotation_offset;
    }
    
    public Vector3 GetCamera2RotationOffset()
    {
        return camera_2_rotation_offset;
    }
    
    public Vector3 GetSharedOffset()
    {
        return shared_base_offset;
    }
    
    public Vector3 GetSharedRotationOffset()
    {
        return shared_rotation_offset;
    }
    
    public Vector3 GetFinalCamera1Offset()
    {
        return shared_base_offset + camera_1_offset;
    }
    
    public Vector3 GetFinalCamera2Offset()
    {
        return shared_base_offset + camera_2_offset;
    }
    
    public Vector3 GetFinalCamera1Rotation()
    {
        return base_rotation.eulerAngles + shared_rotation_offset + camera_1_rotation_offset;
    }
    
    public Vector3 GetFinalCamera2Rotation()
    {
        return base_rotation.eulerAngles + shared_rotation_offset + camera_2_rotation_offset;
    }
    
    // Context menu methods for easy testing in the editor
    [ContextMenu("Reset All Offsets")]
    public void ResetAllOffsets()
    {
        camera_1_offset = Vector3.zero;
        camera_2_offset = Vector3.zero;
        camera_1_rotation_offset = Vector3.zero;
        camera_2_rotation_offset = Vector3.zero;
        shared_base_offset = Vector3.zero;
        shared_rotation_offset = Vector3.zero;
    }
    
    [ContextMenu("Set Default Vertical Split Offsets")]
    public void SetDefaultVerticalSplitOffsets()
    {
        camera_1_offset = new Vector3(-2f, 0f, 0f);
        camera_2_offset = new Vector3(2f, 0f, 0f);
        camera_1_rotation_offset = new Vector3(0f, -10f, 0f);
        camera_2_rotation_offset = new Vector3(0f, 10f, 0f);
    }
    
    [ContextMenu("Set Default Horizontal Split Offsets")]
    public void SetDefaultHorizontalSplitOffsets()
    {
        camera_1_offset = new Vector3(0f, 1f, 0f);
        camera_2_offset = new Vector3(0f, -1f, 0f);
        camera_1_rotation_offset = new Vector3(5f, 0f, 0f);
        camera_2_rotation_offset = new Vector3(-5f, 0f, 0f);
    }
    
    [ContextMenu("Set Over-Shoulder Views")]
    public void SetOverShoulderViews()
    {
        camera_1_offset = new Vector3(-1.5f, 1.5f, -2f);
        camera_2_offset = new Vector3(1.5f, 1.5f, -2f);
        camera_1_rotation_offset = new Vector3(10f, 15f, 0f);
        camera_2_rotation_offset = new Vector3(10f, -15f, 0f);
    }
    
    [ContextMenu("Set Top-Down Views")]
    public void SetTopDownViews()
    {
        camera_1_offset = new Vector3(-3f, 8f, 0f);
        camera_2_offset = new Vector3(3f, 8f, 0f);
        camera_1_rotation_offset = new Vector3(90f, 0f, 0f);
        camera_2_rotation_offset = new Vector3(90f, 0f, 0f);
    }
}