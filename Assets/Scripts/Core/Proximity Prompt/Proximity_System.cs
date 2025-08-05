using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Proximity_System : MonoBehaviour
{
    private static Proximity_System instance;
    public static Proximity_System Instance => instance;
    
    [Header("Settings")]
    [SerializeField] private GameObject prompt_prefab;
    [SerializeField] private Input_Icon_Database icon_database;
    [SerializeField] private int pool_size = 10;
    [SerializeField] private bool debug_system = false;
    
    private Queue<Proximity_Prompt> prompt_pool = new Queue<Proximity_Prompt>();
    private List<Proximity_Prompt> active_prompts = new List<Proximity_Prompt>();
    private Dictionary<GameObject, Proximity_Prompt> object_to_prompt = new Dictionary<GameObject, Proximity_Prompt>();
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePool();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Update()
    {
        if (Time.frameCount % 30 == 0)
        {
            UpdateActivePrompts();
        }
    }
    
    private void UpdateActivePrompts()
    {
        foreach (var prompt in active_prompts)
        {
            if (prompt != null && prompt.gameObject.activeInHierarchy)
            {
                prompt.UpdateContent();
            }
        }
    }
    
    private void InitializePool()
    {
        if (prompt_prefab == null)
        {
            CreateDefaultPromptPrefab();
        }
        
        ValidatePromptPrefab();
        
        for (int i = 0; i < pool_size; i++)
        {
            GameObject prompt_obj = Instantiate(prompt_prefab, transform);
            Proximity_Prompt prompt = prompt_obj.GetComponent<Proximity_Prompt>();
            
            if (prompt == null)
            {
                prompt = prompt_obj.AddComponent<Proximity_Prompt>();
            }
            
            prompt_obj.SetActive(false);
            prompt_pool.Enqueue(prompt);
        }
        
        if (debug_system)
        {
            Debug.Log($"Proximity System initialized with {pool_size} prompts. Icon Database: {(icon_database != null ? "Assigned" : "Missing")}");
        }
    }
    
    private void ValidatePromptPrefab()
    {
        if (prompt_prefab == null) return;
        
        Proximity_Prompt prompt_script = prompt_prefab.GetComponent<Proximity_Prompt>();
        Image icon_image = prompt_prefab.GetComponentInChildren<Image>();
        CanvasGroup canvas_group = prompt_prefab.GetComponent<CanvasGroup>();
        
        if (debug_system)
        {
            Debug.Log($"Prefab Validation - Prompt Script: {(prompt_script != null ? "Found" : "Missing")}, " +
                     $"Icon Image: {(icon_image != null ? "Found" : "Missing")}, " +
                     $"Canvas Group: {(canvas_group != null ? "Found" : "Missing")}");
        }
    }
    
    private void CreateDefaultPromptPrefab()
    {
        GameObject prefab = new GameObject("Default_Proximity_Prompt");
        
        Canvas canvas = prefab.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        
        CanvasGroup canvas_group = prefab.AddComponent<CanvasGroup>();
        
        RectTransform rect = prefab.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(100, 100);
        
        GameObject icon_obj = new GameObject("Icon");
        icon_obj.transform.SetParent(prefab.transform, false);
        Image icon_image = icon_obj.AddComponent<Image>();
        RectTransform icon_rect = icon_obj.GetComponent<RectTransform>();
        icon_rect.sizeDelta = new Vector2(80, 80);
        icon_rect.anchoredPosition = Vector2.zero;
        icon_rect.anchorMin = new Vector2(0.5f, 0.5f);
        icon_rect.anchorMax = new Vector2(0.5f, 0.5f);
        
        Proximity_Prompt prompt_script = prefab.AddComponent<Proximity_Prompt>();
        
        var iconField = typeof(Proximity_Prompt).GetField("icon_image", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var canvasGroupField = typeof(Proximity_Prompt).GetField("canvas_group", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        iconField?.SetValue(prompt_script, icon_image);
        canvasGroupField?.SetValue(prompt_script, canvas_group);
        
        prompt_prefab = prefab;
        
        DontDestroyOnLoad(prefab);
        
        if (debug_system)
        {
            Debug.Log("Created default prompt prefab with properly assigned components");
        }
    }
    
    public void ShowPromptForObject(GameObject target_object, I_Interactable interactable, Player_Controller player)
    {
        bool can_interact = interactable.Can_Interact(player.Get_Player_Type());
        
        if (object_to_prompt.ContainsKey(target_object))
        {
            var existing_prompt = object_to_prompt[target_object];
            bool existing_can_interact = existing_prompt.GetInteractable().Can_Interact(existing_prompt.GetAssignedPlayer().Get_Player_Type());
            
            if (can_interact && !existing_can_interact)
            {
                HidePromptForObject(target_object);
            }
            else if (existing_can_interact && !can_interact)
            {
                if (debug_system)
                {
                    Debug.Log($"Keeping existing interactive prompt for {target_object.name}, ignoring restricted request from {player.Get_Player_Type()}");
                }
                return;
            }
            else if (existing_prompt.IsAssignedToPlayer(player) || (can_interact == existing_can_interact))
            {
                existing_prompt.UpdateContent();
                return;
            }
        }
        
        if (!can_interact)
        {
            if (debug_system)
            {
                Debug.Log($"Not showing restricted prompt for {target_object.name}");
            }
            return;
        }
        
        Proximity_Prompt prompt = GetPromptFromPool();
        if (prompt != null)
        {
            prompt.Initialize(target_object.transform, interactable, player);
            prompt.ShowPrompt();
            
            active_prompts.Add(prompt);
            object_to_prompt[target_object] = prompt;
            
            if (debug_system)
            {
                Debug.Log($"Showing interactive prompt for {target_object.name} assigned to player {player.Get_Player_Type()}");
            }
        }
        else
        {
            if (debug_system) Debug.LogWarning("Failed to get prompt from pool");
        }
    }
    
    public void HidePromptForObject(GameObject target_object)
    {
        if (object_to_prompt.TryGetValue(target_object, out Proximity_Prompt prompt))
        {
            prompt.HidePrompt(() => {
                ReturnPromptToPool(prompt);
                active_prompts.Remove(prompt);
            });
            
            object_to_prompt.Remove(target_object);
            
            if (debug_system)
            {
                Debug.Log($"Hiding prompt for {target_object.name}");
            }
        }
    }
    
    public void PlayInteractionEffect(GameObject target_object)
    {
        if (object_to_prompt.TryGetValue(target_object, out Proximity_Prompt prompt))
        {
            prompt.PlayInteractionEffect();
            active_prompts.Remove(prompt);
            object_to_prompt.Remove(target_object);
        }
    }
    
    public void UpdatePromptForObject(GameObject target_object)
    {
        if (object_to_prompt.TryGetValue(target_object, out Proximity_Prompt prompt))
        {
            prompt.UpdateContent();
        }
    }
    
    private Proximity_Prompt GetPromptFromPool()
    {
        if (prompt_pool.Count > 0)
        {
            return prompt_pool.Dequeue();
        }
        
        GameObject prompt_obj = Instantiate(prompt_prefab, transform);
        Proximity_Prompt prompt = prompt_obj.GetComponent<Proximity_Prompt>();
        
        if (prompt == null)
        {
            prompt = prompt_obj.AddComponent<Proximity_Prompt>();
        }
        
        if (debug_system)
        {
            Debug.Log("Created new prompt (pool was empty)");
        }
        
        return prompt;
    }
    
    private void ReturnPromptToPool(Proximity_Prompt prompt)
    {
        prompt.gameObject.SetActive(false);
        prompt_pool.Enqueue(prompt);
    }
    
    public Input_Icon_Database GetIconDatabase()
    {
        if (icon_database == null && debug_system)
        {
            Debug.LogWarning("Icon database is not assigned in Proximity_System!");
        }
        return icon_database;
    }
    
    public void SetIconDatabase(Input_Icon_Database database)
    {
        icon_database = database;
        if (debug_system)
        {
            Debug.Log($"Icon database {(database != null ? "assigned" : "cleared")}");
        }
    }
}