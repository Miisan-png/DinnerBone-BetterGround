using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class VelocityPanel : MonoBehaviour
{
    [Header("Panel Colors")]
    public Color baseColor = Color.white;
    public Color borderColor = Color.black;
    
    [Header("Border Settings")]
    public float borderWidth = 10f;
    
    private Image panelImage;
    private Material panelMaterial;
    
    void Awake()
    {
        panelImage = GetComponent<Image>();
        SetupPanel();
    }
    
    void SetupPanel()
    {
        if (panelMaterial == null)
        {
            panelMaterial = new Material(Shader.Find("VelocityUI/VelocityPanel"));
            panelImage.material = panelMaterial;
        }
        
        panelImage.type = Image.Type.Sliced;
        
        UpdateColors();
        UpdateBorderWidth();
    }
    
    void OnValidate()
    {
        if (Application.isPlaying && panelMaterial != null)
        {
            UpdateColors();
            UpdateBorderWidth();
        }
    }
    
    void UpdateColors()
    {
        if (panelMaterial != null)
        {
            panelMaterial.SetColor("_BaseColor", baseColor);
            panelMaterial.SetColor("_BorderColor", borderColor);
        }
    }
    
    void UpdateBorderWidth()
    {
        if (panelMaterial != null)
        {
            panelMaterial.SetFloat("_BorderWidth", borderWidth / 300f);
        }
    }
    
    public void SetBaseColor(Color color)
    {
        baseColor = color;
        UpdateColors();
    }
    
    public void SetBorderColor(Color color)
    {
        borderColor = color;
        UpdateColors();
    }
    
    public void SetBorderWidth(float width)
    {
        borderWidth = width;
        UpdateBorderWidth();
    }
}