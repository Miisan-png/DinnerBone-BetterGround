using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PickupItem : MonoBehaviour, I_Interactable, IInteractionIdentifier
{
    [SerializeField] private ItemData itemData;
    [SerializeField] private Canvas itemIconCanvas;
    [SerializeField] private Image itemIconImage;
    [SerializeField] private float detectionRange = 3f;
    [SerializeField] private float scaleUpSize = 1.2f;
    [SerializeField] private float animationSpeed = 0.3f;
    [SerializeField] private string interaction_id = "pickup_item";
    
    private Player_Controller nearbyPlayer;
    private bool isScaledUp = false;
    private Vector3 originalScale;
    
    void Start()
    {
        if (itemIconCanvas == null) itemIconCanvas = GetComponentInChildren<Canvas>();
        if (itemIconImage == null) itemIconImage = GetComponentInChildren<Image>();
        
        if (itemIconImage != null && itemData != null)
        {
            itemIconImage.sprite = itemData.itemIcon;
        }
        
        if (itemIconCanvas != null)
        {
            originalScale = itemIconCanvas.transform.localScale;
        }
    }
    
    void Update()
    {
        CheckForNearbyPlayers();
        UpdateIconScale();
    }
    
    private void CheckForNearbyPlayers()
    {
        Player_Controller closestPlayer = null;
        float closestDistance = detectionRange;
        
        Player_Controller[] players = FindObjectsByType<Player_Controller>(FindObjectsSortMode.None);
        
        foreach (Player_Controller player in players)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = player;
            }
        }
        
        nearbyPlayer = closestPlayer;
    }
    
    private void UpdateIconScale()
    {
        if (itemIconCanvas == null) return;
        
        bool shouldScaleUp = nearbyPlayer != null;
        
        if (shouldScaleUp && !isScaledUp)
        {
            isScaledUp = true;
            itemIconCanvas.transform.DOScale(originalScale * scaleUpSize, animationSpeed);
        }
        else if (!shouldScaleUp && isScaledUp)
        {
            isScaledUp = false;
            itemIconCanvas.transform.DOScale(originalScale, animationSpeed);
        }
    }
    
    public bool Can_Interact(Player_Type player_type)
    {
        return itemData != null && Inventory_Manager.Instance.GetHeldItem(player_type) == null;
    }
    
    public void Start_Interaction(Player_Controller player)
    {
        if (Inventory_Manager.Instance.PickupItem(itemData.itemID, player.Get_Player_Type()))
        {
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
        return itemData != null ? $"Pick up {itemData.itemName}" : "Pick up item";
    }
    
    public Vector3 Get_Interaction_Position()
    {
        return transform.position;
    }
    
    public string GetInteractionID()
    {
        return interaction_id;
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        if (nearbyPlayer != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, nearbyPlayer.transform.position);
        }
    }
}