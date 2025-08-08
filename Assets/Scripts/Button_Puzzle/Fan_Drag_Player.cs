using UnityEngine;

public class Fan_Drag_Player : MonoBehaviour
{
    [Header("Drag Settings")]
    public float base_drag_force = 5f; // Base drag force at full fan speed
    public float min_drag_multiplier = 0.1f; // Minimum drag when fan is slowest (10% of base)
    
    [Header("References")]
    [SerializeField] private Transform fan_object;
    [SerializeField] private Fan_Rotate_Logic fan_logic; // Reference to the fan's logic
    [SerializeField] private LayerMask player_layer = -1;
    
    [Header("Debug")]
    [SerializeField] private bool show_debug_info = false;
    
    private float current_drag_force;
    
    void Start()
    {
        Collider trigger = GetComponent<Collider>();
        if (trigger == null)
        {
            trigger = gameObject.AddComponent<BoxCollider>();
        }
        trigger.isTrigger = true;
        
        // Try to find fan logic if not assigned
        if (fan_logic == null && fan_object != null)
        {
            fan_logic = fan_object.GetComponent<Fan_Rotate_Logic>();
        }
        
        current_drag_force = base_drag_force;
    }
    
    void Update()
    {
        UpdateDragForce();
    }
    
    private void UpdateDragForce()
    {
        if (fan_logic != null)
        {
            // Get the fan's current speed percentage (0.0 to 1.0)
            float speed_percentage = fan_logic.GetSpeedPercentage();
            
            // Calculate drag force based on fan speed
            // At full speed = base_drag_force
            // At minimum speed = base_drag_force * min_drag_multiplier
            float drag_multiplier = Mathf.Lerp(min_drag_multiplier, 1f, speed_percentage);
            current_drag_force = base_drag_force * drag_multiplier;
            
            if (show_debug_info)
            {
                Debug.Log($"Fan Speed: {speed_percentage:F2} | Drag Force: {current_drag_force:F2}");
            }
        }
        else
        {
            // Fallback to base drag if no fan logic
            current_drag_force = base_drag_force;
        }
    }
    
    void OnTriggerStay(Collider other)
    {
        if (IsInLayerMask(other.gameObject, player_layer))
        {
            Player_Controller player = other.GetComponent<Player_Controller>();
            if (player != null && fan_object != null)
            {
                Vector3 player_pos = player.transform.position;
                Vector3 fan_pos = fan_object.position;
                
                Vector3 drag_direction = (fan_pos - player_pos).normalized;
                
                CharacterController cc = player.GetComponent<CharacterController>();
                if (cc != null)
                {
                    cc.Move(drag_direction * current_drag_force * Time.deltaTime);
                }
                else
                {
                    player.transform.position += drag_direction * current_drag_force * Time.deltaTime;
                }
            }
        }
    }
    
    private bool IsInLayerMask(GameObject obj, LayerMask layerMask)
    {
        return (layerMask.value & (1 << obj.layer)) != 0;
    }
    
    // Public method to manually set drag force (useful for testing)
    public void SetDragForce(float force)
    {
        base_drag_force = force;
    }
    
    // Public getter for current drag force
    public float GetCurrentDragForce()
    {
        return current_drag_force;
    }
    
    void OnDrawGizmos()
    {
        if (fan_object != null)
        {
            // Draw drag direction from trigger to fan
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, fan_object.position);
            
            // Draw drag force visualization
            if (Application.isPlaying)
            {
                float intensity = current_drag_force / base_drag_force;
                Gizmos.color = Color.Lerp(Color.green, Color.red, intensity);
                Gizmos.DrawWireSphere(transform.position, intensity * 2f);
                
                // Draw current drag force as text (if you have a way to display it)
                if (show_debug_info)
                {
                    Gizmos.color = Color.white;
                    // You could add text rendering here if you have a text mesh or UI component
                }
            }
            else
            {
                // Show base drag area in editor
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, 2f);
            }
        }
        
        // Draw trigger area
        Collider trigger = GetComponent<Collider>();
        if (trigger != null)
        {
            Gizmos.color = new Color(0, 1, 1, 0.3f);
            Gizmos.matrix = transform.localToWorldMatrix;
            
            if (trigger is BoxCollider box)
            {
                Gizmos.DrawCube(box.center, box.size);
            }
            else if (trigger is SphereCollider sphere)
            {
                Gizmos.DrawSphere(sphere.center, sphere.radius);
            }
        }
    }
}