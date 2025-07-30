using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private string dialogueTag;
    [SerializeField] private bool oneTimeOnly = true;
    [SerializeField] private bool debugMode = false;
    
    private bool player1InTrigger = false;
    private bool player2InTrigger = false;
    private bool hasTriggered = false;
    private Camera_Manager cameraManager;
    private DialogueManager dialogueManager;
    
    void Start()
    {
        cameraManager = FindObjectOfType<Camera_Manager>();
        dialogueManager = FindObjectOfType<DialogueManager>();
    }
    
    void OnTriggerEnter(Collider other)
    {
        Player_Controller player = other.GetComponent<Player_Controller>();
        if (player == null) 
        {
            if (debugMode) Debug.Log($"[DialogueTrigger] Non-player object entered: {other.name}");
            return;
        }
        
        if (player.Get_Player_Type() == Player_Type.Luthe)
        {
            player1InTrigger = true;
            if (debugMode) Debug.Log("[DialogueTrigger] Player 1 (Luthe) entered trigger");
        }
        else if (player.Get_Player_Type() == Player_Type.Cherie)
        {
            player2InTrigger = true;
            if (debugMode) Debug.Log("[DialogueTrigger] Player 2 (Cherie) entered trigger");
        }
        
        CheckTriggerConditions();
    }
    
    void OnTriggerExit(Collider other)
    {
        Player_Controller player = other.GetComponent<Player_Controller>();
        if (player == null) return;
        
        if (player.Get_Player_Type() == Player_Type.Luthe)
        {
            player1InTrigger = false;
            if (debugMode) Debug.Log("[DialogueTrigger] Player 1 (Luthe) exited trigger");
        }
        else if (player.Get_Player_Type() == Player_Type.Cherie)
        {
            player2InTrigger = false;
            if (debugMode) Debug.Log("[DialogueTrigger] Player 2 (Cherie) exited trigger");
        }
    }
    
    void CheckTriggerConditions()
    {
        if (debugMode) Debug.Log($"[DialogueTrigger] Checking conditions: P1={player1InTrigger}, P2={player2InTrigger}, HasTriggered={hasTriggered}, CameraMode={cameraManager?.Get_Current_Mode()}");
        
        if (hasTriggered && oneTimeOnly) 
        {
            if (debugMode) Debug.Log("[DialogueTrigger] Already triggered and oneTimeOnly is true");
            return;
        }
        
        if (!player1InTrigger || !player2InTrigger) 
        {
            if (debugMode) Debug.Log("[DialogueTrigger] Both players not in trigger");
            return;
        }
        
        if (cameraManager.Get_Current_Mode() != Camera_Mode.Follow) 
        {
            if (debugMode) Debug.Log("[DialogueTrigger] Camera not in Follow mode");
            return;
        }
        
        if (dialogueManager == null) 
        {
            if (debugMode) Debug.Log("[DialogueTrigger] DialogueManager not found");
            return;
        }
        
        if (dialogueManager.IsDialoguePlaying()) 
        {
            if (debugMode) Debug.Log("[DialogueTrigger] Dialogue already playing");
            return;
        }
        
        if (debugMode) Debug.Log($"[DialogueTrigger] All conditions met! Playing dialogue: {dialogueTag}");
        dialogueManager.PlayDialogue(dialogueTag);
        hasTriggered = true;
    }
    
    public void ResetTrigger()
    {
        hasTriggered = false;
    }
}