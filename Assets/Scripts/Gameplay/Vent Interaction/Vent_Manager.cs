using UnityEngine;

public class Vent_Manager : MonoBehaviour
{
    private static Vent_Manager instance;
    public static Vent_Manager Instance => instance;
    
    private Player_Controller current_vent_player;
    private Camera_Manager main_camera_manager;
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
    
    void Start()
    {
        main_camera_manager = FindFirstObjectByType<Camera_Manager>();
    }
    
    public void Enter_Vent(Player_Controller player, Vent_Entry entry)
    {
        if (player_in_vent) return;
        
        current_vent_player = player;
        player_in_vent = true;
        
        Vent_Fade.Instance.Fade_In_Out(() => {
            TeleportPlayerToVent(player, entry);
            SetupSplitScreenForVent();
        });
    }
    
    public void Exit_Vent(Player_Controller player, Vent_Exit exit)
    {
        if (!player_in_vent || current_vent_player != player) return;
        
        Vent_Fade.Instance.Fade_In_Out(() => {
            TeleportPlayerFromVent(player, exit);
            RestoreNormalCameraSystem();
            ResetVentState();
        });
    }
    
    private void TeleportPlayerToVent(Player_Controller player, Vent_Entry entry)
    {
        CharacterController playerCC = player.GetComponent<CharacterController>();
        if (playerCC != null)
        {
            playerCC.enabled = false;
        }
        
        if (entry.Get_Enter_Position() != null)
        {
            player.transform.position = entry.Get_Enter_Position().position;
            player.transform.rotation = entry.Get_Enter_Position().rotation;
        }
        
        if (playerCC != null)
        {
            playerCC.enabled = true;
        }
    }
    
    private void TeleportPlayerFromVent(Player_Controller player, Vent_Exit exit)
    {
        CharacterController playerCC = player.GetComponent<CharacterController>();
        if (playerCC != null)
        {
            playerCC.enabled = false;
        }
        
        if (exit.Get_Exit_Position() != null)
        {
            player.transform.position = exit.Get_Exit_Position().position;
            player.transform.rotation = exit.Get_Exit_Position().rotation;
        }
        
        if (playerCC != null)
        {
            playerCC.enabled = true;
        }
    }
    
    private void SetupSplitScreenForVent()
    {
        if (main_camera_manager != null)
        {
            main_camera_manager.Force_Split_Screen();
            main_camera_manager.enabled = false;
            
            Camera_Follow follow_script = main_camera_manager.GetComponent<Camera_Follow>();
            if (follow_script != null) follow_script.enabled = false;
        }
    }
    
    private void RestoreNormalCameraSystem()
    {
        if (main_camera_manager != null)
        {
            main_camera_manager.Enable_Auto_Switch();
            main_camera_manager.enabled = true;
        }
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