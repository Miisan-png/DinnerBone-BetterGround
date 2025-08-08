using UnityEngine;

public class Player_Movement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float move_speed = 5f;
    [SerializeField] private float sprint_speed = 8f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float jump_force = 8f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float rotation_speed = 10f;
    [SerializeField] private float lean_angle = 15f;
    [SerializeField] private Transform player_mesh;
    [SerializeField] private float rotation_threshold = 0.1f;

    [Header("Stamina Settings")]
    [SerializeField] private float max_stamina = 100f;
    [SerializeField] private float stamina_drain_rate = 20f; // Stamina per second while sprinting
    [SerializeField] private float stamina_regen_rate = 15f; // Stamina per second while not sprinting
    [SerializeField] private float min_stamina_to_sprint = 10f; // Minimum stamina needed to start sprinting
    [SerializeField] private float stamina_regen_delay = 1f; // Delay before stamina starts regenerating after stopping sprint

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audio_source;
    [SerializeField] private AudioClip walk_sound;
    [SerializeField] private float footstep_interval = 0.5f; // Time between footsteps
    private float footstep_timer;

    private CharacterController controller;
    private Vector3 velocity;
    private Vector3 target_velocity;
    private Vector3 last_movement_direction;
    private Vector2 current_input;
    private bool is_grounded;

    // Sprint variables
    private float current_stamina;
    private bool is_sprinting = false;
    private bool can_sprint = true;
    private float stamina_regen_timer = 0f;
    private bool sprint_input = false;

    // Events for UI updates (optional)
    public System.Action<float, float> OnStaminaChanged; // current, max

    void Start()
    {
        controller = GetComponent<CharacterController>();
        current_stamina = max_stamina;

        // Ensure the AudioSource component is assigned
        if (audio_source == null)
        {
            audio_source = GetComponent<AudioSource>();
        }

        if (player_mesh == null)
        {
            player_mesh = transform.GetChild(0);

            if (player_mesh == null)
            {
                Debug.LogWarning($"Player mesh not found for {gameObject.name}. Using main transform for rotation.");
                player_mesh = transform;
            }
        }

        Debug.Log($"Player mesh assigned to: {player_mesh.name}");
    }

    void Update()
    {
        if (controller == null || !controller.enabled) return;

        is_grounded = controller.isGrounded;

        if (is_grounded && velocity.y < 0)
            velocity.y = -2f;

        // Handle stamina and sprinting
        UpdateSprinting();
        UpdateStamina();

        // Calculate movement speed based on sprint state
        float current_speed = is_sprinting ? sprint_speed : move_speed;
        Vector3 desired_velocity = new Vector3(current_input.x, 0, current_input.y) * current_speed;

        Vector3 horizontal_velocity = Vector3.Lerp(
            new Vector3(velocity.x, 0, velocity.z),
            desired_velocity,
            acceleration * Time.deltaTime
        );

        velocity.x = horizontal_velocity.x;
        velocity.z = horizontal_velocity.z;
        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);

        UpdateMeshRotation();

        // Trigger stamina UI update
        OnStaminaChanged?.Invoke(current_stamina, max_stamina);

        // Footstep Audio Logic
        if (current_input.magnitude > 0.1f && is_grounded)
        {
            footstep_timer -= Time.deltaTime;
            if (footstep_timer <= 0)
            {
                if (audio_source != null && walk_sound != null)
                {
                    audio_source.PlayOneShot(walk_sound);
                }
                footstep_timer = footstep_interval;
            }
        }
        else // Reset the timer when the player stops moving
        {
            footstep_timer = 0;
        }
    }

    public void Move(Vector2 input)
    {
        current_input = input;
        target_velocity = new Vector3(input.x, 0, input.y) * move_speed;
    }

    public void SetSprintInput(bool sprinting)
    {
        sprint_input = sprinting;
    }

    public void Jump()
    {
        if (controller != null && controller.enabled && is_grounded)
        {
            velocity.y = jump_force;
            GetComponent<Player_Animation_Controller>()?.TriggerJump(); // Add this line
        }
    }

    private void UpdateSprinting()
    {
        bool is_moving = current_input.magnitude > 0.1f;
        bool wants_to_sprint = sprint_input && is_moving;
        bool has_stamina = current_stamina >= min_stamina_to_sprint;

        // Start sprinting conditions
        if (wants_to_sprint && has_stamina && can_sprint && !is_sprinting)
        {
            is_sprinting = true;
            stamina_regen_timer = 0f;
        }

        // Stop sprinting conditions
        if (is_sprinting && (!wants_to_sprint || current_stamina <= 0f))
        {
            is_sprinting = false;
            stamina_regen_timer = stamina_regen_delay;
        }

        // Update can_sprint flag (prevents sprint spam when out of stamina)
        if (current_stamina <= 0f)
        {
            can_sprint = false;
        }
        else if (current_stamina >= min_stamina_to_sprint * 2f) // Need more stamina to resume sprinting
        {
            can_sprint = true;
        }
    }

    private void UpdateStamina()
    {
        if (is_sprinting)
        {
            // Drain stamina while sprinting
            current_stamina -= stamina_drain_rate * Time.deltaTime;
            current_stamina = Mathf.Max(0f, current_stamina);
            stamina_regen_timer = stamina_regen_delay; // Reset regen timer
        }
        else
        {
            // Regenerate stamina when not sprinting
            if (stamina_regen_timer > 0f)
            {
                stamina_regen_timer -= Time.deltaTime;
            }
            else
            {
                current_stamina += stamina_regen_rate * Time.deltaTime;
                current_stamina = Mathf.Min(max_stamina, current_stamina);
            }
        }
    }

    private void UpdateMeshRotation()
    {
        if (player_mesh == null) return;

        Vector3 input_direction = new Vector3(current_input.x, 0, current_input.y);

        if (input_direction.magnitude > rotation_threshold)
        {
            last_movement_direction = input_direction.normalized;

            Quaternion target_rotation = Quaternion.LookRotation(last_movement_direction);

            player_mesh.rotation = Quaternion.Lerp(
                player_mesh.rotation,
                target_rotation,
                rotation_speed * Time.deltaTime
            );

            // Apply lean effect
            float lean_amount = Vector3.Dot(transform.right, last_movement_direction) * lean_angle;
            Vector3 lean_euler = new Vector3(0, player_mesh.eulerAngles.y, -lean_amount);
            player_mesh.rotation = Quaternion.Lerp(
                player_mesh.rotation,
                Quaternion.Euler(lean_euler),
                rotation_speed * Time.deltaTime
            );
        }
        else
        {
            if (last_movement_direction != Vector3.zero)
            {
                Quaternion target_rotation = Quaternion.LookRotation(last_movement_direction);
                Vector3 neutral_euler = new Vector3(0, target_rotation.eulerAngles.y, 0);
                player_mesh.rotation = Quaternion.Lerp(
                    player_mesh.rotation,
                    Quaternion.Euler(neutral_euler),
                    rotation_speed * Time.deltaTime
                );
            }
        }
    }

    // Public getters for other scripts
    public bool IsSprinting()
    {
        return is_sprinting;
    }

    public float GetCurrentStamina()
    {
        return current_stamina;
    }

    public float GetMaxStamina()
    {
        return max_stamina;
    }

    public float GetStaminaPercentage()
    {
        return current_stamina / max_stamina;
    }

    public bool CanSprint()
    {
        return can_sprint && current_stamina >= min_stamina_to_sprint;
    }

    void OnDrawGizmos()
    {
        if (player_mesh != null && last_movement_direction != Vector3.zero)
        {
            // Draw forward direction
            Gizmos.color = is_sprinting ? Color.red : Color.blue;
            Gizmos.DrawRay(player_mesh.position, player_mesh.forward * 2f);

            // Draw intended direction
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, last_movement_direction * 2f);
        }

        // Draw stamina as a sphere size
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            float stamina_ratio = current_stamina / max_stamina;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 3f, stamina_ratio);
        }
    }
}