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
    
    [Header("Dual Panel Settings")]
    [SerializeField] private bool is_dual_panel_mode = false;
    
    [Header("Power Indicator Settings")]
    [SerializeField] private GameObject[] power_indicator_planes;
    [SerializeField] private float[] correct_z_rotations;
    [SerializeField] private LightFlickerManager light_flicker_manager;
    [SerializeField] private float reset_delay = 5f;
    [SerializeField] private float reset_duration = 1f;
    
    private Player_Controller interacting_player;
    private bool is_puzzle_active = false;
    private bool is_rotating_object = false;
    private int current_component_index = 0;
    private Player_Movement player_movement;
    private bool input_pressed = false;
    private bool interact_pressed = false;
    private Material[] indicator_materials;
    private bool[] components_correct;
    private int correct_components_count = 0;
    private bool puzzle_solved = false;
    private Vector3[] original_rotations;
    private Tween[] reset_tweens;
    private bool[] has_been_rotated;
    private Dual_Panel_Manager dual_panel_manager;
    private int panel_id = 0;
    
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
        
        if (correct_z_rotations == null || correct_z_rotations.Length != power_components.Length)
        {
            correct_z_rotations = new float[power_components.Length];
            for (int i = 0; i < correct_z_rotations.Length; i++)
            {
                correct_z_rotations[i] = 0f;
            }
        }
        
        components_correct = new bool[power_components.Length];
        original_rotations = new Vector3[power_components.Length];
        reset_tweens = new Tween[power_components.Length];
        has_been_rotated = new bool[power_components.Length];
        
        for (int i = 0; i < power_components.Length; i++)
        {
            if (power_components[i] != null)
            {
                original_rotations[i] = power_components[i].eulerAngles;
            }
        }
        
        InitializePowerIndicators();
    }
    
    private void InitializePowerIndicators()
    {
        if (power_indicator_planes == null) return;
        
        indicator_materials = new Material[power_indicator_planes.Length];
        
        for (int i = 0; i < power_indicator_planes.Length; i++)
        {
            if (power_indicator_planes[i] != null)
            {
                Renderer renderer = power_indicator_planes[i].GetComponent<Renderer>();
                if (renderer != null)
                {
                    indicator_materials[i] = new Material(renderer.material);
                    renderer.material = indicator_materials[i];
                    SetIndicatorRed(i);
                }
            }
        }
    }
    
    private void SetIndicatorRed(int index)
    {
        if (indicator_materials == null || index >= indicator_materials.Length || indicator_materials[index] == null) return;
        
        Material mat = indicator_materials[index];
        Color deepRed = new Color(0.8f, 0.1f, 0.1f, 1f);
        mat.SetColor("_GlowColor", deepRed);
        mat.SetFloat("_GlowIntensity", 3f);
    }
    
    private void SetIndicatorGreen(int index)
    {
        if (indicator_materials == null || index >= indicator_materials.Length || indicator_materials[index] == null) return;
        
        Material mat = indicator_materials[index];
        Color brightGreen = new Color(0.2f, 1f, 0.2f, 1f);
        mat.SetColor("_GlowColor", brightGreen);
        mat.SetFloat("_GlowIntensity", 3f);
        
        Debug.Log($"Setting indicator {index} to green. Material: {mat.name}");
    }
    
    public bool Can_Interact(Player_Type player_type)
    {
        return !is_puzzle_active && !puzzle_solved;
    }
    
    public void Start_Interaction(Player_Controller player)
    {
        if (is_puzzle_active || puzzle_solved) return;
        
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
            if (!is_rotating_object)
            {
                StartObjectRotation();
            }
            else
            {
                ExitObjectRotation();
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
            int old_index = current_component_index;
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
                if (has_been_rotated[old_index])
                {
                    StartResetTimer(old_index);
                }
                
                current_component_index = new_index;
                MoveArrowToCurrentComponent();
                UpdateArrowAppearance();
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
            SoundManager.Instance.PlaySound("sfx_wire_select");

            Vector3 currentScale = arrow_indicator.transform.localScale;
            arrow_indicator.transform.DOScale(currentScale * 1.2f, 0.15f)
                .OnComplete(() => {
                    arrow_indicator.transform.DOScale(currentScale, 0.15f);
                });
        }
    }
    
    private void ExitObjectRotation()
    {
        is_rotating_object = false;
        SoundManager.Instance.PlaySound("sfx_wire_deselect");
        UpdateArrowAppearance();
    }
    
    private void RotateCurrentObject(float degrees)
    {
        if (current_component_index < power_components.Length && 
            power_components[current_component_index] != null)
        {
            Transform target = power_components[current_component_index];
            Vector3 currentRotation = target.eulerAngles;
            Vector3 targetRotation = currentRotation + (Vector3.forward * degrees);
            
            has_been_rotated[current_component_index] = true;
            SoundManager.Instance.PlaySound("sfx_wire_rotate");


            if (reset_tweens[current_component_index] != null)
            {
                reset_tweens[current_component_index].Kill();
            }
            
            target.DORotate(targetRotation, rotation_duration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => {
                    CheckComponentCorrectness(current_component_index);
                });
        }
    }
    
    private void CheckComponentCorrectness(int componentIndex)
    {
        if (componentIndex >= power_components.Length || componentIndex >= correct_z_rotations.Length) return;
        
        Transform component = power_components[componentIndex];
        float currentZ = NormalizeAngle(component.eulerAngles.z);
        float correctZ = NormalizeAngle(correct_z_rotations[componentIndex]);
        
        bool was_correct = components_correct[componentIndex];
        bool is_correct = Mathf.Abs(currentZ - correctZ) < 5f;
        
        components_correct[componentIndex] = is_correct;
        
        if (is_correct && !was_correct)
        {
            correct_components_count++;
            if (reset_tweens[componentIndex] != null)
            {
                reset_tweens[componentIndex].Kill();
            }
        }
        else if (!is_correct && was_correct)
        {
            correct_components_count--;
            if (is_dual_panel_mode && dual_panel_manager != null)
            {
                dual_panel_manager.NotifyPanelComplete(panel_id, false);
            }
        }
        
        Debug.Log($"Component {componentIndex}: Correct={is_correct}, Total Correct={correct_components_count}/{power_components.Length}");
        
        if (correct_components_count == power_components.Length)
        {
            CompletePuzzle();
        }
    }
    
    private float NormalizeAngle(float angle)
    {
        angle = angle % 360f;
        if (angle < 0) angle += 360f;
        return angle;
    }
    
    private void StartResetTimer(int componentIndex)
    {
        if (!has_been_rotated[componentIndex]) return;
        
        reset_tweens[componentIndex] = DOVirtual.DelayedCall(reset_delay, () => {
            ResetComponent(componentIndex);
        });
    }
    
    private void ResetComponent(int componentIndex)
    {
        if (componentIndex >= power_components.Length || puzzle_solved) return;
        
        Transform component = power_components[componentIndex];
        component.DORotate(original_rotations[componentIndex], reset_duration);
        has_been_rotated[componentIndex] = false;
        
        bool was_correct = components_correct[componentIndex];
        components_correct[componentIndex] = false;
        
        if (was_correct)
        {
            correct_components_count--;
        }
    }
    
    private void UpdatePowerIndicators()
    {
        for (int i = 0; i < power_indicator_planes.Length; i++)
        {
            int index = i;
            DOVirtual.DelayedCall(i * 0.3f, () => {
                SetIndicatorGreen(index);
            });
        }
    }
    
    private void CompletePuzzle()
    {
        puzzle_solved = true;
        
        for (int i = 0; i < reset_tweens.Length; i++)
        {
            if (reset_tweens[i] != null)
            {
                reset_tweens[i].Kill();
            }
        }
        
        Debug.Log("Puzzle completed! Starting indicator sequence...");
        
        if (is_dual_panel_mode && dual_panel_manager != null)
        {
            dual_panel_manager.NotifyPanelComplete(panel_id, true);
        }
        else if (!is_dual_panel_mode)
        {
            UpdatePowerIndicators();
            if (light_flicker_manager != null)
            {
                DOVirtual.DelayedCall(power_indicator_planes.Length * 0.3f + 0.5f, () => {
                    light_flicker_manager.ToggleFullBrightness(true);
                });
            }
            DOVirtual.DelayedCall(2f, () => {
                ExitPuzzle();
            });
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
            SoundManager.Instance.PlaySound("sfx_wire_choose");
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
    }
    
    public string Get_Interaction_Text()
    {
        return puzzle_solved ? "Power restored" : "Use power panel";
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
    
    public void SetDualPanelManager(Dual_Panel_Manager manager)
    {
        dual_panel_manager = manager;
        is_dual_panel_mode = true;
    }
    
    public void SetPanelID(int id)
    {
        panel_id = id;
    }
    
    public void TriggerPanelCompletion()
    {
        UpdatePowerIndicators();
    }
    
    public void ExitPuzzleFromManager()
    {
        ExitPuzzle();
    }
    
    public void ForceReset()
    {
        puzzle_solved = false;
        correct_components_count = 0;
        
        for (int i = 0; i < components_correct.Length; i++)
        {
            components_correct[i] = false;
        }
        
        for (int i = 0; i < has_been_rotated.Length; i++)
        {
            has_been_rotated[i] = false;
        }
        
        for (int i = 0; i < reset_tweens.Length; i++)
        {
            if (reset_tweens[i] != null)
            {
                reset_tweens[i].Kill();
            }
        }
        
        for (int i = 0; i < power_components.Length; i++)
        {
            if (power_components[i] != null)
            {
                power_components[i].DORotate(original_rotations[i], reset_duration);
            }
        }
        
        for (int i = 0; i < power_indicator_planes.Length; i++)
        {
            SetIndicatorRed(i);
        }
    }
    
    public bool IsPuzzleSolved()
    {
        return puzzle_solved;
    }
}