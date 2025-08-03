using UnityEngine;

public class ItemUse : MonoBehaviour
{
    [SerializeField] private string requiredItemID;
    [SerializeField] private bool consumeOnUse = true;
    
    public bool TryUseItem(Player_Controller player)
    {
        if (Inventory_Manager.Instance.HasItem(requiredItemID, player.Get_Player_Type()))
        {
            OnItemUsed(player);
            
            if (consumeOnUse)
            {
                Inventory_Manager.Instance.DropItem(player.Get_Player_Type());
            }
            
            return true;
        }
        
        return false;
    }
    
    protected virtual void OnItemUsed(Player_Controller player)
    {
    }
}