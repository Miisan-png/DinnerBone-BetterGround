using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class Proximity_System : MonoBehaviour
{
    private static Proximity_System instance;
    public static Proximity_System Instance => instance;
    
    [Header("Settings")]
    [SerializeField] private GameObject prompt_prefab;
    [SerializeField] private Input_Icon_Database icon_database;
    [SerializeField] private int pool_size = 10;
    
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
    
    private void InitializePool()
    {
        if (prompt_prefab == null)
        {
            CreateDefaultPromptPrefab();
        }
        
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
    }
    
    private void CreateDefaultPromptPrefab()
    {
        GameObject prefab = new GameObject("Default_Proximity_Prompt");
        
        Canvas canvas = prefab.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        
        CanvasGroup canvas_group = prefab.AddComponent<CanvasGroup>();
        
        RectTransform rect = prefab.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200, 100);
        
        GameObject icon_obj = new GameObject("Icon");
        icon_obj.transform.SetParent(prefab.transform);
        Image icon_image = icon_obj.AddComponent<Image>();
        RectTransform icon_rect = icon_obj.GetComponent<RectTransform>();
        icon_rect.sizeDelta = new Vector2(50, 50);
        icon_rect.anchoredPosition = new Vector2(0, 10);
        
        GameObject text_obj = new GameObject("Action_Label");
        text_obj.transform.SetParent(prefab.transform);
        TextMeshProUGUI text_component = text_obj.AddComponent<TextMeshProUGUI>();
        text_component.text = "Interact";
        text_component.fontSize = 16;
        text_component.alignment = TextAlignmentOptions.Center;
        RectTransform text_rect = text_obj.GetComponent<RectTransform>();
        text_rect.sizeDelta = new Vector2(180, 30);
        text_rect.anchoredPosition = new Vector2(0, -30);
        
        Proximity_Prompt prompt_script = prefab.AddComponent<Proximity_Prompt>();
        
        prompt_prefab = prefab;
        
        DontDestroyOnLoad(prefab);
    }
    
    public void ShowPromptForObject(GameObject target_object, I_Interactable interactable, Player_Controller player)
    {
        if (object_to_prompt.ContainsKey(target_object))
        {
            var existing_prompt = object_to_prompt[target_object];
            if (existing_prompt.IsAssignedToPlayer(player))
            {
                return;
            }
        }
        
        Proximity_Prompt prompt = GetPromptFromPool();
        if (prompt != null)
        {
            prompt.Initialize(target_object.transform, interactable, player);
            prompt.ShowPrompt();
            
            active_prompts.Add(prompt);
            object_to_prompt[target_object] = prompt;
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
        
        return prompt;
    }
    
    private void ReturnPromptToPool(Proximity_Prompt prompt)
    {
        prompt.gameObject.SetActive(false);
        prompt_pool.Enqueue(prompt);
    }
    
    public Input_Icon_Database GetIconDatabase()
    {
        return icon_database;
    }
    
    public void SetIconDatabase(Input_Icon_Database database)
    {
        icon_database = database;
    }
}