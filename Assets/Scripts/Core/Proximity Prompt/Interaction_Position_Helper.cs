using UnityEngine;

public class Interaction_Position_Helper : MonoBehaviour
{
    [Header("Position Offset")]
    [SerializeField] private Vector3 localOffset = Vector3.zero;
    [SerializeField] private Vector3 worldOffset = Vector3.zero;
    [SerializeField] private float heightOffset = 2f;
    
    [Header("Debug Visualization")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color gizmoColor = Color.yellow;
    [SerializeField] private float gizmoSize = 0.5f;
    [SerializeField] private bool showLine = true;
    
    public Vector3 GetInteractionPosition()
    {
        Vector3 basePosition = transform.position;
        Vector3 localOffsetWorld = transform.TransformDirection(localOffset);
        return basePosition + localOffsetWorld + worldOffset + Vector3.up * heightOffset;
    }
    
    void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        Vector3 interactionPos = GetInteractionPosition();
        
        Gizmos.color = gizmoColor;
        
        Gizmos.DrawWireSphere(interactionPos, gizmoSize);
        
        Gizmos.DrawSphere(interactionPos, gizmoSize * 0.3f);
        
        if (showLine)
        {
            Gizmos.color = gizmoColor * 0.7f;
            Gizmos.DrawLine(transform.position, interactionPos);
        }
        
        Gizmos.color = Color.red;
        Vector3 labelPos = interactionPos + Vector3.up * (gizmoSize + 0.2f);
        
        #if UNITY_EDITOR
        UnityEditor.Handles.color = gizmoColor;
        UnityEditor.Handles.Label(labelPos, "Interaction Point");
        #endif
    }
    
    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;
        
        Vector3 interactionPos = GetInteractionPosition();
        
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(interactionPos, Vector3.one * 0.1f);
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + transform.TransformDirection(localOffset));
        
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position + transform.TransformDirection(localOffset), 
                       transform.position + transform.TransformDirection(localOffset) + worldOffset);
        
        Gizmos.color = Color.green;
        Vector3 beforeHeight = transform.position + transform.TransformDirection(localOffset) + worldOffset;
        Gizmos.DrawLine(beforeHeight, beforeHeight + Vector3.up * heightOffset);
    }
}