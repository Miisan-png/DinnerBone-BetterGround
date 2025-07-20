using UnityEngine;

public class Split_Screen_Controller : MonoBehaviour
{
    [SerializeField] private float follow_speed = 8f;
    [SerializeField] private float camera_size = 6f;
    [SerializeField] private Vector3 offset = new Vector3(0, 8, -6);
    
    private Camera camera_1;
    private Camera camera_2;
    private Transform player_1;
    private Transform player_2;
    
    public void Initialize(Camera cam1, Camera cam2, Transform p1, Transform p2)
    {
        camera_1 = cam1;
        camera_2 = cam2;
        player_1 = p1;
        player_2 = p2;
        
        SetupSplitScreen();
    }
    
    void LateUpdate()
    {
        if (player_1 != null)
        {
            Vector3 target_pos_1 = player_1.position + offset;
            camera_1.transform.position = Vector3.Lerp(camera_1.transform.position, target_pos_1, follow_speed * Time.deltaTime);
        }
        
        if (player_2 != null)
        {
            Vector3 target_pos_2 = player_2.position + offset;
            camera_2.transform.position = Vector3.Lerp(camera_2.transform.position, target_pos_2, follow_speed * Time.deltaTime);
        }
    }
    
    private void SetupSplitScreen()
    {
        camera_1.rect = new Rect(0, 0, 0.5f, 1);
        camera_2.rect = new Rect(0.5f, 0, 0.5f, 1);
        
        camera_1.orthographicSize = camera_size;
        camera_2.orthographicSize = camera_size;
    }
}