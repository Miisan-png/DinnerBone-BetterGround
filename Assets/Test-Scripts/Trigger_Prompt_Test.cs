using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Trigger_Prompt_Test : MonoBehaviour
{
   [SerializeField] private RawImage inputIcon;
   [SerializeField] private float detectionRadius = 3f;
   [SerializeField] private float maxScale = 1f;
   [SerializeField] private float minScale = 0f;
   [SerializeField] private float animationDuration = 0.3f;
   
   private Transform playerInRange;
   private Camera currentCamera;
   private bool isVisible = false;
   
   void Start()
   {
       if (inputIcon != null)
           inputIcon.transform.localScale = Vector3.zero;
   }
   
   void Update()
   {
       FindNearestPlayer();
       UpdateCameraReference();
       HandleIconVisibility();
       FaceCamera();
   }
   
   void FindNearestPlayer()
   {
       GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
       GameObject player1 = GameObject.FindWithTag("Player1");
       GameObject player2 = GameObject.FindWithTag("Player2");
       
       Transform closestPlayer = null;
       float closestDistance = detectionRadius;
       
       foreach (GameObject player in players)
       {
           if (player != null)
           {
               float distance = Vector3.Distance(transform.position, player.transform.position);
               if (distance < closestDistance)
               {
                   closestDistance = distance;
                   closestPlayer = player.transform;
               }
           }
       }
       
       if (player1 != null)
       {
           float distance = Vector3.Distance(transform.position, player1.transform.position);
           if (distance < closestDistance)
           {
               closestDistance = distance;
               closestPlayer = player1.transform;
           }
       }
       
       if (player2 != null)
       {
           float distance = Vector3.Distance(transform.position, player2.transform.position);
           if (distance < closestDistance)
           {
               closestDistance = distance;
               closestPlayer = player2.transform;
           }
       }
       
       playerInRange = closestPlayer;
   }
   
   void UpdateCameraReference()
   {
       Camera[] cameras = Camera.allCameras;
       Camera closestCamera = null;
       float closestDistance = Mathf.Infinity;
       
       foreach (Camera cam in cameras)
       {
           if (cam.enabled && cam.gameObject.activeInHierarchy)
           {
               float distance = Vector3.Distance(transform.position, cam.transform.position);
               if (distance < closestDistance)
               {
                   closestDistance = distance;
                   closestCamera = cam;
               }
           }
       }
       
       currentCamera = closestCamera;
   }
   
   void HandleIconVisibility()
   {
       if (inputIcon == null) return;
       
       if (playerInRange != null)
       {
           float distance = Vector3.Distance(transform.position, playerInRange.position);
           float normalizedDistance = Mathf.Clamp01(distance / detectionRadius);
           float targetScale = Mathf.Lerp(maxScale, minScale, normalizedDistance);
           
           if (!isVisible)
           {
               isVisible = true;
               inputIcon.transform.DOScale(targetScale, animationDuration).SetEase(Ease.OutBack);
           }
           else
           {
               inputIcon.transform.DOScale(targetScale, animationDuration * 0.5f);
           }
       }
       else
       {
           if (isVisible)
           {
               isVisible = false;
               inputIcon.transform.DOScale(0f, animationDuration).SetEase(Ease.InBack);
           }
       }
   }
   
   void FaceCamera()
   {
       if (inputIcon != null && currentCamera != null)
       {
           Vector3 directionToCamera = currentCamera.transform.position - inputIcon.transform.position;
           inputIcon.transform.rotation = Quaternion.LookRotation(-directionToCamera);
       }
   }
   
   void OnTriggerEnter(Collider other)
   {
       if (other.CompareTag("Player") || other.CompareTag("Player1") || other.CompareTag("Player2"))
       {
           playerInRange = other.transform;
       }
   }
   
   void OnTriggerExit(Collider other)
   {
       if (other.transform == playerInRange)
       {
           playerInRange = null;
       }
   }
}