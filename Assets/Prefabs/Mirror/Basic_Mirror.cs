using UnityEngine;
public class MirrorProjection : MonoBehaviour
{
    private Camera cam;
    
    void Start()
    {
        cam = GetComponent<Camera>();
    }
    
    void Update()
    {
        Matrix4x4 projMatrix = cam.projectionMatrix;
        projMatrix.m00 *= -1; 
        cam.projectionMatrix = projMatrix;
        
        GL.invertCulling = true;
    }
    
    void OnDisable()
    {
        GL.invertCulling = false;
    }
}