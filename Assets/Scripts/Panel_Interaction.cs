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
    
    [Header("Rotation Settings")]
    [SerializeField] private float rotation_speed = 90f;
    [SerializeField] private float rotation_duration = 0.3f;
    [SerializeField] private bool[] can_rotate_objects;
    [SerializeField] private Vector3 rotation_axis = Vector3.forward;
    
    private Player_Controller interacting_player;
    private bool is_puzzle_active = false;
    private bool is_rotating_object = false;
    private int current_component_index = 0;
    private Player_Movement player_movement;
    private bool input_pressed = false;
    private bool interact_pressed = false;
    
    void Start()
    {
        if (can_rotate_objects == null || can_rotate_objects.Length != power_components.Length)
        {
            can_rotate_objects = new bool[power_components.Length];
            for (int i = 0; i < can_rotate_objects.Length; i++)
            {
                can_rotate_objects[i] = true;
            }
        }
    }
    
    public bool Can_Interact(Player_Type player_type)
    {
        return !is_puzzle_active;
    }
    
    public void Start_Interaction(Player_Controller player)
    {
        if (is_puzzle_active) return;
        
        is_puzzle_active = true;
        is_rotating_object = false;
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
            UpdateArrowAppearance();
        }
        
        Debug.Log("Panel interaction started - Use movement keys to navigate, interact to select object for rotation");
    }
    
    public void End_Interaction(Player_Controller player)
    {
    }
    
    void Update()
    {
        if (!is_puzzle_active || interacting_player == null || power_components.Length == 0) return;
        
        HandleInteractInput();
        
        if (is_rotating_object)
        {
            HandleObjectRotation();
        }
        else
        {
            HandleNavigation();
        }
        
        HandleExitInput();
    }
    
    private bool last_interact_state = false;
    
    private void HandleInteractInput()
    {
        bool current_interact_held = interacting_player.Get_Interact_Held();
        bool interact_just_pressed = current_interact_held && !last_interact_state;
        
        if (interact_just_pressed)
        {
            Debug.Log("INTERACT BUTTON PRESSED!");
            
            if (!is_rotating_object)
            {
                StartObjectRotation();
                Debug.Log($"STARTED ROTATING COMPONENT {current_component_index}");
            }
            else
            {
                ExitObjectRotation();
                Debug.Log($"STOPPED ROTATING COMPONENT {current_component_index}");
            }
        }
        
        last_interact_state = current_interact_held;
    }
    
    private void HandleNavigation()
    {
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
                UpdateArrowAppearance();
                Debug.Log($"Selected component {current_component_index}");
            }
        }
        else if (!has_input)
        {
            input_pressed = false;
        }
    }
    
    private void HandleObjectRotation()
    {
        Vector3 input = interacting_player.Get_Movement_Input();
        bool has_input = input.magnitude > 0.5f;
        
        if (has_input && !input_pressed)
        {
            input_pressed = true;
            
            if (input.x < -0.5f)
            {
                RotateCurrentObject(rotation_speed);
            }
            else if (input.x > 0.5f)
            {
                RotateCurrentObject(-rotation_speed);
            }
        }
        else if (!has_input)
        {
            input_pressed = false;
        }
    }
    
    private void StartObjectRotation()
    {
        is_rotating_object = true;
        UpdateArrowAppearance();
        
        if (arrow_indicator != null)
        {
            Vector3 currentScale = arrow_indicator.transform.localScale;
            arrow_indicator.transform.DOScale(currentScale * 1.2f, 0.15f)
                .OnComplete(() => {
                    arrow_indicator.transform.DOScale(currentScale, 0.15f);
                });
        }
        
        Debug.Log($"Started rotating component {current_component_index} - Use left/right to rotate, interact to exit rotation mode");
    }
    
    private void ExitObjectRotation()
    {
        is_rotating_object = false;
        UpdateArrowAppearance();
        Debug.Log("Exited rotation mode - Use movement keys to navigate, interact to select object for rotation");
    }
    
    private void RotateCurrentObject(float degrees)
    {
        if (current_component_index < power_components.Length && 
            power_components[current_component_index] != null)
        {
            Transform target = power_components[current_component_index];
            Vector3 currentRotation = target.eulerAngles;
            Vector3 targetRotation = currentRotation + (Vector3.forward * degrees);
            
            target.DORotate(targetRotation, rotation_duration)
                .SetEase(Ease.OutQuad);
                
            Debug.Log($"Rotating component {current_component_index} by {degrees} degrees");
        }
    }
    
    private void UpdateArrowAppearance()
    {
        if (arrow_indicator == null) return;
        
        Renderer arrowRenderer = arrow_indicator.GetComponent<Renderer>();
        if (arrowRenderer != null && arrowRenderer.material != null)
        {
            Material mat = arrowRenderer.material;
            Color targetGlowColor;
            
            if (is_rotating_object)
            {
                targetGlowColor = Color.green;
            }
            else
            {
                targetGlowColor = Color.yellow;
            }
            
            if (mat.HasProperty("_GlowColor"))
            {
                mat.DOColor(targetGlowColor, "_GlowColor", 0.2f);
            }
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
    
    private void HandleExitInput()
    {
        if (interacting_player.Get_Exit_Input())
        {
            ExitPuzzle();
        }
    }
    
    private void ExitPuzzle()
    {
        is_puzzle_active = false;
        is_rotating_object = false;
        
        if (arrow_indicator != null)
        {
            arrow_indicator.SetActive(false);
        }
        
        if (player_movement != null)
        {
            player_movement.enabled = true;
        }
        
        interacting_player = null;
        Debug.Log("Exited panel interaction");
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
    
    public void SetObjectRotatable(int index, bool canRotate)
    {
        if (index >= 0 && index < can_rotate_objects.Length)
        {
            can_rotate_objects[index] = canRotate;
        }
    }
    
    public bool IsRotatingObject()
    {
        return is_rotating_object;
    }
    
    public int GetCurrentComponentIndex()
    {
        return current_component_index;
    }
}