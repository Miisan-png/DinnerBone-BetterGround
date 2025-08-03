using UnityEngine;

public class PlayerDust_Trail : MonoBehaviour
{
   [SerializeField] private ParticleSystem dustTrail;
   [SerializeField] private float minSpeedToEmit = 0.1f;
   
   private CharacterController controller;
   
   void Start()
   {
       controller = GetComponent<CharacterController>();
       if (dustTrail != null)
       {
           var emission = dustTrail.emission;
           emission.enabled = false;
       }
   }
   
   void Update()
   {
       if (controller == null || dustTrail == null) return;
       
       Vector3 horizontalVelocity = new Vector3(controller.velocity.x, 0, controller.velocity.z);
       bool isMoving = horizontalVelocity.magnitude > minSpeedToEmit && controller.isGrounded;
       
       var emission = dustTrail.emission;
       emission.enabled = isMoving;
   }
}