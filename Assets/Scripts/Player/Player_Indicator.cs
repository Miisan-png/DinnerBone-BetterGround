using UnityEngine;

public class Player_Indicator : MonoBehaviour
{
    [SerializeField] private GameObject indicatorRing;
    [SerializeField] private Material ringMaterial;
    [SerializeField] private float fadeSpeed = 5f;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private LayerMask groundMask = 1;
    [SerializeField] private float jumpThreshold = 0.5f;
    [SerializeField] private float movementThreshold = 0.1f;
    [SerializeField] private float idleDelay = 1f;
    
    private CharacterController characterController;
    private Renderer ringRenderer;
    private bool isGrounded = true;
    private bool isMoving = false;
    private float currentAlpha = 1f;
    private float targetAlpha = 1f;
    private float idleTimer = 0f;
    
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        
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
    }
    
    void Update()
    {
        CheckGroundStatus();
        CheckMovementStatus();
        UpdateIndicatorVisibility();
    }
    
    private void CheckGroundStatus()
    {
        if (characterController != null)
        {
            isGrounded = characterController.isGrounded;
            
            Vector3 velocity = characterController.velocity;
            if (transform.position.y > jumpThreshold || velocity.y > 0.1f)
            {
                isGrounded = false;
            }
        }
        else
        {
            RaycastHit hit;
            Vector3 rayStart = transform.position + Vector3.up * 0.1f;
            isGrounded = Physics.Raycast(rayStart, Vector3.down, out hit, groundCheckDistance + 0.1f, groundMask);
        }
    }
    
    private void CheckMovementStatus()
    {
        if (characterController != null)
        {
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
        bool shouldShow = isGrounded && (isMoving || idleTimer < idleDelay);
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
    
    void OnDrawGizmos()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;
        Gizmos.DrawRay(rayStart, Vector3.down * (groundCheckDistance + 0.1f));
        
        if (isMoving)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}