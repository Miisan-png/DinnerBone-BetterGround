using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Advanced_Interact_Icon : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image leftBracket;
    [SerializeField] private Image rightBracket;
    
    [Header("Icons")]
    [SerializeField] private Sprite lockedIcon;
    [SerializeField] private Sprite unlockedIcon;
    
    [Header("Colors")]
    [SerializeField] private Color visibleColor = Color.white;
    [SerializeField] private Color canInteractColor = Color.green;
    [SerializeField] private Color cannotInteractColor = Color.red;
    
    [Header("Animation Settings")]
    [SerializeField] private float bracketExpandDistance = 20f;
    [SerializeField] private float fadeSpeed = 0.3f;
    [SerializeField] private float detectionRange = 3f;
    [SerializeField] private float interactRange = 2f;
    
    private Vector3 leftBracketOriginalPos;
    private Vector3 rightBracketOriginalPos;
    private bool isExpanded = false;
    private bool isVisible = false;
    private I_Interactable parentInteractable;
    private Player_Controller nearbyPlayer;
    private bool isInInteractRange = false;
    
    void Start()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        
        parentInteractable = GetComponentInParent<I_Interactable>();
        
        if (leftBracket != null)
            leftBracketOriginalPos = leftBracket.rectTransform.anchoredPosition;
        if (rightBracket != null)
            rightBracketOriginalPos = rightBracket.rectTransform.anchoredPosition;
        
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }
    
    void Update()
    {
        CheckForNearbyPlayers();
        UpdateIconState();
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
        
        if (nearbyPlayer != null)
        {
            float distance = Vector3.Distance(transform.position, nearbyPlayer.transform.position);
            isInInteractRange = distance <= interactRange;
        }
        else
        {
            isInInteractRange = false;
        }
    }
    
    private void UpdateIconState()
    {
        bool shouldShow = nearbyPlayer != null;
        
        if (shouldShow && !isVisible)
        {
            Show();
        }
        else if (!shouldShow && isVisible)
        {
            Hide();
        }
        
        if (shouldShow && isVisible)
        {
            UpdateVisualState();
            
            if (isInInteractRange && !isExpanded)
            {
                ExpandBrackets();
            }
            else if (!isInInteractRange && isExpanded)
            {
                CollapseBrackets();
            }
        }
    }
    
    private void UpdateVisualState()
    {
        if (nearbyPlayer == null || parentInteractable == null) return;
        
        bool canInteract = parentInteractable.Can_Interact(nearbyPlayer.Get_Player_Type());
        
        if (isInInteractRange)
        {
            Color targetColor = canInteract ? canInteractColor : cannotInteractColor;
            ApplyColors(targetColor, canInteract);
        }
        else
        {
            ApplyColors(visibleColor, canInteract);
        }
    }
    
    private void ApplyColors(Color color, bool canInteract)
    {
        if (iconImage != null)
        {
            iconImage.sprite = canInteract ? unlockedIcon : lockedIcon;
            iconImage.color = color;
        }
        
        if (leftBracket != null)
            leftBracket.color = color;
        if (rightBracket != null)
            rightBracket.color = color;
    }
    
    private void Show()
    {
        isVisible = true;
        
        if (canvasGroup != null)
        {
            canvasGroup.DOFade(1f, fadeSpeed);
        }
    }
    
    public void Hide()
    {
        isVisible = false;
        
        if (canvasGroup != null)
        {
            canvasGroup.DOFade(0f, fadeSpeed);
        }
        
        if (isExpanded)
        {
            CollapseBrackets();
        }
    }
    

    
    private void ExpandBrackets()
    {
        if (isExpanded) return;
        isExpanded = true;
        
        if (leftBracket != null)
        {
            Vector3 leftTarget = leftBracketOriginalPos + Vector3.left * bracketExpandDistance;
            leftBracket.rectTransform.DOAnchorPos(leftTarget, fadeSpeed).SetEase(Ease.OutQuad);
        }
        
        if (rightBracket != null)
        {
            Vector3 rightTarget = rightBracketOriginalPos + Vector3.right * bracketExpandDistance;
            rightBracket.rectTransform.DOAnchorPos(rightTarget, fadeSpeed).SetEase(Ease.OutQuad);
        }
    }
    
    private void CollapseBrackets()
    {
        if (!isExpanded) return;
        isExpanded = false;
        
        if (leftBracket != null)
        {
            leftBracket.rectTransform.DOAnchorPos(leftBracketOriginalPos, fadeSpeed).SetEase(Ease.InQuad);
        }
        
        if (rightBracket != null)
        {
            rightBracket.rectTransform.DOAnchorPos(rightBracketOriginalPos, fadeSpeed).SetEase(Ease.InQuad);
        }
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
        
        if (nearbyPlayer != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, nearbyPlayer.transform.position);
        }
    }
}