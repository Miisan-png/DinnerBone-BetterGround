using UnityEngine;

public class Death_Trigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [SerializeField] private bool killBothPlayers = false;
    [SerializeField] private bool oneTimeOnly = false;
    [SerializeField] private LayerMask playerLayerMask = -1;
    [SerializeField] private bool debugTrigger = false;
    
    private bool hasTriggered = false;
    
    void Start()
    {
        Collider triggerCollider = GetComponent<Collider>();
        if (triggerCollider == null)
        {
            triggerCollider = gameObject.AddComponent<BoxCollider>();
        }
        triggerCollider.isTrigger = true;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && oneTimeOnly) return;
        
        if (!IsInLayerMask(other.gameObject, playerLayerMask)) return;
        
        Player_Controller player = other.GetComponent<Player_Controller>();
        if (player == null) return;
        
        if (Player_State_Manager.Instance == null)
        {
            if (debugTrigger) Debug.LogError("Player_State_Manager not found!");
            return;
        }
        
        if (debugTrigger) Debug.Log($"Death trigger activated by {player.Get_Player_Type()}");
        
        if (killBothPlayers)
        {
            Player_Controller[] allPlayers = FindObjectsByType<Player_Controller>(FindObjectsSortMode.None);
            foreach (Player_Controller p in allPlayers)
            {
                Player_State_Manager.Instance.KillPlayer(p);
            }
        }
        else
        {
            Player_State_Manager.Instance.KillPlayer(player);
        }
        
        if (oneTimeOnly)
            hasTriggered = true;
    }
    
    private bool IsInLayerMask(GameObject obj, LayerMask layerMask)
    {
        return (layerMask.value & (1 << obj.layer)) != 0;
    }
    
    public void ResetTrigger()
    {
        hasTriggered = false;
    }
    
    void OnDrawGizmos()
    {
        Collider triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
        {
            Gizmos.color = killBothPlayers ? Color.red : Color.magenta;
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
    }
}