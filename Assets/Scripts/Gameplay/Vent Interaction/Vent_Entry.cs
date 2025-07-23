using UnityEngine;

public class Vent_Entry : MonoBehaviour, I_Interactable, IInteractionIdentifier
{
    [SerializeField] private Allowed_Players allowed_players = Allowed_Players.Anyone;
    [SerializeField] private bool is_locked = false;
    [SerializeField] private Transform enter_position;
    [SerializeField] private Vent_Exit connected_exit;
    
    [Header("Database Integration")]
    [SerializeField] private string interaction_id = "vent_enter";
    
    public bool Can_Interact(Player_Type player_type)
    {
        if (is_locked) return false;
        
        switch (allowed_players)
        {
            case Allowed_Players.Luthe_Only:
                return player_type == Player_Type.Luthe;
            case Allowed_Players.Cherie_Only:
                return player_type == Player_Type.Cherie;
            case Allowed_Players.Anyone:
                return true;
            default:
                return false;
        }
    }
    
    public void Start_Interaction(Player_Controller player)
    {
        Vent_Manager.Instance.Enter_Vent(player, this);
    }
    
    public void End_Interaction(Player_Controller player)
    {
    }
    
    public string Get_Interaction_Text()
    {
        // This method is now mainly for fallback/compatibility
        // The database system will handle the actual text display
        return is_locked ? "Vent is locked" : "Enter vent";
    }
    
    public Vector3 Get_Interaction_Position()
    {
        return transform.position;
    }
    
    public string GetInteractionID()
    {
        return interaction_id;
    }
    
    public Transform Get_Enter_Position()
    {
        return enter_position;
    }
    
    public Vent_Exit Get_Connected_Exit()
    {
        return connected_exit;
    }
    
    public void Set_Locked(bool locked)
    {
        is_locked = locked;
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = is_locked ? Color.red : Color.cyan;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
        
        if (enter_position != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(enter_position.position, 0.5f);
            Gizmos.DrawLine(transform.position, enter_position.position);
        }
        
        Color player_color = allowed_players == Allowed_Players.Luthe_Only ? Color.blue :
                           allowed_players == Allowed_Players.Cherie_Only ? Color.green : Color.white;
        Gizmos.color = player_color;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.3f);
    }
}