using UnityEngine;

public class Player_Animation_Controller : MonoBehaviour
{
    [SerializeField] private Animator meshAnimator;
    [SerializeField] private float movementThreshold = 0.1f;
    
    private Player_Movement playerMovement;
    private CharacterController characterController;
    private bool isWalking = false;
    
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
    }
    
    void Update()
    {
        HandleAnimations();
    }
    
    private void HandleAnimations()
    {
        if (meshAnimator == null || characterController == null) return;
        
        Vector3 velocity = characterController.velocity;
        Vector3 horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);
        float speed = horizontalVelocity.magnitude;
        
        bool shouldWalk = speed > movementThreshold && characterController.isGrounded;
        
        if (shouldWalk && !isWalking)
        {
            isWalking = true;
            meshAnimator.SetTrigger("walk");
        }
        else if (!shouldWalk && isWalking)
        {
            isWalking = false;
            meshAnimator.SetTrigger("idle");
        }
    }
}