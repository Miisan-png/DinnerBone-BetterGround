using UnityEngine;

public interface I_Interactable
{
    bool Can_Interact(Player_Type player_type);
    void Start_Interaction(Player_Controller player);
    void End_Interaction(Player_Controller player);
    string Get_Interaction_Text();
    Vector3 Get_Interaction_Position();
}