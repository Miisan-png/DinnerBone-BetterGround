using UnityEngine;

public class Fan_Drag_Player : MonoBehaviour
{
    [SerializeField] private float drag_force = 5f;
    [SerializeField] private Transform fan_object;
    [SerializeField] private LayerMask player_layer = -1;
    
    void OnTriggerStay(Collider other)
    {
        if (IsInLayerMask(other.gameObject, player_layer))
        {
            Player_Controller player = other.GetComponent<Player_Controller>();
            if (player != null && fan_object != null)
            {
                Vector3 player_pos = player.transform.position;
                Vector3 fan_pos = fan_object.position;
                
                Vector3 drag_direction = (fan_pos - player_pos).normalized;
                
                CharacterController cc = player.GetComponent<CharacterController>();
                if (cc != null)
                {
                    cc.Move(drag_direction * drag_force * Time.deltaTime);
                }
                else
                {
                    player.transform.position += drag_direction * drag_force * Time.deltaTime;
                }
            }
        }
    }
    
    private bool IsInLayerMask(GameObject obj, LayerMask layerMask)
    {
        return (layerMask.value & (1 << obj.layer)) != 0;
    }
    
    void Start()
    {
        Collider trigger = GetComponent<Collider>();
        if (trigger == null)
        {
            trigger = gameObject.AddComponent<BoxCollider>();
        }
        trigger.isTrigger = true;
    }
}