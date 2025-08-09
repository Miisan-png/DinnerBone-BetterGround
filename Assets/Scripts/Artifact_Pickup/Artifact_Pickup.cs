using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class Artifact_Note_System : MonoBehaviour, I_Interactable, IInteractionIdentifier
{
    [Header("Artifact Settings")]
    [SerializeField] private Canvas iconCanvas;
    [SerializeField] private Image iconImage;
    [SerializeField] private Sprite noteSprite;
    [SerializeField] private string interaction_id = "artifact_note";
    
    [Header("Note UI")]
    [SerializeField] private CanvasGroup noteCanvasGroup;
    [SerializeField] private Image showNoteSampleImage;
    [SerializeField] private TextMeshProUGUI continueText;
    
    [Header("Settings")]
    [SerializeField] private float detectionRange = 3f;
    [SerializeField] private Color interactColor = Color.yellow;
    
    private bool isUsed = false;
    private Player_Controller nearbyPlayer;
    private Vector3 originalIconScale;
    private Vector3 originalTextPosition;
    private Vector3 originalImagePosition;
    private Color originalIconColor;
    private Note_UI_Closer uiCloser;
    
    void Start()
    {
        if (iconCanvas != null)
        {
            originalIconScale = iconCanvas.transform.localScale;
            iconCanvas.transform.localScale = Vector3.zero;
        }
        
        if (iconImage != null)
        {
            originalIconColor = iconImage.color;
        }
        
        if (noteCanvasGroup != null)
        {
            noteCanvasGroup.alpha = 0f;
            noteCanvasGroup.interactable = false;
            noteCanvasGroup.blocksRaycasts = false;
            uiCloser = noteCanvasGroup.GetComponent<Note_UI_Closer>();
            
            if (uiCloser != null)
            {
                originalImagePosition = uiCloser.GetImageFinalPosition();
            }
        }
        
        if (showNoteSampleImage != null)
        {
            showNoteSampleImage.transform.localPosition = originalImagePosition + Vector3.down * 469f;
        }
    }
    
    void Update()
    {
        if (isUsed) return;
        
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
        if (iconCanvas == null) return;
        
        bool shouldShow = nearbyPlayer != null;
        Vector3 targetScale = shouldShow ? originalIconScale : Vector3.zero;
        
        iconCanvas.transform.DOScale(targetScale, 0.3f);
    }
    
    public bool Can_Interact(Player_Type player_type)
    {
        return !isUsed && nearbyPlayer != null;
    }
    
    public void Start_Interaction(Player_Controller player)
    {
        if (isUsed) return;
        
        isUsed = true;
        ShowNote();
        Destroy(gameObject);
    }
    
    public void End_Interaction(Player_Controller player) { }
    
    public string Get_Interaction_Text()
    {
        return "Read Note";
    }
    
    public Vector3 Get_Interaction_Position()
    {
        return transform.position;
    }
    
    public string GetInteractionID()
    {
        return interaction_id;
    }
    
    private void ShowNote()
    {
        if (noteCanvasGroup != null)
        {
            noteCanvasGroup.gameObject.SetActive(true);
            noteCanvasGroup.DOFade(1f, 0.5f);
            noteCanvasGroup.interactable = true;
            noteCanvasGroup.blocksRaycasts = true;
        }
        
        if (showNoteSampleImage != null && noteSprite != null)
        {
            showNoteSampleImage.sprite = noteSprite;
            showNoteSampleImage.transform.DOLocalMove(originalImagePosition, 0.8f).SetEase(Ease.OutBack).OnComplete(() => {
                
                if (continueText != null)
                {
                    continueText.DOFade(1f, 0.5f).OnComplete(() => {
                        continueText.transform.DOScale(1.05f, 2f)
                            .SetLoops(-1, LoopType.Yoyo)
                            .SetEase(Ease.InOutSine);
                    });
                }
            });
        }
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        if (nearbyPlayer != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, nearbyPlayer.transform.position);
        }
    }
}