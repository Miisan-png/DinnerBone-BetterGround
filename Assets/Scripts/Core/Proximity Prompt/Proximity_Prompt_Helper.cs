using UnityEngine;

public class Proximity_Prompt_Helper : MonoBehaviour
{
    [Header("Position Settings")]
    [SerializeField] private Vector3 localOffset = Vector3.zero;
    [SerializeField] private Vector3 worldOffset = Vector3.zero;
    [SerializeField] private float heightOffset = 2f;
    
    [Header("Debug")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color gizmoColor = Color.cyan;
    [SerializeField] private float gizmoSize = 0.3f;
    
    public Vector3 GetPromptPosition()
    {
        Vector3 basePosition = transform.position;
        Vector3 localOffsetWorld = transform.TransformDirection(localOffset);
        return basePosition + localOffsetWorld + worldOffset + Vector3.up * heightOffset;
    }
    
    void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        Vector3 promptPos = GetPromptPosition();
        
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(promptPos, gizmoSize);
        Gizmos.DrawLine(transform.position, promptPos);
    }
}