using UnityEngine;
using DG.Tweening;

public class Panel_Interaction : MonoBehaviour, I_Interactable, IInteractionIdentifier
{
    [SerializeField] private string interaction_id = "power_panel";
    [SerializeField] private GameObject arrow_indicator;
    [SerializeField] private Transform[] power_components;
    [SerializeField] private float move_duration = 0.2f;
    [SerializeField] private float z_offset = -0.1f;
    [SerializeField] private float height_offset = 0.5f;
    
    private Player_Controller interacting_player;
    private bool is_puzzle_active = false;
    private int current_component_index = 0;
    private Player_Movement player_movement;
    private bool input_pressed = false;
    
    public bool Can_Interact(Player_Type player_type)
    {
        return !is_puzzle_active;
    }
    
    public void Start_Interaction(Player_Controller player)
    {
        if (is_puzzle_active) return;
        
        is_puzzle_active = true;
        interacting_player = player;
        player_movement = player.GetComponent<Player_Movement>();
        
        if (player_movement != null)
        {
            player_movement.enabled = false;
        }
        
        if (arrow_indicator != null)
        {
            arrow_indicator.SetActive(true);
            MoveArrowToCurrentComponent();
        }
    }
    
    public void End_Interaction(Player_Controller player)
    {
    }
    
    void Update()
    {
        if (!is_puzzle_active || interacting_player == null || power_components.Length == 0) return;
        
        Vector3 input = interacting_player.Get_Movement_Input();
        bool has_input = input.magnitude > 0.5f;
        
        if (has_input && !input_pressed)
        {
            input_pressed = true;
            int new_index = current_component_index;
            
            if (input.z > 0.5f)
            {
                new_index = MoveInGrid(-3);
            }
            else if (input.z < -0.5f)
            {
                new_index = MoveInGrid(3);
            }
            else if (input.x < -0.5f)
            {
                new_index = MoveInGrid(-1);
            }
            else if (input.x > 0.5f)
            {
                new_index = MoveInGrid(1);
            }
            
            if (new_index != current_component_index)
            {
                current_component_index = new_index;
                MoveArrowToCurrentComponent();
            }
        }
        else if (!has_input)
        {
            input_pressed = false;
        }
        
        if (interacting_player.Get_Interact_Input())
        {
            ExitPuzzle();
        }
        
        if (interacting_player.Get_Exit_Input())
        {
            ExitPuzzle();
        }
    }
    
    private int MoveInGrid(int direction)
    {
        int new_index = current_component_index + direction;
        
        if (direction == -1 || direction == 1)
        {
            int current_row = current_component_index / 3;
            int new_row = new_index / 3;
            if (current_row != new_row && (new_index < 0 || new_index >= power_components.Length))
            {
                return current_component_index;
            }
        }
        
        if (new_index < 0 || new_index >= power_components.Length)
        {
            return current_component_index;
        }
        
        return new_index;
    }
    
    private void MoveArrowToCurrentComponent()
    {
        if (arrow_indicator != null && current_component_index < power_components.Length)
        {
            Vector3 target_pos = power_components[current_component_index].position;
            target_pos.z += z_offset;
            target_pos.y += height_offset;
            arrow_indicator.transform.DOMove(target_pos, move_duration).SetEase(Ease.OutCubic);
        }
    }
    
    private void ExitPuzzle()
    {
        is_puzzle_active = false;
        
        if (arrow_indicator != null)
        {
            arrow_indicator.SetActive(false);
        }
        
        if (player_movement != null)
        {
            player_movement.enabled = true;
        }
        
        interacting_player = null;
    }
    
    public string Get_Interaction_Text()
    {
        return "Use power panel";
    }
    
    public Vector3 Get_Interaction_Position()
    {
        return transform.position;
    }
    
    public string GetInteractionID()
    {
        return interaction_id;
    }
}