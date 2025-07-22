using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace StylizedPointLight
{
    [RequireComponent (typeof (Camera))]
    [ExecuteInEditMode]
    public class BuiltIn_CameraDepthNormal : MonoBehaviour
    {
        Camera cam;
        private void Start()
        {
            //get the camera and tell it to render a depthnormals texture
            cam = GetComponent<Camera> ();
            cam.depthTextureMode |= DepthTextureMode.Depth;
            cam.depthTextureMode = cam.depthTextureMode | DepthTextureMode.DepthNormals;
        }
    }
}
