using UnityEngine;
using DG.Tweening;
public class Player_Animation_Controller : MonoBehaviour
{
    [SerializeField] private Animator meshAnimator;
    [SerializeField] private float movementThreshold = 0.1f;
    [SerializeField] private bool debugAnimations = false;
    
    private Player_Movement playerMovement;
    private CharacterController characterController;
    private Player_Controller playerController;
    
    private bool isWalking = false;
    private bool isRunning = false;
    private bool wasGrounded = true;
    private bool isJumping = false;
    private bool isPickingUp = false;
    
    void Start()
    {
        playerMovement = GetComponent<Player_Movement>();
        characterController = GetComponent<CharacterController>();
        playerController = GetComponent<Player_Controller>();
        
        if (meshAnimator == null)
        {
            Transform meshChild = transform.GetChild(0);
            if (meshChild != null)
            {
                meshAnimator = meshChild.GetComponent<Animator>();
            }
        }
        
        if (meshAnimator == null)
        {
            Debug.LogError($"No Animator found for {gameObject.name}! Please assign the meshAnimator or ensure the first child has an Animator component.");
        }
        
        if (debugAnimations)
        {
            Debug.Log($"Animation Controller initialized for {gameObject.name}");
        }
    }
    
    void Update()
    {
        HandleAnimations();
        CheckForPickupTrigger();
    }
    
    private void HandleAnimations()
{
    if (meshAnimator == null || characterController == null || playerMovement == null) return;

    // Stop everything else if picking up or jumping
    if (isPickingUp || isJumping)
    {
        meshAnimator.SetTrigger("idle");
        isWalking = false;
        isRunning = false;
        return;
    }

    Vector3 velocity = characterController.velocity;
    Vector3 horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);
    float speed = horizontalVelocity.magnitude;
    bool isGrounded = characterController.isGrounded;
    bool isSprinting = playerMovement.IsSprinting();

    // check if player has actual input (so standing still while interacting won't play walk)
    bool hasInput = playerMovement != null && playerMovement.CurrentInputMagnitude() > 0.1f;
    bool shouldMove = hasInput && speed > movementThreshold && isGrounded;

    // Landing
    if (!wasGrounded && isGrounded)
    {
        if (debugAnimations) Debug.Log($"{gameObject.name}: Landing");
        meshAnimator.SetTrigger("land");
    }

    if (shouldMove)
    {
        if (isSprinting)
        {
            if (!isRunning)
            {
                if (debugAnimations) Debug.Log($"{gameObject.name}: Start running");
                meshAnimator.SetTrigger("running");
                isRunning = true;
                isWalking = false;
            }
        }
        else
        {
            if (!isWalking)
            {
                if (debugAnimations) Debug.Log($"{gameObject.name}: Start walking");
                meshAnimator.SetTrigger("walk");
                isWalking = true;
                isRunning = false;
            }
        }
    }
    else
    {
        if (isWalking || isRunning)
        {
            if (debugAnimations) Debug.Log($"{gameObject.name}: Idle");
            meshAnimator.SetTrigger("idle");
            isWalking = false;
            isRunning = false;
        }
    }

    // Transition checks
    if (shouldMove && isWalking && isSprinting)
    {
        meshAnimator.SetTrigger("running");
        isRunning = true;
        isWalking = false;
    }
    else if (shouldMove && isRunning && !isSprinting)
    {
        meshAnimator.SetTrigger("walk");
        isWalking = true;
        isRunning = false;
    }

    wasGrounded = isGrounded;

    // Update animator parameters if they exist
    if (meshAnimator.parameters.Length > 0)
    {
        foreach (AnimatorControllerParameter param in meshAnimator.parameters)
        {
            switch (param.name)
            {
                case "Speed":
                    meshAnimator.SetFloat("Speed", speed);
                    break;
                case "IsGrounded":
                    meshAnimator.SetBool("IsGrounded", isGrounded);
                    break;
                case "IsSprinting":
                    meshAnimator.SetBool("IsSprinting", isSprinting);
                    break;
                case "IsMoving":
                    meshAnimator.SetBool("IsMoving", shouldMove);
                    break;
            }
        }
    }
}
    
    private void CheckForPickupTrigger()
    {
        if (playerController != null && playerController.Get_Interact_Input() && !isPickingUp)
        {
            TriggerPickup();
        }
    }
    
    public void TriggerJump()
    {
        if (meshAnimator != null && !isJumping && !isPickingUp)
        {
            meshAnimator.SetTrigger("jump");
            isJumping = true;
            isWalking = false;
            isRunning = false;
            if (debugAnimations) Debug.Log($"{gameObject.name}: Jump triggered");

            SoundManager.Instance.PlaySound("sfx_player_jump");


            DOTween.Sequence()
                .AppendInterval(0.5f)
                .AppendCallback(() => {
                    isJumping = false;
                    meshAnimator.SetTrigger("idle");
                });
        }
    }
    
    public void TriggerPickup()
    {
        if (meshAnimator != null && !isPickingUp && !isJumping)
        {
            meshAnimator.SetTrigger("pickup");
            isPickingUp = true;
            isWalking = false;
            isRunning = false;
            if (debugAnimations) Debug.Log($"{gameObject.name}: Pickup triggered");
            
            DOTween.Sequence()
                .AppendInterval(1f)
                .AppendCallback(() => {
                    isPickingUp = false;
                    meshAnimator.SetTrigger("idle");
                });
        }
    }
    
    public void TriggerLanding()
    {
        if (meshAnimator != null)
        {
            meshAnimator.SetTrigger("land");
            if (debugAnimations) Debug.Log($"{gameObject.name}: Landing triggered");
        }
    }
    
    public void ForceIdle()
    {
        if (meshAnimator != null)
        {
            meshAnimator.SetTrigger("idle");
            isWalking = false;
            isRunning = false;
            isJumping = false;
            isPickingUp = false;
            if (debugAnimations) Debug.Log($"{gameObject.name}: Forced to idle");
        }
    }
    
    public bool IsCurrentlyWalking() => isWalking;
    public bool IsCurrentlyRunning() => isRunning;
    public bool IsCurrentlyJumping() => isJumping;
    public bool IsCurrentlyPickingUp() => isPickingUp;
    
    void OnDrawGizmos()
    {
        if (debugAnimations && Application.isPlaying)
        {
            Vector3 textPos = transform.position + Vector3.up * 2.5f;
            string state = "Idle";
            
            if (isJumping) state = "Jumping";
            else if (isPickingUp) state = "Picking Up";
            else if (isRunning) state = "Running";
            else if (isWalking) state = "Walking";
        }
    }
}