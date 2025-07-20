/// ---------- Advanced pushable object with grab points and smooth rotation ----------

using UnityEngine;

public enum Push_Object_Type { Light, Heavy, Slippery }
public enum Allowed_Players { Anyone, Luthe_Only, Cherie_Only }

public class Push_Object : MonoBehaviour, I_Interactable
{
    [Header("Object Settings")]
    [SerializeField] private Push_Object_Type object_type = Push_Object_Type.Light;
    [SerializeField] private Allowed_Players allowed_players = Allowed_Players.Anyone;
    [SerializeField] private float push_force = 5f;
    [SerializeField] private float rotation_force = 15f;
    [SerializeField] private float friction_multiplier = 1f;
    
    [Header("Grab Points")]
    [SerializeField] private Transform[] grab_points;
    [SerializeField] private float snap_speed = 8f;
    
    [Header("Debug")]
    [SerializeField] private bool show_debug = true;
    
    private Rigidbody rb;
    private Player_Controller current_player;
    private Transform current_grab_point;
    private bool is_being_pushed = false;
    private bool player_snapped = false;
    private Vector3 push_direction;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        rb.mass = object_type == Push_Object_Type.Heavy ? 10f : 1f;
        rb.linearDamping = object_type == Push_Object_Type.Slippery ? 0.5f : 2f * friction_multiplier;
        
        Create_Default_Grab_Points();
    }
    
    void Update()
    {
        if (is_being_pushed && current_player != null)
        {
            if (!player_snapped)
            {
                Snap_Player_To_Grab_Point();
            }
            else
            {
                Handle_Push_Input();
                Update_Player_Position();
            }
        }
    }
    
    public bool Can_Interact(Player_Type player_type)
    {
        switch (allowed_players)
        {
            case Allowed_Players.Luthe_Only:
                return player_type == Player_Type.Luthe;
            case Allowed_Players.Cherie_Only:
                return player_type == Player_Type.Cherie;
            case Allowed_Players.Anyone:
                return true;
            default:
                return false;
        }
    }
    
    public void Start_Interaction(Player_Controller player)
    {
        current_player = player;
        is_being_pushed = true;
        player_snapped = false;
        
        current_grab_point = Find_Closest_Grab_Point(player.transform.position);
        
        current_player.Set_Push_Mode(true);
        
        CharacterController playerCC = current_player.GetComponent<CharacterController>();
        if (playerCC != null)
        {
            playerCC.enabled = false;
        }
        
        if (rb != null)
        {
            rb.isKinematic = false;
        }
    }
    
    public void End_Interaction(Player_Controller player)
    {
        if (current_player != null)
        {
            current_player.Set_Push_Mode(false);
            
            CharacterController playerCC = current_player.GetComponent<CharacterController>();
            if (playerCC != null)
            {
                playerCC.enabled = true;
            }
        }
        
        current_player = null;
        current_grab_point = null;
        is_being_pushed = false;
        player_snapped = false;
        push_direction = Vector3.zero;
    }
    
    public string Get_Interaction_Text()
    {
        string type_text = object_type.ToString().ToLower();
        return $"Hold to push {type_text} object";
    }
    
    public Vector3 Get_Interaction_Position()
    {
        return transform.position;
    }
    
    private void Create_Default_Grab_Points()
    {
        if (grab_points == null || grab_points.Length == 0)
        {
            GameObject grab_parent = new GameObject("Grab_Points");
            grab_parent.transform.SetParent(transform);
            grab_parent.transform.localPosition = Vector3.zero;
            
            grab_points = new Transform[4];
            Vector3[] positions = {
                Vector3.forward,   // Front
                Vector3.back,      // Back  
                Vector3.left,      // Left
                Vector3.right      // Right
            };
            
            for (int i = 0; i < 4; i++)
            {
                GameObject grab_point = new GameObject($"Grab_Point_{i}");
                grab_point.transform.SetParent(grab_parent.transform);
                grab_point.transform.localPosition = positions[i] * 1.5f;
                grab_points[i] = grab_point.transform;
            }
        }
    }
    
    private Transform Find_Closest_Grab_Point(Vector3 player_position)
    {
        Transform closest = grab_points[0];
        float closest_distance = float.MaxValue;
        
        foreach (Transform grab_point in grab_points)
        {
            float distance = Vector3.Distance(player_position, grab_point.position);
            if (distance < closest_distance)
            {
                closest_distance = distance;
                closest = grab_point;
            }
        }
        
        return closest;
    }
    
    private void Snap_Player_To_Grab_Point()
    {
        Vector3 target_position = current_grab_point.position;
        target_position.y = current_player.transform.position.y;
        
        float distance = Vector3.Distance(current_player.transform.position, target_position);
        
        if (distance > 0.1f)
        {
            current_player.transform.position = Vector3.Lerp(
                current_player.transform.position, 
                target_position, 
                snap_speed * Time.deltaTime
            );
        }
        else
        {
            player_snapped = true;
        }
    }
    
    private void Handle_Push_Input()
    {
        Vector3 input_direction = current_player.Get_Movement_Input();
        float rotation_input = current_player.Get_Rotation_Input();
        
        // Handle movement
        if (input_direction.magnitude > 0.1f)
        {
            push_direction = new Vector3(input_direction.x, 0, input_direction.z).normalized;
            
            Vector3 force = push_direction * push_force;
            
            if (object_type == Push_Object_Type.Heavy)
                force *= 0.5f;
            else if (object_type == Push_Object_Type.Slippery)
                force *= 1.5f;
            
            rb.AddForce(force, ForceMode.Force);
        }
        
        // Handle rotation (independent of movement)
        if (Mathf.Abs(rotation_input) > 0.1f)
        {
            float final_rotation_force = rotation_force;
            
            if (object_type == Push_Object_Type.Heavy)
                final_rotation_force *= 0.7f;
            else if (object_type == Push_Object_Type.Slippery)
                final_rotation_force *= 1.2f;
            
            rb.AddTorque(Vector3.up * rotation_input * final_rotation_force, ForceMode.Force);
        }
    }
    
    private void Update_Player_Position()
    {
        Vector3 target_position = current_grab_point.position;
        target_position.y = current_player.transform.position.y;
        current_player.transform.position = target_position;
    }
    
    void OnDrawGizmos()
    {
        if (!show_debug) return;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
        
        if (grab_points != null)
        {
            foreach (Transform grab_point in grab_points)
            {
                if (grab_point != null)
                {
                    Gizmos.color = grab_point == current_grab_point ? Color.red : Color.cyan;
                    Gizmos.DrawWireSphere(grab_point.position, 0.3f);
                    Gizmos.DrawLine(transform.position, grab_point.position);
                }
            }
        }
        
        if (is_being_pushed && push_direction != Vector3.zero)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, push_direction * 2f);
        }
        
        Color player_color = allowed_players == Allowed_Players.Luthe_Only ? Color.blue :
                           allowed_players == Allowed_Players.Cherie_Only ? Color.green : Color.white;
        Gizmos.color = player_color;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.5f);
    }
}