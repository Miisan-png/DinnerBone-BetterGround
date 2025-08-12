using UnityEngine;
using UnityEngine.SceneManagement;

public class Vent_Manager : MonoBehaviour
{
    private static Vent_Manager instance;
    public static Vent_Manager Instance => instance;

    private Player_Controller current_vent_player;
    private bool player_in_vent = false;

    [SerializeField] private bool persistAcrossScenes = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            if (persistAcrossScenes) DontDestroyOnLoad(gameObject);
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (instance == this) SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        if (!persistAcrossScenes) { Destroy(gameObject); return; }
        if (!HasVentEndpointsInScene()) Destroy(gameObject);
    }

    bool HasVentEndpointsInScene()
    {
        return FindFirstObjectByType<Vent_Entry>() != null ||
               FindFirstObjectByType<Vent_Exit>()  != null;
    }

    public void Enter_Vent(Player_Controller player, Vent_Entry entry)
    {
        current_vent_player = player;
        player_in_vent = true;
        Vent_Fade.Instance.Fade_In_Out(() => { TeleportPlayerToVent(player, entry); });
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

    void TeleportPlayerToVent(Player_Controller player, Vent_Entry entry)
    {
        var cc = player.GetComponent<CharacterController>();
        if (cc) cc.enabled = false;
        if (entry.Get_Enter_Position())
        {
            player.transform.SetPositionAndRotation(entry.Get_Enter_Position().position,
                                                    entry.Get_Enter_Position().rotation);
        }
        if (cc) cc.enabled = true;
    }

    void TeleportPlayerFromVent(Player_Controller player, Vent_Exit exit)
    {
        var cc = player.GetComponent<CharacterController>();
        if (cc) cc.enabled = false;
        if (exit.Get_Exit_Position())
        {
            player.transform.SetPositionAndRotation(exit.Get_Exit_Position().position,
                                                    exit.Get_Exit_Position().rotation);
        }
        if (cc) cc.enabled = true;
    }

    void ResetVentState()
    {
        current_vent_player = null;
        player_in_vent = false;
    }

    public bool Is_Player_In_Vent() => player_in_vent;
    public Player_Controller Get_Current_Vent_Player() => current_vent_player;
}
