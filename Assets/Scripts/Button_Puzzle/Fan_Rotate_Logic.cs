using UnityEngine;
using DG.Tweening;

public class Fan_Rotate_Logic : MonoBehaviour
{
    [Header("Fan Rotation")]
    [SerializeField] private Transform fan_blades;
    [SerializeField] private float rotation_speed = 100f;
    [SerializeField] private float slowdown_amount = 20f;
    [SerializeField] private float min_speed = 10f;
    
    [Header("Button Icon")]
    [SerializeField] private GameObject button_icon;
    [SerializeField] private Material[] button_materials;
    
    [Header("Button Connections")]
    [SerializeField] private Fan_Button_Logic[] connected_buttons;
    
    [Header("Randomization Settings")]
    [SerializeField] private bool use_random_order = true;
    [SerializeField] private float min_icon_change_interval = 1.5f;
    [SerializeField] private float max_icon_change_interval = 3f;
    
    [Header("Slowdown Settings")]
    [SerializeField] private float slowdown_duration = 2f; // How long the slowdown effect lasts
    [SerializeField] private float speed_recovery_rate = 30f; // How fast speed recovers per second

    [SerializeField] private AudioSource fanAudio;
    
    private Material icon_material;
    private int current_button_index = 0;
    private int current_icon_id = 0;  // ID that matches the material index
    private float rotation_timer = 0f;
    private float icon_change_interval = 2f;
    private float current_rotation_speed;
    private float target_rotation_speed;
    private bool is_recovering_speed = false;
    private Tween speed_recovery_tween;
    
    // Randomization variables
    private int previous_icon_id = -1;

    void Start()
    {
        current_rotation_speed = rotation_speed;
        target_rotation_speed = rotation_speed;
        
        // Set initial random interval
        if (use_random_order)
        {
            icon_change_interval = Random.Range(min_icon_change_interval, max_icon_change_interval);
        }
        
        if (button_icon != null)
        {
            Renderer renderer = button_icon.GetComponent<Renderer>();
            if (renderer != null)
            {
                icon_material = renderer.material;
                UpdateIconMaterial();
                button_icon.SetActive(true);
            }
        }
        
        // Connect this fan to all the buttons
        ConnectToButtons();
    }

    void Update()
    {
        if (fan_blades != null)
        {
            // Changed from Vector3.forward to -Vector3.forward to rotate clockwise (to the right)
            fan_blades.Rotate(-Vector3.forward, current_rotation_speed * Time.deltaTime);
            fanAudio.pitch = current_rotation_speed / rotation_speed;

        }

        rotation_timer += Time.deltaTime;
        if (rotation_timer >= icon_change_interval)
        {
            rotation_timer = 0f;
            ChangeToNextIcon();
        }
    }
    
    private void ChangeToNextIcon()
    {
        if (use_random_order)
        {
            // Generate random icon ID (but different from previous)
            int new_icon_id;
            int attempts = 0;
            
            do
            {
                new_icon_id = Random.Range(0, button_materials.Length);
                attempts++;
            }
            while (new_icon_id == previous_icon_id && attempts < 10); // Prevent infinite loop
            
            previous_icon_id = current_icon_id;
            current_icon_id = new_icon_id;
            current_button_index = current_icon_id; // Keep in sync
            
            // Set new random interval for next change
            icon_change_interval = Random.Range(min_icon_change_interval, max_icon_change_interval);
        }
        else
        {
            // Original sequential behavior
            current_button_index = (current_button_index + 1) % button_materials.Length;
            current_icon_id = current_button_index;
        }
        
        UpdateIconMaterial();
    }
    
    private void ConnectToButtons()
    {
        if (connected_buttons != null)
        {
            foreach (Fan_Button_Logic button in connected_buttons)
            {
                if (button != null)
                {
                    button.SetConnectedFan(this);
                }
            }
        }
    }
    
