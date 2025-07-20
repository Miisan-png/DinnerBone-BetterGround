using UnityEngine;
using DG.Tweening;

public enum Pickable_Type { Basic, Tool, Key, Throwable }

public class Pickable_Object : MonoBehaviour, I_Interactable
{
    [Header("Object Settings")]
    [SerializeField] private Pickable_Type object_type = Pickable_Type.Basic;
    [SerializeField] private string object_name = "Object";
    [SerializeField] private float throw_force = 10f;
    [SerializeField] private bool can_be_thrown = true;
    
    [Header("Pickup Animation")]
    [SerializeField] private float pickup_duration = 0.6f;
    [SerializeField] private float scale_down_amount = 0.8f;
    
    [Header("Debug")]
    [SerializeField] private bool show_debug = true;
    
    private Rigidbody rb;
    private Collider col;
    private bool is_held = false;
    private Transform hold_point;
    private Player_Controller holder;
    private Vector3 original_scale;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        original_scale = transform.localScale;
        
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
    }
    
    void Update()
    {
        // Force object to stay at hold point if held
        if (is_held && hold_point != null)
        {
            transform.position = hold_point.position;
            transform.rotation = hold_point.rotation;
        }
    }
    
    public bool Can_Interact(Player_Type player_type)
    {
        return !is_held;
    }
    
    public void Start_Interaction(Player_Controller player)
    {
        if (player.Get_Held_Object() != null || is_held)
        {
            Debug.Log("Player already holding something or object already held");
            return;
        }
        
        Debug.Log($"Starting pickup of {object_name}");
        
        holder = player;
        hold_point = player.Get_Hold_Point();
        is_held = true;
        
        // Disable physics COMPLETELY
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        
        if (col != null)
        {
            col.isTrigger = true;
        }
        
        // Simple smooth move to hold point
        transform.DOMove(hold_point.position, pickup_duration).SetEase(Ease.OutBack);
        transform.DOScale(original_scale * scale_down_amount, pickup_duration).SetEase(Ease.OutBack);
        
        player.Set_Held_Object(this);
        
        Debug.Log($"Pickup complete for {object_name}");
    }
    
    public void End_Interaction(Player_Controller player)
    {
        
    }
    
    public string Get_Interaction_Text()
    {
        return $"Pick up {object_name}";
    }
    
    public Vector3 Get_Interaction_Position()
    {
        return transform.position;
    }
    
    public void Drop_Object(Vector3 drop_position, bool throw_object = false)
    {
        Debug.Log($"Dropping {object_name}");
        
        is_held = false;
        
        transform.DOKill();
        
        transform.position = drop_position;
        transform.DOScale(original_scale, 0.3f);
        
        // Re-enable physics
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            
            if (throw_object && can_be_thrown)
            {
                Vector3 throw_direction = holder.transform.forward + Vector3.up * 0.3f;
                rb.AddForce(throw_direction * throw_force, ForceMode.Impulse);
            }
        }
        
        if (col != null)
        {
            col.isTrigger = false;
        }
        
        if (holder != null)
        {
            holder.Set_Held_Object(null);
            holder = null;
        }
        
        hold_point = null;
    }
    
    public Pickable_Type Get_Object_Type()
    {
        return object_type;
    }
    
    public string Get_Object_Name()
    {
        return object_name;
    }
    
    void OnDrawGizmos()
    {
        if (!show_debug) return;
        
        Gizmos.color = is_held ? Color.green : Color.cyan;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
        
        if (is_held && hold_point != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, hold_point.position);
        }
        
        Color type_color = object_type == Pickable_Type.Key ? Color.yellow :
                          object_type == Pickable_Type.Tool ? Color.blue :
                          object_type == Pickable_Type.Throwable ? Color.red : Color.white;
        Gizmos.color = type_color;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 1.5f, 0.3f);
    }
}