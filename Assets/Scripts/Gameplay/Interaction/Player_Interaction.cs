/// ---------- Fixed player interaction with priority system ----------

using UnityEngine;

public class Player_Interaction : MonoBehaviour
{
    [SerializeField] private float interaction_range = 2f;
    [SerializeField] private LayerMask interaction_layer = -1;
    
    private Player_Controller player_controller;
    private I_Interactable current_interactable;
    private bool is_interacting = false;
    
    void Start()
    {
        player_controller = GetComponent<Player_Controller>();
    }
    
    void Update()
    {
        Find_Nearby_Interactable();
        Handle_Interaction_Input();
    }
    
    private void Find_Nearby_Interactable()
    {
        Collider[] nearby_objects = Physics.OverlapSphere(transform.position, interaction_range, interaction_layer);
        I_Interactable closest_interactable = null;
        float closest_distance = float.MaxValue;
        
        // First pass: Look for pickable objects (priority)
        foreach (Collider col in nearby_objects)
        {
            Pickable_Object pickable = col.GetComponent<Pickable_Object>();
            if (pickable != null && pickable.Can_Interact(player_controller.Get_Player_Type()))
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);
                if (distance < closest_distance)
                {
                    closest_distance = distance;
                    closest_interactable = pickable;
                }
            }
        }
        
        // Second pass: Look for other interactables only if no pickables found
        if (closest_interactable == null)
        {
            foreach (Collider col in nearby_objects)
            {
                I_Interactable interactable = col.GetComponent<I_Interactable>();
                if (interactable != null && !(interactable is Pickable_Object) && interactable.Can_Interact(player_controller.Get_Player_Type()))
                {
                    float distance = Vector3.Distance(transform.position, col.transform.position);
                    if (distance < closest_distance)
                    {
                        closest_distance = distance;
                        closest_interactable = interactable;
                    }
                }
            }
        }
        
        current_interactable = closest_interactable;
    }
    
    private void Handle_Interaction_Input()
    {
        bool interact_pressed = player_controller.Get_Interact_Input();
        bool interact_held = player_controller.Get_Interact_Held();
        
        if (interact_pressed)
        {
            Pickable_Object held_object = player_controller.Get_Held_Object();
            if (held_object != null)
            {
                Debug.Log($"Dropping held object: {held_object.Get_Object_Name()}");
                
                bool should_throw = false;
                Vector3 movement_input = player_controller.Get_Movement_Input();
                if (movement_input.magnitude > 0.1f)
                {
                    should_throw = true;
                }
                
                Vector3 drop_position = player_controller.transform.position + player_controller.transform.forward * 1.5f;
                held_object.Drop_Object(drop_position, should_throw);
                return; 
            }
            
            if (current_interactable != null && !is_interacting)
            {
                if (current_interactable is Pickable_Object)
                {
                    Debug.Log($"Picking up: {((Pickable_Object)current_interactable).Get_Object_Name()}");
                    current_interactable.Start_Interaction(player_controller);
                }
                else if (interact_held)
                {
                    Debug.Log("Starting push interaction");
                    Start_Interaction();
                }
            }
        }
        else if (is_interacting && !interact_held)
        {
            End_Interaction();
        }
}
    
    private void Start_Interaction()
    {
        is_interacting = true;
        current_interactable.Start_Interaction(player_controller);
    }
    
    private void End_Interaction()
    {
        if (current_interactable != null)
        {
            current_interactable.End_Interaction(player_controller);
        }
        is_interacting = false;
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interaction_range);
        
        if (current_interactable != null)
        {
            Gizmos.color = current_interactable is Pickable_Object ? Color.green : Color.yellow;
            Gizmos.DrawLine(transform.position, current_interactable.Get_Interaction_Position());
        }
    }
}