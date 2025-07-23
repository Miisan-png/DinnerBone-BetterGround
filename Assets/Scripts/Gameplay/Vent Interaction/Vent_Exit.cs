using UnityEngine;

public class Vent_Exit : MonoBehaviour, I_Interactable
{
    [SerializeField] private Transform exit_position;
    
    public bool Can_Interact(Player_Type player_type)
    {
        return Vent_Manager.Instance.Is_Player_In_Vent();
    }
    
    public void Start_Interaction(Player_Controller player)
    {
        Vent_Manager.Instance.Exit_Vent(player, this);
    }
    
    public void End_Interaction(Player_Controller player)
    {
    }
    
    public string Get_Interaction_Text()
    {
        return "Exit vent";
    }
    
    public Vector3 Get_Interaction_Position()
    {
        return transform.position;
    }
    
    public Transform Get_Exit_Position()
    {
        return exit_position;
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
        
        if (exit_position != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(exit_position.position, 0.5f);
            Gizmos.DrawLine(transform.position, exit_position.position);
        }
    }
}