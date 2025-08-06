using UnityEngine;

public class Checkpoint_Trigger : MonoBehaviour
{
    [Header("Checkpoint Settings")]
    [SerializeField] private bool setBothPlayers = true;
    [SerializeField] private bool oneTimeOnly = true;
    [SerializeField] private LayerMask playerLayerMask = -1;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private bool requireBothPlayers = false;
    [SerializeField] private bool debugCheckpoint = false;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject checkpointVFXPrefab;
    [SerializeField] private AudioSource checkpointSound;
    
    private bool hasTriggered = false;
    private bool player1InTrigger = false;
    private bool player2InTrigger = false;
    
    void Start()
    {
        Collider triggerCollider = GetComponent<Collider>();
        if (triggerCollider == null)
        {
            triggerCollider = gameObject.AddComponent<BoxCollider>();
        }
        triggerCollider.isTrigger = true;
        
        if (spawnPoint == null)
        {
            GameObject spawnObj = new GameObject("Spawn Point");
            spawnObj.transform.SetParent(transform);
            spawnObj.transform.localPosition = Vector3.zero;
            spawnPoint = spawnObj.transform;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && oneTimeOnly) return;
        
        if (!IsInLayerMask(other.gameObject, playerLayerMask)) return;
        
        Player_Controller player = other.GetComponent<Player_Controller>();
        if (player == null) return;
        
        if (player.Get_Player_Type() == Player_Type.Luthe)
        {
            player1InTrigger = true;
            if (debugCheckpoint) Debug.Log("Player 1 entered checkpoint");
        }
        else if (player.Get_Player_Type() == Player_Type.Cherie)
        {
            player2InTrigger = true;
            if (debugCheckpoint) Debug.Log("Player 2 entered checkpoint");
        }
        
        CheckActivateCheckpoint();
    }
    
    void OnTriggerExit(Collider other)
    {
        Player_Controller player = other.GetComponent<Player_Controller>();
        if (player == null) return;
        
        if (player.Get_Player_Type() == Player_Type.Luthe)
        {
            player1InTrigger = false;
        }
        else if (player.Get_Player_Type() == Player_Type.Cherie)
        {
            player2InTrigger = false;
        }
    }
    
    private void CheckActivateCheckpoint()
    {
        bool shouldActivate = false;
        
        if (requireBothPlayers)
        {
            shouldActivate = player1InTrigger && player2InTrigger;
        }
        else
        {
            shouldActivate = player1InTrigger || player2InTrigger;
        }
        
        if (shouldActivate)
        {
            ActivateCheckpoint();
        }
    }
    
    private void ActivateCheckpoint()
    {
        if (Player_State_Manager.Instance == null)
        {
            if (debugCheckpoint) Debug.LogError("Player_State_Manager not found!");
            return;
        }
        
        Vector3 checkpointPosition = spawnPoint.position;
        
        if (setBothPlayers)
        {
            Player_State_Manager.Instance.SetCheckpoint(Player_Type.Luthe, checkpointPosition);
            Player_State_Manager.Instance.SetCheckpoint(Player_Type.Cherie, checkpointPosition + Vector3.right * 2f);
        }
        else
        {
            if (player1InTrigger)
                Player_State_Manager.Instance.SetCheckpoint(Player_Type.Luthe, checkpointPosition);
            if (player2InTrigger)
                Player_State_Manager.Instance.SetCheckpoint(Player_Type.Cherie, checkpointPosition + Vector3.right * 2f);
        }
        
        PlayCheckpointEffects();
        
        if (debugCheckpoint) Debug.Log($"Checkpoint activated at {checkpointPosition}");
        
        if (oneTimeOnly)
            hasTriggered = true;
    }
    
    private void PlayCheckpointEffects()
    {
        if (checkpointVFXPrefab != null)
        {
            GameObject vfx = Instantiate(checkpointVFXPrefab, spawnPoint.position, Quaternion.identity);
            Destroy(vfx, 3f);
        }
        
        if (checkpointSound != null)
        {
            checkpointSound.Play();
        }
    }
    
    private bool IsInLayerMask(GameObject obj, LayerMask layerMask)
    {
        return (layerMask.value & (1 << obj.layer)) != 0;
    }
    
    public void ResetCheckpoint()
    {
        hasTriggered = false;
        player1InTrigger = false;
        player2InTrigger = false;
    }
    
    void OnDrawGizmos()
    {
        Collider triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
        {
            Gizmos.color = hasTriggered ? Color.green : Color.cyan;
            if (requireBothPlayers && !(player1InTrigger && player2InTrigger))
                Gizmos.color *= 0.7f;
                
            Gizmos.matrix = transform.localToWorldMatrix;
            
            if (triggerCollider is BoxCollider box)
            {
                Gizmos.DrawWireCube(box.center, box.size);
            }
            else if (triggerCollider is SphereCollider sphere)
            {
                Gizmos.DrawWireSphere(sphere.center, sphere.radius);
            }
        }
        
        if (spawnPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
            Gizmos.DrawLine(transform.position, spawnPoint.position);
            
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(spawnPoint.position + Vector3.right * 2f, 0.3f);
        }
        
        if (debugCheckpoint)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 2f, Vector3.one * 0.3f);
        }
    }
}