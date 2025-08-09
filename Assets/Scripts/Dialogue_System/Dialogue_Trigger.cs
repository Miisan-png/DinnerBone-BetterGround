using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class DialogueEntry
{
    public bool isLeftSpeaker = true;
    public bool showOnBothSides = false;
    [TextArea(2, 5)]
    public string dialogueText;
    public float delayAfterText = 1f;
}

public class Dialogue_Trigger : MonoBehaviour
{
    [Header("Player References")]
    [SerializeField] private Transform player1;
    [SerializeField] private Transform player2;
    
    [Header("UI References")]
    [SerializeField] private CanvasGroup dialogueUI;
    [SerializeField] private TextMeshProUGUI leftDialogueLabel;
    [SerializeField] private TextMeshProUGUI rightDialogueLabel;
    
    [Header("Dialogue Settings")]
    [SerializeField] private List<DialogueEntry> dialogueEntries = new List<DialogueEntry>();
    
    [Header("Trigger Settings")]
    [SerializeField] private bool requireBothPlayers = true;
    [SerializeField] private bool replayable = false;
    [SerializeField] private float typewriterSpeed = 0.05f;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    
    private bool hasTriggered = false;
    private bool isPlayingDialogue = false;
    private bool player1InTrigger = false;
    private bool player2InTrigger = false;
    private Coroutine dialogueCoroutine;
    
    void Start()
    {
        if (dialogueUI != null)
        {
            dialogueUI.alpha = 0f;
            dialogueUI.interactable = false;
            dialogueUI.blocksRaycasts = false;
        }
        
        ClearDialogueTexts();
    }
    
    void OnTriggerEnter(Collider other)
    {
        Player_Controller player = other.GetComponent<Player_Controller>();
        if (player == null) return;
        
        if (player.transform == player1)
        {
            player1InTrigger = true;
        }
        else if (player.transform == player2)
        {
            player2InTrigger = true;
        }
        
        CheckTriggerConditions();
    }
    
    void OnTriggerExit(Collider other)
    {
        Player_Controller player = other.GetComponent<Player_Controller>();
        if (player == null) return;
        
        if (player.transform == player1)
        {
            player1InTrigger = false;
        }
        else if (player.transform == player2)
        {
            player2InTrigger = false;
        }
    }
    
    private void CheckTriggerConditions()
    {
        if (isPlayingDialogue) return;
        if (!replayable && hasTriggered) return;
        
        bool shouldTrigger = false;
        
        if (requireBothPlayers)
        {
            shouldTrigger = player1InTrigger && player2InTrigger;
        }
        else
        {
            shouldTrigger = player1InTrigger || player2InTrigger;
        }
        
        if (shouldTrigger)
        {
            StartDialogue();
        }
    }
    
    private void StartDialogue()
    {
        if (dialogueEntries.Count == 0) return;
        
        hasTriggered = true;
        isPlayingDialogue = true;
        
        if (dialogueUI != null)
        {
            dialogueUI.DOFade(1f, fadeInDuration);
            dialogueUI.interactable = true;
            dialogueUI.blocksRaycasts = true;
        }
        
        if (dialogueCoroutine != null)
        {
            StopCoroutine(dialogueCoroutine);
        }
        
        dialogueCoroutine = StartCoroutine(PlayDialogueSequence());
    }
    
    private IEnumerator PlayDialogueSequence()
    {
        ClearDialogueTexts();
        
        for (int i = 0; i < dialogueEntries.Count; i++)
        {
            DialogueEntry entry = dialogueEntries[i];
            
            if (entry.showOnBothSides)
            {
                if (leftDialogueLabel != null && rightDialogueLabel != null)
                {
                    StartCoroutine(TypewriterEffect(leftDialogueLabel, entry.dialogueText));
                    yield return StartCoroutine(TypewriterEffect(rightDialogueLabel, entry.dialogueText));
                }
            }
            else
            {
                TextMeshProUGUI targetLabel = entry.isLeftSpeaker ? leftDialogueLabel : rightDialogueLabel;
                TextMeshProUGUI otherLabel = entry.isLeftSpeaker ? rightDialogueLabel : leftDialogueLabel;
                
                if (otherLabel != null)
                {
                    otherLabel.text = "";
                }
                
                if (targetLabel != null)
                {
                    yield return StartCoroutine(TypewriterEffect(targetLabel, entry.dialogueText));
                }
            }
            
            yield return new WaitForSeconds(entry.delayAfterText);
        }
        
        EndDialogue();
    }
    
    private IEnumerator TypewriterEffect(TextMeshProUGUI label, string text)
    {
        label.text = "";
        
        for (int i = 0; i <= text.Length; i++)
        {
            label.text = text.Substring(0, i);
            yield return new WaitForSeconds(typewriterSpeed);
        }
    }
    
    private void EndDialogue()
    {
        isPlayingDialogue = false;
        
        if (dialogueUI != null)
        {
            dialogueUI.DOFade(0f, fadeOutDuration).OnComplete(() => {
                dialogueUI.interactable = false;
                dialogueUI.blocksRaycasts = false;
                ClearDialogueTexts();
            });
        }
    }
    
    private void ClearDialogueTexts()
    {
        if (leftDialogueLabel != null) leftDialogueLabel.text = "";
        if (rightDialogueLabel != null) rightDialogueLabel.text = "";
    }
    
    public void ForceStartDialogue()
    {
        if (!isPlayingDialogue)
        {
            StartDialogue();
        }
    }
    
    public void ForceStopDialogue()
    {
        if (isPlayingDialogue)
        {
            if (dialogueCoroutine != null)
            {
                StopCoroutine(dialogueCoroutine);
            }
            EndDialogue();
        }
    }
    
    public void ResetTrigger()
    {
        hasTriggered = false;
    }
}