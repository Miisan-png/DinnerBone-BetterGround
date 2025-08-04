using UnityEngine;

public class Player_Animation_Controller : MonoBehaviour
{
    [SerializeField] private Animator meshAnimator;
    [SerializeField] private float movementThreshold = 0.1f;
    [SerializeField] private bool debugAnimations = false;
    
    private Player_Movement playerMovement;
    private CharacterController characterController;
    
    // Animation states
    private bool isWalking = false;
    private bool isRunning = false;
    private bool wasGrounded = true;
    
    void Start()
    {
        playerMovement = GetComponent<Player_Movement>();
        characterController = GetComponent<CharacterController>();
        
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
    }
    
    private void HandleAnimations()
    {
        if (meshAnimator == null || characterController == null || playerMovement == null) return;
        
        // Get current state info
        Vector3 velocity = characterController.velocity;
        Vector3 horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);
        float speed = horizontalVelocity.magnitude;
        bool isGrounded = characterController.isGrounded;
        bool isSprinting = playerMovement.IsSprinting();
        
        // Determine if player should be moving
        bool shouldMove = speed > movementThreshold && isGrounded;
        
        // Handle landing (if you have a landing animation)
        if (!wasGrounded && isGrounded)
        {
            if (debugAnimations) Debug.Log($"{gameObject.name}: Landing");
            // You can add a landing trigger here if you have one
            // meshAnimator.SetTrigger("land");
        }
        
        // Handle animation state transitions
        if (shouldMove)
        {
            if (isSprinting)
            {
                // Transition to running
                if (!isRunning)
                {
                    if (debugAnimations) Debug.Log($"{gameObject.name}: Starting to run");
                    meshAnimator.SetTrigger("running");
                    isRunning = true;
                    isWalking = false;
                }
            }
            else
            {
                // Transition to walking
                if (!isWalking)
                {
                    if (debugAnimations) Debug.Log($"{gameObject.name}: Starting to walk");
                    meshAnimator.SetTrigger("walk");
                    isWalking = true;
                    isRunning = false;
                }
            }
        }
        else
        {
            // Transition to idle
            if (isWalking || isRunning)
            {
                if (debugAnimations) Debug.Log($"{gameObject.name}: Going to idle");
                meshAnimator.SetTrigger("idle");
                isWalking = false;
                isRunning = false;
            }
        }
        
        // Handle sprint state changes while moving
        if (shouldMove && isWalking && isSprinting)
        {
            if (debugAnimations) Debug.Log($"{gameObject.name}: Transitioning from walk to run");
            meshAnimator.SetTrigger("running");
            isRunning = true;
            isWalking = false;
        }
        else if (shouldMove && isRunning && !isSprinting)
        {
            if (debugAnimations) Debug.Log($"{gameObject.name}: Transitioning from run to walk");
            meshAnimator.SetTrigger("walk");
            isWalking = true;
            isRunning = false;
        }
        
        wasGrounded = isGrounded;
        
        // Optional: Set animator parameters for more complex state machines
        if (meshAnimator.parameters.Length > 0)
        {
            // Check if these parameters exist before setting them
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
    
    // Public methods for external animation triggers
    public void TriggerJump()
    {
        if (meshAnimator != null)
        {
            meshAnimator.SetTrigger("jump");
            if (debugAnimations) Debug.Log($"{gameObject.name}: Jump triggered");
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
            if (debugAnimations) Debug.Log($"{gameObject.name}: Forced to idle");
        }
    }
    
    // Getters for debugging
    public bool IsCurrentlyWalking() => isWalking;
    public bool IsCurrentlyRunning() => isRunning;
    
    void OnDrawGizmos()
    {
        if (debugAnimations && Application.isPlaying)
        {
            Vector3 textPos = transform.position + Vector3.up * 2.5f;
            string state = "Idle";
            
            if (isRunning) state = "Running";
            else if (isWalking) state = "Walking";
            
            // This would show in scene view if you have a custom editor
            // UnityEditor.Handles.Label(textPos, state);
        }
    }
}