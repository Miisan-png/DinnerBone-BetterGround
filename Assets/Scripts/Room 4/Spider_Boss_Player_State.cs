using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class Spider_Boss_Player_State : Singleton<Spider_Boss_Player_State>
{
    [Header("Player References")]
    [SerializeField] private Player_Controller player1;
    [SerializeField] private Player_Controller player2;
    
    [Header("Respawn Points")]
    [SerializeField] private Transform player1RespawnPoint;
    [SerializeField] private Transform player2RespawnPoint;
    
    [Header("Player Hit VFX")]
    [SerializeField] private GameObject player1HitVFX;
    [SerializeField] private GameObject player2HitVFX;
    
    [Header("Knockback Settings")]
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float knockbackDuration = 0.3f;
    [SerializeField] private bool useCharacterController = true;
    
    [Header("Hit Cooldown")]
    [SerializeField] private float hitCooldown = 1f;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip player1HitSound;
    [SerializeField] private AudioClip player2HitSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip respawnSound;
    
    [Header("Lives System")]
    [SerializeField] private int maxHitsPerPlayer = 2;
    
    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float respawnDelay = 2f;
    
    [Header("Debug")]
    [SerializeField] private bool debugHits = true;
    
    private int player1Hits = 0;
    private int player2Hits = 0;
    private float player1LastHitTime = -999f;
    private float player2LastHitTime = -999f;
    
    private Canvas fadeCanvas;
    private Image fadeImage;

    public int Player1Hits { get => player1Hits; }
    public int Player2Hits { get => player2Hits; }

    void Start()
    {
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
        
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        if (player1HitVFX != null) player1HitVFX.SetActive(false);
        if (player2HitVFX != null) player2HitVFX.SetActive(false);
        
        CreateFadeCanvas();
        
        if (debugHits)
        {
            Debug.Log($"[Spider_Boss_Player_State] Initialized - P1 Hits: {player1Hits}/{maxHitsPerPlayer}, P2 Hits: {player2Hits}/{maxHitsPerPlayer}");
        }
    }
    
    private void CreateFadeCanvas()
    {
        GameObject canvasObj = new GameObject("Fade Canvas");
        fadeCanvas = canvasObj.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 9999;
        
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        GameObject fadeObj = new GameObject("Fade Image");
        fadeObj.transform.SetParent(canvasObj.transform);
        
        fadeImage = fadeObj.AddComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 0);
        
        RectTransform rect = fadeImage.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        fadeCanvas.gameObject.SetActive(false);
    }
    
    public void OnPlayerHit(Player_Controller hitPlayer)
    {
        if (hitPlayer == null) return;
        
        float currentTime = Time.time;
        
        if (hitPlayer == player1)
        {
            if (currentTime - player1LastHitTime < hitCooldown) return;
            player1LastHitTime = currentTime;
            
            PlayHitEffects(player1, player1HitVFX, player1HitSound);
            ApplyKnockback(player1);
            
            player1Hits++;
            
            if (debugHits)
            {
                Debug.Log($"[Spider_Boss_Player_State] Player 1 hit! Hits: {player1Hits}/{maxHitsPerPlayer}");
            }
            
            if (player1Hits > maxHitsPerPlayer)
            {
                HandleBothPlayersDeath();
            }
        }
        else if (hitPlayer == player2)
        {
            if (currentTime - player2LastHitTime < hitCooldown) return;
            player2LastHitTime = currentTime;
            
            PlayHitEffects(player2, player2HitVFX, player2HitSound);
            ApplyKnockback(player2);
            
            player2Hits++;
            
            if (debugHits)
            {
                Debug.Log($"[Spider_Boss_Player_State] Player 2 hit! Hits: {player2Hits}/{maxHitsPerPlayer}");
            }
            
            if (player2Hits > maxHitsPerPlayer)
            {
                HandleBothPlayersDeath();
            }
        }
        else
        {
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
    
    private void HandleBothPlayersDeath()
    {
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        
        if (debugHits)
        {
            Debug.Log($"[Spider_Boss_Player_State] Both players died! Respawning...");
        }
        
        StartCoroutine(RespawnBothPlayers());
    }
    
    private IEnumerator RespawnBothPlayers()
    {
        fadeCanvas.gameObject.SetActive(true);
        
        yield return fadeImage.DOFade(1f, fadeDuration).WaitForCompletion();
        
        yield return new WaitForSeconds(respawnDelay);
        
        if (player1RespawnPoint != null)
        {
            TeleportPlayer(player1, player1RespawnPoint.position, player1RespawnPoint.rotation);
        }
        
        if (player2RespawnPoint != null)
        {
            TeleportPlayer(player2, player2RespawnPoint.position, player2RespawnPoint.rotation);
        }
        
        player1Hits = 0;
        player2Hits = 0;
        
        if (audioSource != null && respawnSound != null)
        {
            audioSource.PlayOneShot(respawnSound);
        }
        
        yield return fadeImage.DOFade(0f, fadeDuration).WaitForCompletion();
        
        fadeCanvas.gameObject.SetActive(false);
        
        if (debugHits)
        {
            Debug.Log($"[Spider_Boss_Player_State] Both players respawned! Hits reset.");
        }
    }
    
    private void TeleportPlayer(Player_Controller player, Vector3 position, Quaternion rotation)
    {
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            player.transform.position = position;
            player.transform.rotation = rotation;
            cc.enabled = true;
        }
        else
        {
            player.transform.position = position;
            player.transform.rotation = rotation;
        }
    }
    
    private void PlayHitEffects(Player_Controller player, GameObject hitVFX, AudioClip hitSound)
    {
        if (hitVFX != null)
        {
            hitVFX.SetActive(true);

            SoundManager.Instance.PlaySound("sfx_player_damage");
            
            ParticleSystem[] particles = hitVFX.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particles)
            {
                ps.Play();
            }
            
            DOVirtual.DelayedCall(2f, () => {
                if (hitVFX != null) hitVFX.SetActive(false);
            });
        }
        
        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }
    }
    
    private void ApplyKnockback(Player_Controller player)
    {
        if (player == null) return;
        
        Vector3 knockbackDirection = (player.transform.position - transform.position).normalized;
        knockbackDirection.y = 0;
        
        if (knockbackDirection.magnitude < 0.1f)
        {
            knockbackDirection = player.transform.forward;
        }
        
        Vector3 knockbackVector = knockbackDirection * knockbackForce;
        
        if (useCharacterController)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null)
            {
                StartCoroutine(ApplyCharacterControllerKnockback(cc, knockbackVector));
            }
        }
        else
        {
            Vector3 targetPosition = player.transform.position + knockbackVector;
            player.transform.DOMove(targetPosition, knockbackDuration).SetEase(Ease.OutQuad);
        }
    }
    
    private System.Collections.IEnumerator ApplyCharacterControllerKnockback(CharacterController cc, Vector3 knockbackVector)
    {
        float elapsed = 0f;
        Vector3 currentKnockback = knockbackVector;
        
        while (elapsed < knockbackDuration)
        {
            float progress = elapsed / knockbackDuration;
            float easeOut = 1f - (progress * progress);
            
            Vector3 frameKnockback = currentKnockback * easeOut * Time.deltaTime;
            cc.Move(frameKnockback);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
    
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
    
    public Player_Controller GetPlayer1() => player1;
    public Player_Controller GetPlayer2() => player2;
    public int GetPlayer1Hits() => player1Hits;
    public int GetPlayer2Hits() => player2Hits;
    public int GetMaxHits() => maxHitsPerPlayer;
    
    void OnDrawGizmosSelected()
    {
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
        
        if (player1RespawnPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(player1RespawnPoint.position, Vector3.one);
        }
        
        if (player2RespawnPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(player2RespawnPoint.position, Vector3.one);
        }
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, knockbackForce);
    }
}