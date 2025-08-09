using UnityEngine;
using DG.Tweening;

public class Spider_Boss_Player_State : MonoBehaviour
{
    [Header("Player References")]
    [SerializeField] private Player_Controller player1;
    [SerializeField] private Player_Controller player2;
    
    [Header("Player Hit VFX")]
    [SerializeField] private GameObject player1HitVFX; // Child VFX for player 1
    [SerializeField] private GameObject player2HitVFX; // Child VFX for player 2
    
    [Header("Knockback Settings")]
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float knockbackDuration = 0.3f;
    [SerializeField] private bool useCharacterController = true;
    
    [Header("Hit Cooldown")]
    [SerializeField] private float hitCooldown = 1f; // Prevent multiple hits in quick succession
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip player1HitSound;
    [SerializeField] private AudioClip player2HitSound;
    
    [Header("Debug")]
    [SerializeField] private bool debugHits = true;
    
    private float player1LastHitTime = -999f;
    private float player2LastHitTime = -999f;
    
    void Start()
    {
        // Auto-find players if not assigned
        if (player1 == null || player2 == null)
        {
            Player_Controller[] players = FindObjectsByType<Player_Controller>(FindObjectsSortMode.None);
            foreach (var player in players)
            {
                if (player.Get_Player_Type() == Player_Type.Luthe && player1 == null)
                {
                    player1 = player;
                }
                else if (player.Get_Player_Type() == Player_Type.Cherie && player2 == null)
                {
                    player2 = player;
                }
            }
        }
        
        // Auto-find audio source
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // Ensure VFX are initially inactive
        if (player1HitVFX != null) player1HitVFX.SetActive(false);
        if (player2HitVFX != null) player2HitVFX.SetActive(false);
        
        if (debugHits)
        {
            Debug.Log($"[Spider_Boss_Player_State] Initialized with Player1: {(player1 != null ? player1.name : "None")}, Player2: {(player2 != null ? player2.name : "None")}");
        }
    }
    
    public void OnPlayerHit(Player_Controller hitPlayer)
    {
        if (hitPlayer == null) return;
        
        // Check cooldown to prevent spam hits
        float currentTime = Time.time;
        
        if (hitPlayer == player1)
        {
            if (currentTime - player1LastHitTime < hitCooldown) return;
            player1LastHitTime = currentTime;
            
            PlayHitEffects(player1, player1HitVFX, player1HitSound);
            ApplyKnockback(player1);
            
            if (debugHits)
            {
                Debug.Log($"[Spider_Boss_Player_State] Player 1 ({player1.Get_Player_Type()}) hit!");
            }
        }
        else if (hitPlayer == player2)
        {
            if (currentTime - player2LastHitTime < hitCooldown) return;
            player2LastHitTime = currentTime;
            
            PlayHitEffects(player2, player2HitVFX, player2HitSound);
            ApplyKnockback(player2);
            
            if (debugHits)
            {
                Debug.Log($"[Spider_Boss_Player_State] Player 2 ({player2.Get_Player_Type()}) hit!");
            }
        }
        else
        {
            // Handle case where hit player doesn't match our references
            if (debugHits)
            {
                Debug.LogWarning($"[Spider_Boss_Player_State] Unknown player hit: {hitPlayer.name} ({hitPlayer.Get_Player_Type()})");
            }
            
            // Try to match by player type as fallback
            if (hitPlayer.Get_Player_Type() == Player_Type.Luthe && player1 != null)
            {
                OnPlayerHit(player1);
            }
            else if (hitPlayer.Get_Player_Type() == Player_Type.Cherie && player2 != null)
            {
                OnPlayerHit(player2);
            }
        }
    }
    
    private void PlayHitEffects(Player_Controller player, GameObject hitVFX, AudioClip hitSound)
    {
        // Play VFX
        if (hitVFX != null)
        {
            hitVFX.SetActive(true);
            
            // Get all particle systems in the VFX
            ParticleSystem[] particles = hitVFX.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particles)
            {
                ps.Play();
            }
            
            // Auto-disable VFX after a short time (optional)
            DOVirtual.DelayedCall(2f, () => {
                if (hitVFX != null) hitVFX.SetActive(false);
            });
        }
        
        // Play audio
        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }
    }
    
    private void ApplyKnockback(Player_Controller player)
    {
        if (player == null) return;
        
        // Calculate knockback direction (away from spider/center)
        Vector3 knockbackDirection = (player.transform.position - transform.position).normalized;
        knockbackDirection.y = 0; // Keep knockback horizontal
        
        if (knockbackDirection.magnitude < 0.1f)
        {
            // Fallback direction if player is too close to center
            knockbackDirection = player.transform.forward;
        }
        
        Vector3 knockbackVector = knockbackDirection * knockbackForce;
        
        if (useCharacterController)
        {
            // Use CharacterController for knockback
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null)
            {
                StartCoroutine(ApplyCharacterControllerKnockback(cc, knockbackVector));
            }
        }
        else
        {
            // Use transform-based knockback
            Vector3 targetPosition = player.transform.position + knockbackVector;
            player.transform.DOMove(targetPosition, knockbackDuration).SetEase(Ease.OutQuad);
        }
        
        if (debugHits)
        {
            Debug.Log($"[Spider_Boss_Player_State] Applied knockback to {player.Get_Player_Type()}: {knockbackVector}");
        }
    }
    
    private System.Collections.IEnumerator ApplyCharacterControllerKnockback(CharacterController cc, Vector3 knockbackVector)
    {
        float elapsed = 0f;
        Vector3 currentKnockback = knockbackVector;
        
        while (elapsed < knockbackDuration)
        {
            float progress = elapsed / knockbackDuration;
            float easeOut = 1f - (progress * progress); // Ease out curve
            
            Vector3 frameKnockback = currentKnockback * easeOut * Time.deltaTime;
            cc.Move(frameKnockback);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
    
    // Public methods for manual testing
    [ContextMenu("Test Hit Player 1")]
    public void TestHitPlayer1()
    {
        if (player1 != null)
        {
            OnPlayerHit(player1);
        }
    }
    
    [ContextMenu("Test Hit Player 2")]
    public void TestHitPlayer2()
    {
        if (player2 != null)
        {
            OnPlayerHit(player2);
        }
    }
    
    // Getters for other scripts
    public Player_Controller GetPlayer1() => player1;
    public Player_Controller GetPlayer2() => player2;
    
    public void SetPlayers(Player_Controller p1, Player_Controller p2)
    {
        player1 = p1;
        player2 = p2;
        
        if (debugHits)
        {
            Debug.Log($"[Spider_Boss_Player_State] Players set - P1: {(p1 != null ? p1.name : "None")}, P2: {(p2 != null ? p2.name : "None")}");
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw debug lines to players
        if (player1 != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, player1.transform.position);
            Gizmos.DrawWireSphere(player1.transform.position, 0.5f);
        }
        
        if (player2 != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player2.transform.position);
            Gizmos.DrawWireSphere(player2.transform.position, 0.5f);
        }
        
        // Draw knockback range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, knockbackForce);
    }
}