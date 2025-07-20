using UnityEngine;
/// ---------- Keeps cameras within defined level boundaries ----------

public class Camera_Bounds : MonoBehaviour
{
    [SerializeField] private Transform min_bounds;
    [SerializeField] private Transform max_bounds;
    [SerializeField] private bool use_bounds = true;
    
    private Camera target_camera;
    
    void Start()
    {
        target_camera = GetComponent<Camera>();
    }
    
    void LateUpdate()
    {
        if (!use_bounds || min_bounds == null || max_bounds == null) return;
        
        ClampCameraPosition();
    }
    
    private void ClampCameraPosition()
    {
        float camera_height = target_camera.orthographicSize;
        float camera_width = camera_height * target_camera.aspect;
        
        float min_x = min_bounds.position.x + camera_width;
        float max_x = max_bounds.position.x - camera_width;
        float min_z = min_bounds.position.z + camera_height;
        float max_z = max_bounds.position.z - camera_height;
        
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, min_x, max_x);
        pos.z = Mathf.Clamp(pos.z, min_z, max_z);
        transform.position = pos;
    }
    
    public void SetBounds(Transform min, Transform max)
    {
        min_bounds = min;
        max_bounds = max;
    }
    
    void OnDrawGizmos()
    {
        if (min_bounds == null || max_bounds == null) return;
        
        Gizmos.color = Color.red;
        Vector3 size = max_bounds.position - min_bounds.position;
        Vector3 center = (min_bounds.position + max_bounds.position) / 2f;
        Gizmos.DrawWireCube(center, size);
    }
}