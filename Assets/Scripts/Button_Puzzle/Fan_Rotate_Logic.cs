using UnityEngine;
using DG.Tweening;

public class Fan_Rotate_Logic : MonoBehaviour
{
    [Header("Fan Rotation")]
    [SerializeField] private Transform fan_blades;
    [SerializeField] private float rotation_speed = 100f;
    
    [Header("Button Icon")]
    [SerializeField] private GameObject button_icon;
    [SerializeField] private Material[] button_materials; // Assign 3 materials corresponding to 3 buttons
    
    private Material icon_material;
    private int current_button_index = 0;
    private float rotation_timer = 0f;
    private float icon_change_interval = 2f;

    void Start()
    {
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
        // Rotate the fan blades
        if (fan_blades != null)
        {
            fan_blades.Rotate(Vector3.forward, rotation_speed * Time.deltaTime);
        }
        
        // Change icon material periodically
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
                // Copy properties from the assigned material to the icon's material
                icon_material.CopyPropertiesFromMaterial(button_materials[material_index]);
            }
        }
    }
    
    public int GetCurrentButtonIndex()
    {
        return current_button_index;
    }
}