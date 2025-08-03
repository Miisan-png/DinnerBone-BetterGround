using UnityEngine;
using DG.Tweening;
public class Key_Logic : MonoBehaviour, I_Interactable, IInteractionIdentifier
{
    [SerializeField] private string keyID = "door_key";
    [SerializeField] private ItemData keyItemData;
    [SerializeField] private Canvas itemIconCanvas;
    [SerializeField] private string interaction_id = "pickup_key";
    
    private bool isPickedUp = false;
    
    void Start()
    {
        if (itemIconCanvas == null) 
            itemIconCanvas = GetComponentInChildren<Canvas>();
        
        if (keyItemData == null)
        {
            keyItemData = new ItemData();
            keyItemData.itemID = keyID;
            keyItemData.itemName = "Key";
        }
    }
    
    public bool Can_Interact(Player_Type player_type)
    {
        if (isPickedUp) return false;
        if (Inventory_Manager.Instance == null) return true;
        return Inventory_Manager.Instance.GetHeldItem(player_type) == null;
    }
    
    public void Start_Interaction(Player_Controller player)
    {
        if (isPickedUp) return;
        
        if (Inventory_Manager.Instance.PickupItem(keyID, player.Get_Player_Type()))
        {
            isPickedUp = true;
            
            if (itemIconCanvas != null)
            {
                itemIconCanvas.transform.DOScale(Vector3.zero, 0.2f).OnComplete(() => {
                    Destroy(gameObject);
                });
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
    
    public void End_Interaction(Player_Controller player)
    {
    }
    
    public string Get_Interaction_Text()
    {
        return $"Pick up {keyItemData.itemName}";
    }
    
    public Vector3 Get_Interaction_Position()
    {
        return transform.position;
    }
    
    public string GetInteractionID()
    {
        return interaction_id;
    }
}