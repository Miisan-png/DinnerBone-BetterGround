using UnityEngine;

public class splash_controller : MonoBehaviour 
{
   public ParticleSystem[] particleSystems;
   public float shootForce = 10f;
   public Transform shootDirection;
   public bool alwaysShoot = false;
   
   void Update()
   {
       if (Input.GetKey(KeyCode.E) || alwaysShoot)
       {
           ShootParticles();
       }
   }
   
   void ShootParticles()
   {
       Vector3 direction = shootDirection ? shootDirection.forward : transform.forward;
       
       foreach (ParticleSystem ps in particleSystems)
       {
           if (ps != null)
           {
               var main = ps.main;
               main.startSpeed = shootForce;
               
               var shape = ps.shape;
               shape.enabled = true;
               shape.shapeType = ParticleSystemShapeType.Cone;
               shape.angle = 5f;
               shape.rotation = Quaternion.LookRotation(direction).eulerAngles;
               
               var velocityOverLifetime = ps.velocityOverLifetime;
               velocityOverLifetime.enabled = true;
               velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
               velocityOverLifetime.x = direction.x * shootForce;
               velocityOverLifetime.y = direction.y * shootForce;
               velocityOverLifetime.z = direction.z * shootForce;
               
               ps.Emit(5);
           }
       }
   }
}