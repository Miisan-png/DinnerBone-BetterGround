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
        
        foreach (Collider col in nearby_objects)
        {
            I_Interactable interactable = col.GetComponent<I_Interactable>();
            if (interactable != null && interactable.Can_Interact(player_controller.Get_Player_Type()))
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);
                if (distance < closest_distance)
                {
                    closest_distance = distance;
                    closest_interactable = interactable;
                }
            }
        }
        
        current_interactable = closest_interactable;
    }
    
    private void Handle_Interaction_Input()
    {
        bool interact_pressed = player_controller.Get_Interact_Input();
        bool interact_held = player_controller.Get_Interact_Held();
        
        if (current_interactable != null && interact_held && !is_interacting)
        {
            Start_Interaction();
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
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, current_interactable.Get_Interaction_Position());
        }
    }
}