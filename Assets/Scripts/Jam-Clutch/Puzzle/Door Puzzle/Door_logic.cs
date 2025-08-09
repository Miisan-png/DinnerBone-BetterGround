using UnityEngine;

public class Door_Logic : MonoBehaviour, I_Interactable, IInteractionIdentifier, IIconVisibility
{
    [Header("Door Settings")]
    [SerializeField] private bool noKeyRequired = false; // Toggle to bypass key requirement
    [SerializeField] private string requiredKeyID = "door_key";
    
    [Header("Animation")]
    [SerializeField] private Animation doorAnimation;
    [SerializeField] private AnimationClip doorOpenClip;
    [SerializeField] private AnimationClip doorClosedClip;
    
    [Header("UI")]
    [SerializeField] private Advanced_Interact_Icon interactIcon;
    [SerializeField] private string interaction_id = "door_open";
    
    private bool isOpen = false;
    
    void Start()
    {
        if (doorAnimation == null)
            doorAnimation = GetComponent<Animation>();
        
        if (interactIcon == null)
            interactIcon = GetComponentInChildren<Advanced_Interact_Icon>();
        
        if (doorAnimation != null && doorClosedClip != null)
        {
            doorAnimation.clip = doorClosedClip;
            doorAnimation.Play();
        }
        
        if (doorOpenClip != null)
        {
            doorOpenClip.legacy = true;
        }
        if (doorClosedClip != null)
        {
            doorClosedClip.legacy = true;
        }
    }
    
    public bool Can_Interact(Player_Type player_type)
    {
        if (isOpen) return false;
        
        // If no key required, always allow interaction
        if (noKeyRequired) return true;
        
        // Otherwise check for key
        if (Inventory_Manager.Instance == null) return false;
        return Inventory_Manager.Instance.HasItem(requiredKeyID, player_type);
    }
    
    public void Start_Interaction(Player_Controller player)
    {
        if (isOpen) return;
        
        // If no key required, just open the door
        if (noKeyRequired)
        {
            Debug.Log("Opening door (no key required)");
            OpenDoor();
            return;
        }
        
        // Otherwise check for key
        if (Inventory_Manager.Instance != null && Inventory_Manager.Instance.HasItem(requiredKeyID, player.Get_Player_Type()))
        {
            Debug.Log($"Opening door with key: {requiredKeyID}");
            Inventory_Manager.Instance.DropItem(player.Get_Player_Type());
            OpenDoor();
        }
        else
        {
            Debug.Log($"Cannot open door - missing key: {requiredKeyID}");
        }
    }
    
    public void End_Interaction(Player_Controller player)
    {
    }
    
    private void OpenDoor()
    {
        isOpen = true;
        Debug.Log("Door opening...");
        
        if (doorAnimation != null && doorOpenClip != null)
        {
            doorAnimation.AddClip(doorOpenClip, doorOpenClip.name);
            doorAnimation.AddClip(doorClosedClip, doorClosedClip.name);
            doorAnimation.Play(doorOpenClip.name);
            Debug.Log($"Playing animation: {doorOpenClip.name}");
        }
        else
        {
            Debug.LogError($"Missing components - Animation: {doorAnimation != null}, OpenClip: {doorOpenClip != null}");
        }
        
        if (interactIcon != null)
        {
            interactIcon.Hide();
        }
    }
    
    public string Get_Interaction_Text()
    {
        if (noKeyRequired)
            return "Open door";
        
        return Can_Interact(Player_Type.Luthe) || Can_Interact(Player_Type.Cherie) ? "Open door" : "Locked";
    }
    
    public Vector3 Get_Interaction_Position()
    {
        Proximity_Prompt_Helper helper = GetComponent<Proximity_Prompt_Helper>();
        if (helper != null)
        {
            return helper.GetPromptPosition();
        }
        return transform.position + transform.forward * 1.5f;
    }
    
    public string GetInteractionID()
    {
        return interaction_id;
    }
    
    public bool ShouldShowIcon()
    {
        return !isOpen;
    }
    
    public bool IsOpen()
    {
        return isOpen;
    }
}