using UnityEngine;

public class Door_Logic : MonoBehaviour, I_Interactable, IInteractionIdentifier
{
    [SerializeField] private string requiredKeyID = "door_key";
    [SerializeField] private Animation doorAnimation;
    [SerializeField] private AnimationClip doorOpenClip;
    [SerializeField] private AnimationClip doorClosedClip;
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
    }
    
    public bool Can_Interact(Player_Type player_type)
    {
        if (isOpen) return false;
        if (Inventory_Manager.Instance == null) return false;
        return Inventory_Manager.Instance.HasItem(requiredKeyID, player_type);
    }
    
    public void Start_Interaction(Player_Controller player)
    {
        if (isOpen) return;
        
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
            doorAnimation.clip = doorOpenClip;
            doorAnimation.Play();
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
        return Can_Interact(Player_Type.Luthe) || Can_Interact(Player_Type.Cherie) ? "Open door" : "Locked";
    }
    
    public Vector3 Get_Interaction_Position()
    {
        return transform.position;
    }
    
    public string GetInteractionID()
    {
        return interaction_id;
    }
}