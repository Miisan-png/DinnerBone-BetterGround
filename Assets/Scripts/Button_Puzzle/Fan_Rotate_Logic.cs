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
    
    private Material icon_material;
    private int current_button_index = 0;
    private float rotation_timer = 0f;
    private float icon_change_interval = 2f;
    private float current_rotation_speed;

    void Start()
    {
        current_rotation_speed = rotation_speed;
        
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
    }

    void Update()
    {
        if (fan_blades != null)
        {
            // Changed from Vector3.forward to -Vector3.forward to rotate clockwise (to the right)
            fan_blades.Rotate(-Vector3.forward, current_rotation_speed * Time.deltaTime);
        }
        
        rotation_timer += Time.deltaTime;
        if (rotation_timer >= icon_change_interval)
        {
            rotation_timer = 0f;
            current_button_index = (current_button_index + 1) % 3;
            UpdateIconMaterial();
        }
    }
    
    private void UpdateIconMaterial()
    {
        if (icon_material != null && button_materials != null && button_materials.Length > 0)
        {
            int material_index = current_button_index % button_materials.Length;
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
    
    public void SlowDownFan()
    {
        current_rotation_speed = Mathf.Max(min_speed, current_rotation_speed - slowdown_amount);
        
        if (current_rotation_speed <= min_speed)
        {
            OnFanStopped();
        }
    }
    
    private void OnFanStopped()
    {
        Debug.Log("Fan has stopped!");
    }
}