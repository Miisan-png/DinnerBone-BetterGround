using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Linq;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private Canvas dialogueCanvas;
    [SerializeField] private GameObject textBoxPanel;
    [SerializeField] private TextMeshProUGUI characterNameLabel;
    [SerializeField] private TextMeshProUGUI dialogueContentLabel;
    [SerializeField] private Camera_Manager cameraManager;
    [SerializeField] private Transform player1;
    [SerializeField] private Transform player2;
    [SerializeField] private float dialogueAdvanceDelay = 2f;
    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private bool debugMode = false;
    [SerializeField] private float typewriterSpeed = 0.05f;
    
    private List<DialogueData> allDialogues = new List<DialogueData>();
    private List<DialogueData> currentDialogueSet = new List<DialogueData>();
    private int currentDialogueIndex = 0;
    private bool isDialoguePlaying = false;
    private bool isTyping = false;
    private Vector3 player1OriginalRotation;
    private Vector3 player2OriginalRotation;
    private Coroutine typewriterCoroutine;
    
    public static DialogueManager Instance;
    
    void Awake()
    {
        Instance = this;
        LoadDialogueData();
        dialogueCanvas.gameObject.SetActive(false);
    }
    
    void LoadDialogueData()
    {
        if (debugMode) Debug.Log("[DialogueManager] Loading dialogue data...");
        
        TextAsset csvFile = Resources.Load<TextAsset>("dialogue_data");
        if (csvFile == null)
        {
            if (debugMode) Debug.LogError("[DialogueManager] CSV file 'dialogue_data' not found in Resources folder!");
            return;
        }
        
        string[] lines = csvFile.text.Split('\n');
        if (debugMode) Debug.Log($"[DialogueManager] Found {lines.Length} lines in CSV");
        
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrEmpty(lines[i])) continue;
            
            string[] values = lines[i].Split(',');
            if (values.Length >= 5)
            {
                DialogueData data = new DialogueData
                {
                    ID = int.Parse(values[0].Trim()),
                    DIALOGUE_CONTENT = values[1].Trim(),
                    C_TYPE = values[2].Trim(),
                    D_TYPE = values[3].Trim(),
                    TAG = values[4].Trim()
                };
                allDialogues.Add(data);
                if (debugMode) Debug.Log($"[DialogueManager] Loaded dialogue: ID={data.ID}, TAG='{data.TAG}', C_TYPE={data.C_TYPE}");
            }
        }
        
        if (debugMode) Debug.Log($"[DialogueManager] Total dialogues loaded: {allDialogues.Count}");
    }
    
    public void PlayDialogue(string tag)
    {
        if (debugMode) Debug.Log($"[DialogueManager] PlayDialogue called with tag: {tag}");
        
        if (isDialoguePlaying) 
        {
            if (debugMode) Debug.Log("[DialogueManager] Dialogue already playing, ignoring request");
            return;
        }
        
        if (cameraManager.Get_Current_Mode() != Camera_Mode.Follow) 
        {
            if (debugMode) Debug.Log($"[DialogueManager] Camera not in Follow mode: {cameraManager.Get_Current_Mode()}");
            return;
        }
        
        currentDialogueSet = allDialogues.Where(d => d.TAG.Equals(tag, System.StringComparison.OrdinalIgnoreCase)).OrderBy(d => d.ID).ToList();
        if (debugMode) 
        {
            Debug.Log($"[DialogueManager] Found {currentDialogueSet.Count} dialogues for tag: '{tag}'");
            Debug.Log($"[DialogueManager] Available tags: {string.Join(", ", allDialogues.Select(d => $"'{d.TAG}'").Distinct())}");
        }
        
        if (currentDialogueSet.Count == 0) 
        {
            if (debugMode) Debug.LogWarning($"[DialogueManager] No dialogues found for tag: {tag}");
            return;
        }
        
        StartDialogue();
    }
    
    void StartDialogue()
    {
        if (debugMode) Debug.Log("[DialogueManager] Starting dialogue...");
        
        isDialoguePlaying = true;
        currentDialogueIndex = 0;
        
        if (player1 != null && player2 != null)
        {
            player1OriginalRotation = player1.eulerAngles;
            player2OriginalRotation = player2.eulerAngles;
            RotatePlayersToFaceEachOther();
        }
        else
        {
            if (debugMode) Debug.LogError("[DialogueManager] Player references are null!");
        }
        
        ShowDialogueUI();
        DisplayCurrentDialogue();
    }
    
    void RotatePlayersToFaceEachOther()
    {
        Vector3 player1Target = new Vector3(player1.eulerAngles.x, 90f, player1.eulerAngles.z);
        Vector3 player2Target = new Vector3(player2.eulerAngles.x, -90f, player2.eulerAngles.z);
        
        player1.DORotate(player1Target, rotationSpeed);
        player2.DORotate(player2Target, rotationSpeed);
    }
    
    void ShowDialogueUI()
    {
        if (debugMode) Debug.Log("[DialogueManager] ShowDialogueUI called");
        
        if (dialogueCanvas == null)
        {
            if (debugMode) Debug.LogError("[DialogueManager] Dialogue canvas is null!");
            return;
        }
        
        if (player1 == null || player2 == null)
        {
            if (debugMode) Debug.LogError("[DialogueManager] Player references are null in ShowDialogueUI!");
            return;
        }
        
        Vector3 centerPosition = (player1.position + player2.position) / 2f;
        centerPosition.y += 2f;
        
        if (debugMode) Debug.Log($"[DialogueManager] Setting canvas position to: {centerPosition}");
        
        dialogueCanvas.transform.position = centerPosition;
        dialogueCanvas.gameObject.SetActive(true);
        
        if (textBoxPanel != null)
        {
            textBoxPanel.transform.localScale = Vector3.zero;
            textBoxPanel.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        }
        else
        {
            if (debugMode) Debug.LogError("[DialogueManager] TextBox panel is null!");
        }
    }
    
    void DisplayCurrentDialogue()
    {
        if (currentDialogueIndex >= currentDialogueSet.Count)
        {
            EndDialogue();
            return;
        }
        
        DialogueData currentDialogue = currentDialogueSet[currentDialogueIndex];
        
        characterNameLabel.text = currentDialogue.C_TYPE;
        characterNameLabel.alpha = 0f;
        characterNameLabel.DOFade(1f, 0.2f);
        
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
        }
        
        typewriterCoroutine = StartCoroutine(TypewriterEffect(currentDialogue.DIALOGUE_CONTENT));
    }
    
    IEnumerator TypewriterEffect(string text)
    {
        isTyping = true;
        dialogueContentLabel.text = "";
        dialogueContentLabel.alpha = 1f;
        
        for (int i = 0; i <= text.Length; i++)
        {
            dialogueContentLabel.text = text.Substring(0, i);
            yield return new WaitForSeconds(typewriterSpeed);
        }
        
        isTyping = false;
        
        float waitTime = dialogueAdvanceDelay;
        if (currentDialogueSet[currentDialogueIndex].D_TYPE == "END")
        {
            waitTime = dialogueAdvanceDelay * 1.5f;
        }
        
        yield return new WaitForSeconds(waitTime);
        AdvanceDialogue();
    }
    
    void AdvanceDialogue()
    {
        currentDialogueIndex++;
        
        if (currentDialogueIndex < currentDialogueSet.Count)
        {
            if (currentDialogueSet[currentDialogueIndex].D_TYPE == "END")
            {
                EndDialogue();
                return;
            }
        }
        
        DisplayCurrentDialogue();
    }
    
    void EndDialogue()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }
        
        isTyping = false;
        
        textBoxPanel.transform.DOScale(Vector3.zero, 0.2f).OnComplete(() =>
        {
            dialogueCanvas.gameObject.SetActive(false);
            RestorePlayerRotations();
            isDialoguePlaying = false;
        });
    }
    
    void RestorePlayerRotations()
    {
        player1.DORotate(player1OriginalRotation, rotationSpeed);
        player2.DORotate(player2OriginalRotation, rotationSpeed);
    }
    
    public bool IsDialoguePlaying()
    {
        return isDialoguePlaying;
    }
}