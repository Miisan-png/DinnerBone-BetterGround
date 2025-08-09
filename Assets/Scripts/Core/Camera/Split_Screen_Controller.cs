using UnityEngine;

public class Split_Screen_Controller : MonoBehaviour
{
    [SerializeField] private float follow_speed = 8f;
    
    private Camera camera_1;
    private Camera camera_2;
    private Transform player_1;
    private Transform player_2;
    private Vector3 base_offset;
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
        
        Vector3 center = (player_1.position + player_2.position) / 2f;
        base_offset = main_camera_ref.transform.position - center;
        base_rotation = main_camera_ref.transform.rotation;
        
        SetupSplitScreen();
    }
    
    void LateUpdate()
    {
        if (player_1 != null)
        {
            Vector3 target_pos_1 = player_1.position + base_offset;
            camera_1.transform.position = Vector3.Lerp(camera_1.transform.position, target_pos_1, follow_speed * Time.deltaTime);
            camera_1.transform.rotation = base_rotation;
        }
        
        if (player_2 != null)
        {
            Vector3 target_pos_2 = player_2.position + base_offset;
            camera_2.transform.position = Vector3.Lerp(camera_2.transform.position, target_pos_2, follow_speed * Time.deltaTime);
            camera_2.transform.rotation = base_rotation;
        }
    }
    
    private void SetupSplitScreen()
    {
        camera_1.rect = new Rect(0, 0, 0.5f, 1);
        camera_2.rect = new Rect(0.5f, 0, 0.5f, 1);
        camera_1.orthographic = false;
        camera_2.orthographic = false;
        
        if (main_camera_ref != null)
        {
            camera_1.fieldOfView = main_camera_ref.fieldOfView;
            camera_2.fieldOfView = main_camera_ref.fieldOfView;
        }
        else
        {
            camera_1.fieldOfView = 60f;
            camera_2.fieldOfView = 60f;
        }
    }
}