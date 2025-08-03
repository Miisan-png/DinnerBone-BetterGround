using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[System.Serializable]
public class ItemData
{
    public string itemID;
    public string itemName;
    public Sprite itemIcon;
    public bool isKeyItem = true;
}

public class Inventory_Manager : MonoBehaviour
{
    private static Inventory_Manager instance;
    public static Inventory_Manager Instance => instance;
    
    [SerializeField] private Image player1HeldIcon;
    [SerializeField] private Image player2HeldIcon;
    [SerializeField] private ItemData[] allItems;
    
    private ItemData player1HeldItem;
    private ItemData player2HeldItem;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        if (player1HeldIcon != null)
        {
            player1HeldIcon.gameObject.SetActive(false);
        }
        if (player2HeldIcon != null)
        {
            player2HeldIcon.gameObject.SetActive(false);
        }
    }
    
    public bool PickupItem(string itemID, Player_Type playerType)
    {
        ItemData item = GetItemData(itemID);
        if (item == null) return false;
        
        if (playerType == Player_Type.Luthe)
        {
            if (player1HeldItem != null) return false;
            player1HeldItem = item;
            UpdatePlayerIcon(player1HeldIcon, item);
        }
        else
        {
            if (player2HeldItem != null) return false;
            player2HeldItem = item;
            UpdatePlayerIcon(player2HeldIcon, item);
        }
        
        return true;
    }
    
    public bool DropItem(Player_Type playerType)
    {
        if (playerType == Player_Type.Luthe)
        {
            if (player1HeldItem == null) return false;
            player1HeldItem = null;
            ClearPlayerIcon(player1HeldIcon);
        }
        else
        {
            if (player2HeldItem == null) return false;
            player2HeldItem = null;
            ClearPlayerIcon(player2HeldIcon);
        }
        
        return true;
    }
    
    public ItemData GetHeldItem(Player_Type playerType)
    {
        return playerType == Player_Type.Luthe ? player1HeldItem : player2HeldItem;
    }
    
    public bool HasItem(string itemID, Player_Type playerType)
    {
        ItemData heldItem = GetHeldItem(playerType);
        return heldItem != null && heldItem.itemID == itemID;
    }
    
    private ItemData GetItemData(string itemID)
    {
        foreach (ItemData item in allItems)
        {
            if (item.itemID == itemID) return item;
        }
        return null;
    }
    
    private void UpdatePlayerIcon(Image iconImage, ItemData item)
    {
        if (iconImage == null) return;
        
        iconImage.sprite = item.itemIcon;
        iconImage.color = Color.white;
        iconImage.gameObject.SetActive(true);
        
        iconImage.transform.localScale = Vector3.zero;
        iconImage.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
    }
    
    private void ClearPlayerIcon(Image iconImage)
    {
        if (iconImage == null) return;
        
        iconImage.transform.DOScale(Vector3.zero, 0.2f).OnComplete(() => {
            iconImage.gameObject.SetActive(false);
            iconImage.sprite = null;
        });
    }
}