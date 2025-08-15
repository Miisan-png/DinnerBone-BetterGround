using UnityEngine;

public static class CameraUtils
{
    /// <summary>
    /// Checks if a world position is visible by the given camera.
    /// </summary>
    public static bool IsPointVisible(Camera cam, Vector3 worldPos)
    {
        Vector3 viewportPoint = cam.WorldToViewportPoint(worldPos);

        // Check if in front of camera
        if (viewportPoint.z < 0)
            return false;

        // Check if inside camera's viewport
        return viewportPoint.x >= 0 && viewportPoint.x <= 1 &&
               viewportPoint.y >= 0 && viewportPoint.y <= 1;
    }
}
