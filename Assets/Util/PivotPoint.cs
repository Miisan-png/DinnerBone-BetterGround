using UnityEngine;
using UnityEditor;

[System.Serializable]
public class PivotPoint : MonoBehaviour
{
   [SerializeField] private Vector3 customPivot = Vector3.zero;
   [SerializeField] private bool showGizmo = true;
   [SerializeField] private Color gizmoColor = Color.red;
   [SerializeField] private float gizmoSize = 0.5f;
   
   public Vector3 GetPivotWorldPosition()
   {
       return transform.TransformPoint(customPivot);
   }
   
   public Vector3 GetPivotLocalPosition()
   {
       return customPivot;
   }
   
   public void SetPivotLocalPosition(Vector3 position)
   {
       customPivot = position;
   }
   
   public void RotateAroundPivot(Vector3 axis, float angle)
   {
       transform.RotateAround(GetPivotWorldPosition(), axis, angle);
   }
   
   public void ScaleAroundPivot(Vector3 scale)
   {
       Vector3 pivotWorld = GetPivotWorldPosition();
       Vector3 direction = transform.position - pivotWorld;
       transform.position = pivotWorld + Vector3.Scale(direction, scale);
       transform.localScale = Vector3.Scale(transform.localScale, scale);
   }
   
   void OnDrawGizmos()
   {
       if (!showGizmo) return;
       
       Gizmos.color = gizmoColor;
       Vector3 worldPivot = GetPivotWorldPosition();
       Gizmos.DrawWireSphere(worldPivot, gizmoSize);
       Gizmos.DrawLine(transform.position, worldPivot);
       
       Gizmos.color = Color.white;
       Gizmos.DrawLine(worldPivot, worldPivot + transform.right * gizmoSize);
       Gizmos.color = Color.green;
       Gizmos.DrawLine(worldPivot, worldPivot + transform.up * gizmoSize);
       Gizmos.color = Color.blue;
       Gizmos.DrawLine(worldPivot, worldPivot + transform.forward * gizmoSize);
   }
}

#if UNITY_EDITOR
[CustomEditor(typeof(PivotPoint))]
public class PivotPointEditor : Editor
{
   private PivotPoint pivotPoint;
   private bool isDragging = false;
   
   void OnEnable()
   {
       pivotPoint = (PivotPoint)target;
   }
   
   public override void OnInspectorGUI()
   {
       DrawDefaultInspector();
       
       GUILayout.Space(10);
       
       if (GUILayout.Button("Reset Pivot to Center"))
       {
           Undo.RecordObject(pivotPoint, "Reset Pivot");
           pivotPoint.SetPivotLocalPosition(Vector3.zero);
       }
       
       if (GUILayout.Button("Set Pivot to Bounds Bottom"))
       {
           Undo.RecordObject(pivotPoint, "Set Pivot to Bottom");
           Renderer renderer = pivotPoint.GetComponent<Renderer>();
           if (renderer != null)
           {
               Vector3 bottom = renderer.bounds.min;
               bottom = pivotPoint.transform.InverseTransformPoint(bottom);
               bottom.y = renderer.bounds.min.y - pivotPoint.transform.position.y;
               pivotPoint.SetPivotLocalPosition(bottom);
           }
       }
       
       if (GUILayout.Button("Set Pivot to Bounds Top"))
       {
           Undo.RecordObject(pivotPoint, "Set Pivot to Top");
           Renderer renderer = pivotPoint.GetComponent<Renderer>();
           if (renderer != null)
           {
               Vector3 top = renderer.bounds.max;
               top = pivotPoint.transform.InverseTransformPoint(top);
               top.y = renderer.bounds.max.y - pivotPoint.transform.position.y;
               pivotPoint.SetPivotLocalPosition(top);
           }
       }
       
       GUILayout.Space(5);
       GUILayout.Label("Animation Helpers:", EditorStyles.boldLabel);
       
       if (GUILayout.Button("Rotate 90° around Y"))
       {
           Undo.RecordObject(pivotPoint.transform, "Rotate around pivot");
           pivotPoint.RotateAroundPivot(Vector3.up, 90f);
       }
       
       if (GUILayout.Button("Rotate -90° around Y"))
       {
           Undo.RecordObject(pivotPoint.transform, "Rotate around pivot");
           pivotPoint.RotateAroundPivot(Vector3.up, -90f);
       }
   }
   
   void OnSceneGUI()
   {
       pivotPoint = (PivotPoint)target;
       
       Vector3 worldPivot = pivotPoint.GetPivotWorldPosition();
       
       EditorGUI.BeginChangeCheck();
       Vector3 newWorldPivot = Handles.PositionHandle(worldPivot, pivotPoint.transform.rotation);
       
       if (EditorGUI.EndChangeCheck())
       {
           Undo.RecordObject(pivotPoint, "Move Pivot Point");
           Vector3 newLocalPivot = pivotPoint.transform.InverseTransformPoint(newWorldPivot);
           pivotPoint.SetPivotLocalPosition(newLocalPivot);
       }
       
       Handles.color = Color.yellow;
       Handles.DrawWireDisc(worldPivot, pivotPoint.transform.up, 1f);
       Handles.DrawWireDisc(worldPivot, pivotPoint.transform.right, 1f);
       Handles.DrawWireDisc(worldPivot, pivotPoint.transform.forward, 1f);
       
       Handles.color = Color.white;
       Handles.DrawLine(pivotPoint.transform.position, worldPivot);
       
       Handles.Label(worldPivot + Vector3.up * 0.5f, "PIVOT", EditorStyles.boldLabel);
   }
}
#endif