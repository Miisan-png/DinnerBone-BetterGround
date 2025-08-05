using UnityEngine;

[System.Serializable]
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
        Vector3 finalPosition = basePosition + localOffsetWorld + worldOffset + Vector3.up * heightOffset;
        
        Debug.Log($"Helper calculating position: Base={basePosition}, Local={localOffsetWorld}, World={worldOffset}, Height={heightOffset}, Final={finalPosition}");
        
        return finalPosition;
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
        Gizmos.DrawWireCube(interactionPos + Vector3.up * 0.5f, Vector3.one * 0.2f);
    }
}