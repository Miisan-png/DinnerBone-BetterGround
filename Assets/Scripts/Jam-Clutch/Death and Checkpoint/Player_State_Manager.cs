using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

public class Player_State_Manager : MonoBehaviour
{
    public static Action<Player_Controller> OnPlayerRespawn;

    private static Player_State_Manager instance;
    public static Player_State_Manager Instance => instance;
    
    [Header("Death Settings")]
    [SerializeField] private float deathShakeDuration = 0.5f;
    [SerializeField] private float deathShakeStrength = 1f;
    [SerializeField] private float respawnDelay = 1f;
    
    [Header("Checkpoints")]
    [SerializeField] private Vector3 player1DefaultSpawn = Vector3.zero;
    [SerializeField] private Vector3 player2DefaultSpawn = new Vector3(2f, 0f, 0f);
    
    private Vector3 player1Checkpoint;
    private Vector3 player2Checkpoint;
    private Camera_Manager cameraManager;
    private bool player1IsDead = false;
    private bool player2IsDead = false;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            player1Checkpoint = player1DefaultSpawn;
            player2Checkpoint = player2DefaultSpawn;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        cameraManager = FindObjectOfType<Camera_Manager>();
    }
    
    public void KillPlayer(Player_Controller player)
    {
        if (player.Get_Player_Type() == Player_Type.Luthe && player1IsDead) return;
        if (player.Get_Player_Type() == Player_Type.Cherie && player2IsDead) return;
        
        SoundManager.Instance.PlaySound("sfx_player_death");
        StartCoroutine(HandlePlayerDeath(player));
    }
    
    private IEnumerator HandlePlayerDeath(Player_Controller player)
    {
        Player_Type playerType = player.Get_Player_Type();
        
        if (playerType == Player_Type.Luthe)
            player1IsDead = true;
        else
            player2IsDead = true;
        
        DisablePlayer(player);
        ShakePlayerCamera(player);
        
        yield return new WaitForSeconds(respawnDelay);
        
        RespawnPlayer(player);
        OnPlayerRespawn?.Invoke(player);
        SoundManager.Instance.PlaySound("sfx_player_respawn");

        if (playerType == Player_Type.Luthe)
            player1IsDead = false;
        else
            player2IsDead = false;
    }
    
    private void DisablePlayer(Player_Controller player)
    {
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        
        Player_Movement movement = player.GetComponent<Player_Movement>();
        if (movement != null) movement.enabled = false;
        
        Player_Interaction interaction = player.GetComponent<Player_Interaction>();
        if (interaction != null) interaction.enabled = false;
        
        Renderer[] renderers = player.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = false;
        }
    }
    
    private void EnablePlayer(Player_Controller player)
    {
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = true;
        
        Player_Movement movement = player.GetComponent<Player_Movement>();
        if (movement != null) movement.enabled = true;
        
        Player_Interaction interaction = player.GetComponent<Player_Interaction>();
        if (interaction != null) interaction.enabled = true;
        
        Renderer[] renderers = player.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = true;
        }
    }
    
    private void ShakePlayerCamera(Player_Controller player)
    {
        if (cameraManager == null) return;
        
        Camera targetCamera = null;
        
        if (cameraManager.Get_Current_Mode() == Camera_Mode.Split_Screen)
        {
            if (player.Get_Player_Type() == Player_Type.Luthe)
                targetCamera = cameraManager.Get_Split_Camera_1();
            else
                targetCamera = cameraManager.Get_Split_Camera_2();
        }
        else
        {
            targetCamera = Camera.main;
        }
        
        if (targetCamera != null)
        {
            targetCamera.DOShakePosition(deathShakeDuration, deathShakeStrength);
        }
    }
    
    private void RespawnPlayer(Player_Controller player)
    {
        Vector3 respawnPosition = GetPlayerCheckpoint(player.Get_Player_Type());
        
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            player.transform.position = respawnPosition;
            cc.enabled = true;
        }
        else
        {
            player.transform.position = respawnPosition;
        }
        
        EnablePlayer(player);
        PlayRespawnVFX(player);
    }
    
    private void PlayRespawnVFX(Player_Controller player)
    {
        string vfxName = player.Get_Player_Type() == Player_Type.Luthe ? "LutherRespawnVFX" : "CherieRespawnVFX";
        
        Transform vfxChild = player.transform.Find(vfxName);
        if (vfxChild == null)
        {
            foreach (Transform child in player.transform)
            {
                ParticleSystem ps = child.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    vfxChild = child;
                    break;
                }
            }
        }
        
        if (vfxChild != null)
        {
            ParticleSystem[] particles = vfxChild.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in particles)
            {
                ps.Play();
            }
        }
    }
    
    public void SetCheckpoint(Player_Type playerType, Vector3 position)
    {
        if (playerType == Player_Type.Luthe)
            player1Checkpoint = position;
        else
            player2Checkpoint = position;
    }
    
    public Vector3 GetPlayerCheckpoint(Player_Type playerType)
    {
        return playerType == Player_Type.Luthe ? player1Checkpoint : player2Checkpoint;
    }
    
    public bool IsPlayerDead(Player_Type playerType)
    {
        return playerType == Player_Type.Luthe ? player1IsDead : player2IsDead;
    }
}