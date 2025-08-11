using UnityEngine;

public class Vent_Manager : MonoBehaviour
{
    private static Vent_Manager instance;
    public static Vent_Manager Instance => instance;

    private Player_Controller current_vent_player;
    private bool player_in_vent = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Enter_Vent(Player_Controller player, Vent_Entry entry)
    {
        current_vent_player = player;
        player_in_vent = true;

        Vent_Fade.Instance.Fade_In_Out(() =>
        {
            TeleportPlayerToVent(player, entry);
        });
    }

    public void Exit_Vent(Player_Controller player, Vent_Exit exit)
    {
        if (current_vent_player != player) return;

        Vent_Fade.Instance.Fade_In_Out(() =>
        {
            TeleportPlayerFromVent(player, exit);
            ResetVentState();
        });
    }

    private void TeleportPlayerToVent(Player_Controller player, Vent_Entry entry)
    {
        var playerCC = player.GetComponent<CharacterController>();
        if (playerCC != null) playerCC.enabled = false;

        if (entry.Get_Enter_Position() != null)
        {
            player.transform.position = entry.Get_Enter_Position().position;
            player.transform.rotation = entry.Get_Enter_Position().rotation;
        }

        if (playerCC != null) playerCC.enabled = true;
    }

    private void TeleportPlayerFromVent(Player_Controller player, Vent_Exit exit)
    {
        var playerCC = player.GetComponent<CharacterController>();
        if (playerCC != null) playerCC.enabled = false;

        if (exit.Get_Exit_Position() != null)
        {
            player.transform.position = exit.Get_Exit_Position().position;
            player.transform.rotation = exit.Get_Exit_Position().rotation;
        }

        if (playerCC != null) playerCC.enabled = true;
    }

    private void ResetVentState()
    {
        current_vent_player = null;
        player_in_vent = false;
    }

    public bool Is_Player_In_Vent()
    {
        return player_in_vent;
    }

    public Player_Controller Get_Current_Vent_Player()
    {
        return current_vent_player;
    }
}
