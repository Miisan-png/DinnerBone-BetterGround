using UnityEngine;

public class Player_Indicator : MonoBehaviour
{
    [SerializeField] private GameObject indicatorRing;
    [SerializeField] private Material ringMaterial;
    [SerializeField] private float fadeSpeed = 5f;
    [SerializeField] private float movementThreshold = 0.1f;
    [SerializeField] private float idleDelay = 1f;
    
    private CharacterController characterController;
    private Player_Movement playerMovement;
    private Renderer ringRenderer;
    private bool isJumping = false;
    private bool isMoving = false;
    private float currentAlpha = 1f;
    private float targetAlpha = 1f;
    private float idleTimer = 0f;
    
    // For tracking movement when using Player_Movement script
    private Vector3 lastPosition;
    
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerMovement = GetComponent<Player_Movement>();
        
        if (indicatorRing != null)
        {
            ringRenderer = indicatorRing.GetComponent<Renderer>();
            
            if (ringMaterial == null && ringRenderer != null)
            {
                ringMaterial = ringRenderer.material;
            }
        }
        
        if (ringMaterial != null)
        {
            ringMaterial = new Material(ringMaterial);
            if (ringRenderer != null)
            {
                ringRenderer.material = ringMaterial;
            }
        }
        
        // Initialize last position for movement detection
        lastPosition = transform.position;
    }
    
    void Update()
    {
        CheckJumpingStatus();
        CheckMovementStatus();
        UpdateIndicatorVisibility();
    }
    
    private void CheckJumpingStatus()
    {
        if (characterController != null)
        {
            // Simple check: if we have significant upward velocity, we're jumping
            Vector3 velocity = characterController.velocity;
            isJumping = velocity.y > 0.5f || !characterController.isGrounded;
        }
        else
        {
            // Fallback: assume not jumping if no CharacterController
            isJumping = false;
        }
    }
    
    private void CheckMovementStatus()
    {
        // Use Player_Movement script if available, otherwise fall back to CharacterController
        if (playerMovement != null)
        {
            // Check if player is giving movement input or is sprinting
            // We can detect movement by checking position change over time
            Vector3 currentPosition = transform.position;
            Vector3 horizontalMovement = new Vector3(
                currentPosition.x - lastPosition.x, 
                0, 
                currentPosition.z - lastPosition.z
            );
            
            float movementSpeed = horizontalMovement.magnitude / Time.deltaTime;
            
            bool wasMoving = isMoving;
            isMoving = movementSpeed > movementThreshold;
            
            // Update idle timer
            if (isMoving)
            {
                idleTimer = 0f;
            }
            else if (wasMoving && !isMoving)
            {
                idleTimer = 0f;
            }
            else if (!isMoving)
            {
                idleTimer += Time.deltaTime;
            }
            
            // Update last position for next frame
            lastPosition = currentPosition;
        }
        else if (characterController != null)
        {
            // Fallback to original CharacterController method
            Vector3 velocity = characterController.velocity;
            Vector3 horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);
            float speed = horizontalVelocity.magnitude;
            
            bool wasMoving = isMoving;
            isMoving = speed > movementThreshold;
            
            if (isMoving)
            {
                idleTimer = 0f;
            }
            else if (wasMoving && !isMoving)
            {
                idleTimer = 0f;
            }
            else if (!isMoving)
            {
                idleTimer += Time.deltaTime;
            }
        }
    }
    
    private void UpdateIndicatorVisibility()
    {
        // Show indicator only when NOT jumping and (moving or recently moved)
        bool shouldShow = !isJumping && (isMoving || idleTimer < idleDelay);
        targetAlpha = shouldShow ? 1f : 0f;
        
        currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);
        
        if (ringMaterial != null)
        {
            ringMaterial.SetFloat("_Alpha", currentAlpha);
        }
        
        if (indicatorRing != null)
        {
            indicatorRing.SetActive(currentAlpha > 0.01f);
        }
    }
    
    public void SetRingColor(Color baseColor, Color glowColor)
    {
        if (ringMaterial != null)
        {
            ringMaterial.SetColor("_BaseColor", baseColor);
            ringMaterial.SetColor("_GlowColor", glowColor);
        }
    }
    
    public void SetGlowIntensity(float intensity)
    {
        if (ringMaterial != null)
        {
            ringMaterial.SetFloat("_GlowIntensity", intensity);
        }
    }
    
    // Public method to get movement info (optional, for debugging)
    public bool IsPlayerMoving()
    {
        return isMoving;
    }
    
    // Public method to check if using Player_Movement script
    public bool IsUsingPlayerMovementScript()
    {
        return playerMovement != null;
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = !isJumping ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.5f, 0.2f);
        
        if (isMoving)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
        
        // Visual indicator if using Player_Movement script
        if (playerMovement != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 2f, Vector3.one * 0.2f);
        }
    }
}