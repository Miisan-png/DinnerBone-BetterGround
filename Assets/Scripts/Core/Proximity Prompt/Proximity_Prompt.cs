using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Proximity_Prompt : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image icon_image;
    [SerializeField] private CanvasGroup canvas_group;
    
    [Header("Settings")]
    [SerializeField] private float appear_duration = 0.3f;
    [SerializeField] private float disappear_duration = 0.2f;
    [SerializeField] private bool debug_input = false;
    
    [Header("Interaction Effects")]
    [SerializeField] private Color interaction_color = Color.green;
    [SerializeField] private float interaction_scale = 1.2f;
    
    private Transform target_transform;
    private I_Interactable current_interactable;
    private Player_Controller assigned_player;
    private string interaction_id;
    private bool is_visible = false;
    private Tween current_tween;
    private Camera active_camera;
    private Proximity_Prompt_Helper prompt_helper;
    
    void Awake()
    {
        SetupUIComponents();
        
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
    
    private void SetupUIComponents()
    {
        if (icon_image == null)
        {
            icon_image = GetComponentInChildren<Image>();
            if (icon_image == null)
            {
                Transform iconTransform = transform.Find("Icon");
                if (iconTransform != null)
                {
                    icon_image = iconTransform.GetComponent<Image>();
                }
            }
        }
        
        if (debug_input)
        {
            Debug.Log($"Proximity_Prompt Setup - Icon Image: {(icon_image != null ? "Found" : "Missing")}");
        }
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
        
        prompt_helper = target.GetComponent<Proximity_Prompt_Helper>();
        
        if (interactable is IInteractionIdentifier identifier)
        {
            interaction_id = identifier.GetInteractionID();
        }
        else
        {
            interaction_id = DetermineInteractionID(interactable);
        }
        
        SetupUIComponents();
        UpdatePromptContent();
        UpdatePosition();
        
        if (debug_input)
        {
            Debug.Log($"Initializing prompt for player: {player.Get_Player_Type()} with interaction ID: {interaction_id}");
        }
    }
    
    public void ShowPrompt()
    {
        if (is_visible) return;
        
        is_visible = true;
        gameObject.SetActive(true);
        
        UpdatePromptContent();
        
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
    
    public void PlayInteractionEffect()
    {
        if (icon_image != null)
        {
            icon_image.color = interaction_color;
            icon_image.transform.DOScale(interaction_scale, 0.1f).OnComplete(() => {
                HidePrompt();
            });
        }
        else
        {
            HidePrompt();
        }
    }
    
    private void UpdatePromptContent()
    {
        if (current_interactable == null || assigned_player == null)
        {
            if (debug_input) Debug.Log("UpdatePromptContent: Missing interactable or player");
            return;
        }
        
        bool can_interact = current_interactable.Can_Interact(assigned_player.Get_Player_Type());
        
        if (Input_Detector.Instance == null)
        {
            if (debug_input) Debug.Log("UpdatePromptContent: Input_Detector.Instance is null");
            return;
        }
        
        if (Proximity_System.Instance == null)
        {
            if (debug_input) Debug.Log("UpdatePromptContent: Proximity_System.Instance is null");
            return;
        }
        
        InputDeviceType device_type = Input_Detector.Instance.GetPlayerDeviceType(assigned_player.Get_Player_Type());
        
        if (debug_input)
        {
            Debug.Log($"Player {assigned_player.Get_Player_Type()} using device: {device_type}, Can interact: {can_interact}, Interaction ID: {interaction_id}");
        }
        
        Input_Icon_Database icon_db = Proximity_System.Instance.GetIconDatabase();
        if (icon_db != null)
        {
            InteractionData interaction_data = icon_db.GetInteractionData(
                interaction_id, 
                device_type, 
                assigned_player.Get_Player_Type(), 
                !can_interact
            );
            
            if (icon_image != null)
            {
                if (interaction_data.icon != null)
                {
                    icon_image.sprite = interaction_data.icon;
                    if (debug_input) Debug.Log($"Updated icon sprite to: {interaction_data.icon.name}");
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
        }
        else
        {
            if (debug_input)
            {
                Debug.Log("Missing Icon Database in Proximity_System");
            }
        }
    }
    
    private string DetermineInteractionID(I_Interactable interactable)
    {
        string type_name = interactable.GetType().Name.ToLower();
        
        if (type_name.Contains("vent_entry"))
            return "vent_enter";
        else if (type_name.Contains("vent_exit"))
            return "vent_exit";
        else if (type_name.Contains("push_object"))
            return "push_object";
        else if (type_name.Contains("door"))
            return "door_open";
        else if (type_name.Contains("switch"))
            return "switch_activate";
        else if (type_name.Contains("lever"))
            return "lever_pull";
        else
            return "generic_interact";
    }
    
    private void UpdatePosition()
    {
        if (target_transform == null) return;
        
        Vector3 world_position;
        if (prompt_helper != null)
        {
            world_position = prompt_helper.GetPromptPosition();
        }
        else
        {
            world_position = target_transform.position + Vector3.up * 2f;
        }
        
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
    
    public Player_Controller GetAssignedPlayer()
    {
        return assigned_player;
    }
}