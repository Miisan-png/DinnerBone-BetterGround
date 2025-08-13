using UnityEngine;

public class Hit_Trigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [SerializeField] private bool hitBothPlayers = false;
    [SerializeField] private bool oneTimeOnly = false;
    [SerializeField] private LayerMask playerLayerMask = -1;
    [SerializeField] private bool debugTrigger = false;

    [Header("Hit System")]
    [SerializeField] private bool useSpiderBossSystem = true; // Toggle between the two systems
    
    private bool hasTriggered = false;
    private Spider_Boss_Player_State playerStateManager;
    
    void Start()
    {
        Collider triggerCollider = GetComponent<Collider>();
        if (triggerCollider == null)
        {
            triggerCollider = gameObject.AddComponent<BoxCollider>();
        }
        triggerCollider.isTrigger = true;

        // Find the player state manager (similar to SpiderLegEvents)
        if (useSpiderBossSystem)
        {
            playerStateManager = FindObjectOfType<Spider_Boss_Player_State>();
            if (playerStateManager == null)
            {
                Debug.LogWarning("[Hit_Trigger] No Spider_Boss_Player_State found in scene. Falling back to Player_State_Manager system.");
                useSpiderBossSystem = false;
            }
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && oneTimeOnly) return;
        
        if (!IsInLayerMask(other.gameObject, playerLayerMask)) return;
        
        Player_Controller player = other.GetComponent<Player_Controller>();
        if (player == null) return;
        
        if (debugTrigger) Debug.Log($"Hit trigger activated by {player.Get_Player_Type()}");
        
        if (hitBothPlayers)
        {
            Player_Controller[] allPlayers = FindObjectsByType<Player_Controller>(FindObjectsSortMode.None);
            foreach (Player_Controller p in allPlayers)
            {
                TriggerPlayerHit(p);
            }
        }
        else
        {
            TriggerPlayerHit(player);
        }
        
        if (oneTimeOnly)
            hasTriggered = true;
    }

    private void TriggerPlayerHit(Player_Controller player)
    {
        if (useSpiderBossSystem && playerStateManager != null)
        {
            // Use the same system as SpiderLegEvents
            playerStateManager.OnPlayerHit(player);
            if (debugTrigger) Debug.Log($"Used Spider Boss system to hit {player.Get_Player_Type()}");
        }
        else
        {
            // Fall back to the original kill system
            if (Player_State_Manager.Instance == null)
            {
                if (debugTrigger) Debug.LogError("Player_State_Manager not found!");
                return;
            }
            
            Player_State_Manager.Instance.KillPlayer(player);
            if (debugTrigger) Debug.Log($"Used kill system on {player.Get_Player_Type()}");
        }
    }
    
    private bool IsInLayerMask(GameObject obj, LayerMask layerMask)
    {
        return (layerMask.value & (1 << obj.layer)) != 0;
    }
    
    public void ResetTrigger()
    {
        hasTriggered = false;
    }

    // Public methods to control the hit system at runtime
    public void SetUseSpiderBossSystem(bool useSpiderSystem)
    {
        useSpiderBossSystem = useSpiderSystem;
        if (useSpiderSystem && playerStateManager == null)
        {
            playerStateManager = FindObjectOfType<Spider_Boss_Player_State>();
            if (playerStateManager == null)
            {
                Debug.LogWarning("[Hit_Trigger] No Spider_Boss_Player_State found. Cannot use Spider Boss system.");
                useSpiderBossSystem = false;
            }
        }
    }

    public void SetHitBothPlayers(bool hitBoth)
    {
        hitBothPlayers = hitBoth;
    }
    
    void OnDrawGizmos()
    {
        Collider triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
        {
            // Color coding: Red for kill system, Blue for spider boss system
            if (useSpiderBossSystem)
            {
                Gizmos.color = hitBothPlayers ? Color.blue : Color.cyan;
            }
            else
            {
                Gizmos.color = hitBothPlayers ? Color.red : Color.magenta;
            }
            
            if (hasTriggered && oneTimeOnly)
                Gizmos.color *= 0.5f;
                
            Gizmos.matrix = transform.localToWorldMatrix;
            
            if (triggerCollider is BoxCollider box)
            {
                Gizmos.DrawWireCube(box.center, box.size);
            }
            else if (triggerCollider is SphereCollider sphere)
            {
                Gizmos.DrawWireSphere(sphere.center, sphere.radius);
            }
            else if (triggerCollider is CapsuleCollider capsule)
            {
                Gizmos.DrawWireCube(capsule.center, new Vector3(capsule.radius * 2, capsule.height, capsule.radius * 2));
            }
        }
        
        if (debugTrigger)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.3f);
        }

        // Visual indicator for which system is being used
        if (useSpiderBossSystem)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 2.5f, 0.1f);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 2.5f, 0.1f);
        }
    }
}