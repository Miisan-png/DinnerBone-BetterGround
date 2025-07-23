using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class Proximity_Prompt : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image icon_image;
    [SerializeField] private TextMeshProUGUI action_label;
    [SerializeField] private CanvasGroup canvas_group;
    
    [Header("Settings")]
    [SerializeField] private float appear_duration = 0.3f;
    [SerializeField] private float disappear_duration = 0.2f;
    [SerializeField] private float height_offset = 2f;
    [SerializeField] private bool debug_input = false;
    
    private Transform target_transform;
    private I_Interactable current_interactable;
    private Player_Controller assigned_player;
    private bool is_visible = false;
    private Tween current_tween;
    private Camera active_camera;
    
    void Awake()
    {
        if (canvas_group == null)
        {
            canvas_group = GetComponent<CanvasGroup>();
            if (canvas_group == null)
            {
                canvas_group = gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        canvas_group.alpha = 0f;
        transform.localScale = Vector3.zero;
    }
    
    void Update()
    {
        if (is_visible && target_transform != null)
        {
            UpdatePosition();
            UpdateLookAt();
        }
    }
    
    public void Initialize(Transform target, I_Interactable interactable, Player_Controller player)
    {
        target_transform = target;
        current_interactable = interactable;
        assigned_player = player;
        
        UpdatePromptContent();
        UpdatePosition();
    }
    
    public void ShowPrompt()
    {
        if (is_visible) return;
        
        is_visible = true;
        gameObject.SetActive(true);
        
        current_tween?.Kill();
        current_tween = DOTween.Sequence()
            .Append(canvas_group.DOFade(1f, appear_duration))
            .Join(transform.DOScale(Vector3.one, appear_duration).SetEase(Ease.OutBack))
            .SetUpdate(true);
    }
    
    public void HidePrompt(System.Action on_complete = null)
    {
        if (!is_visible) return;
        
        is_visible = false;
        
        current_tween?.Kill();
        current_tween = DOTween.Sequence()
            .Append(canvas_group.DOFade(0f, disappear_duration))
            .Join(transform.DOScale(Vector3.zero, disappear_duration).SetEase(Ease.InBack))
            .OnComplete(() => {
                gameObject.SetActive(false);
                on_complete?.Invoke();
            })
            .SetUpdate(true);
    }
    
    private void UpdatePromptContent()
    {
        if (current_interactable == null || assigned_player == null) return;
        
        bool can_interact = current_interactable.Can_Interact(assigned_player.Get_Player_Type());
        
        if (Input_Detector.Instance == null || Proximity_System.Instance == null) return;
        
        InputDeviceType device_type = Input_Detector.Instance.GetPlayerDeviceType(assigned_player.Get_Player_Type());
        
        if (debug_input)
        {
            Debug.Log($"Player {assigned_player.Get_Player_Type()} using device: {device_type}");
        }
        
        Input_Icon_Database icon_db = Proximity_System.Instance.GetIconDatabase();
        if (icon_db != null && icon_image != null)
        {
            Sprite icon = icon_db.GetIcon(device_type, assigned_player.Get_Player_Type(), !can_interact);
            if (icon != null)
            {
                icon_image.sprite = icon;
            }
            
            if (!can_interact)
            {
                icon_image.color = Color.red;
                if (canvas_group != null) canvas_group.alpha = 0.7f;
            }
            else
            {
                icon_image.color = Color.white;
                if (canvas_group != null) canvas_group.alpha = 1f;
            }
        }
        
        if (action_label != null)
        {
            string interaction_text = current_interactable.Get_Interaction_Text();
            action_label.text = can_interact ? interaction_text : "Cannot interact";
            action_label.color = can_interact ? Color.white : Color.red;
        }
    }
    
    private void UpdatePosition()
    {
        if (target_transform == null) return;
        
        Vector3 world_position = target_transform.position + Vector3.up * height_offset;
        transform.position = world_position;
    }
    
    private void UpdateLookAt()
    {
        Camera target_camera = GetNearestCamera();
        if (target_camera != null && target_camera != active_camera)
        {
            active_camera = target_camera;
        }
        
        if (active_camera != null)
        {
            Vector3 camera_position = active_camera.transform.position;
            Vector3 direction = camera_position - transform.position;
            
            if (direction.magnitude > 0.01f)
            {
                transform.LookAt(camera_position);
                transform.Rotate(0, 180, 0);
            }
        }
    }
    
    private Camera GetNearestCamera()
    {
        Camera nearest_camera = null;
        float nearest_distance = float.MaxValue;
        
        Camera[] cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
        
        foreach (Camera cam in cameras)
        {
            if (cam.gameObject.activeInHierarchy)
            {
                float distance = Vector3.Distance(transform.position, cam.transform.position);
                if (distance < nearest_distance)
                {
                    nearest_distance = distance;
                    nearest_camera = cam;
                }
            }
        }
        
        return nearest_camera;
    }
    
    public void UpdateContent()
    {
        UpdatePromptContent();
    }
    
    public bool IsAssignedToPlayer(Player_Controller player)
    {
        return assigned_player == player;
    }
    
    public I_Interactable GetInteractable()
    {
        return current_interactable;
    }
}