    private void UpdateIconMaterial()
    {
        if (icon_material != null && button_materials != null && button_materials.Length > 0)
        {
            // Use current_icon_id to get the exact material index
            int material_index = current_icon_id % button_materials.Length;
            if (button_materials[material_index] != null)
            {
                icon_material.CopyPropertiesFromMaterial(button_materials[material_index]);
            }
        }
    }
    
    public int GetCurrentButtonIndex()
    {
        return current_button_index;
    }
    
    public int GetCurrentIconId()
    {
        return current_icon_id;
    }
    
    // Method to manually set icon ID (useful for button matching)
    public void SetIconId(int id)
    {
        if (id >= 0 && id < button_materials.Length)
        {
            previous_icon_id = current_icon_id;
            current_icon_id = id;
            current_button_index = id;
            UpdateIconMaterial();
        }
    }
    
    // Method to force a random icon change (useful for testing)
    public void ForceRandomIconChange()
    {
        if (use_random_order)
        {
            ChangeToNextIcon();
        }
    }
    
    // Method to toggle randomization mode
    public void SetRandomMode(bool random)
    {
        use_random_order = random;
        if (random)
        {
            // Reset timer with new random interval
            icon_change_interval = Random.Range(min_icon_change_interval, max_icon_change_interval);
            rotation_timer = 0f;
        }
        else
        {
            // Reset to default interval for sequential mode
            icon_change_interval = 2f;
        }
    }

    private Tween slowdown_tween;

    public void SlowDownFan()
    {
        // Kill any existing slowdown tween (and prevent OnComplete from running)
        if (slowdown_tween != null)
        {
            slowdown_tween.Kill(true);
            slowdown_tween = null;
        }

        // Kill any existing recovery tween
        if (speed_recovery_tween != null)
        {
            speed_recovery_tween.Kill(true);
            speed_recovery_tween = null;
        }

        // Calculate new target speed
        float new_target_speed = Mathf.Max(min_speed, current_rotation_speed - slowdown_amount);

        // Animate to the slower speed
        slowdown_tween = DOTween.To(() => current_rotation_speed,
                                     x => current_rotation_speed = x,
                                     new_target_speed,
                                     0.5f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                // After slowdown, start recovery after a delay
                DOVirtual.DelayedCall(slowdown_duration, () =>
                {
                    StartSpeedRecovery();
                });
            });

        Debug.Log($"Fan slowed down! Speed: {current_rotation_speed} -> {new_target_speed}");

        if (new_target_speed <= min_speed)
        {
            OnFanStopped();
        }
    }

    private void StartSpeedRecovery()
    {
        if (current_rotation_speed < rotation_speed)
        {
            is_recovering_speed = true;

            // Gradually recover speed
            speed_recovery_tween = DOTween.To(
                () => current_rotation_speed, 
                x => current_rotation_speed = x, 
                rotation_speed, 
                (rotation_speed - current_rotation_speed) / speed_recovery_rate
            )
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                is_recovering_speed = false;
                Debug.Log("Fan speed fully recovered!");
            });
        }
    }
    
    private void OnFanStopped()
    {
        Debug.Log("Fan has stopped!");
        
        // Optional: Add some special effect or sound when fan stops
        // Maybe trigger a win condition or unlock something
    }
    
    // Public method to check if fan is stopped (useful for win conditions)
    public bool IsFanStopped()
    {
        return current_rotation_speed <= min_speed;
    }
    
    // Public method to get current speed percentage
    public float GetSpeedPercentage()
    {
        return current_rotation_speed / rotation_speed;
    }
    
    // Method to reset fan to full speed (useful for puzzle reset)
    public void ResetFanSpeed()
    {
        if (speed_recovery_tween != null)
        {
            speed_recovery_tween.Kill();
        }
        
        current_rotation_speed = rotation_speed;
        target_rotation_speed = rotation_speed;
        is_recovering_speed = false;
    }
    
    void OnDestroy()
    {
        // Clean up tweens
        if (speed_recovery_tween != null)
        {
            speed_recovery_tween.Kill();
        }
    }
}