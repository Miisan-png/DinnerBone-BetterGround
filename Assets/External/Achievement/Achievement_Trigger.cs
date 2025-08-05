using UnityEngine;

public enum Trigger_Condition { Enter, Exit, Both_Players_Enter, Any_Player_Enter }

public class Achievement_Trigger : MonoBehaviour
{
    [Header("Achievement Settings")]
    [SerializeField] private string achievement_tag;
    [SerializeField] private Trigger_Condition unlock_condition = Trigger_Condition.Enter;
    
    [Header("Player Requirements")]
    [SerializeField] private bool require_both_players = false;
    [SerializeField] private bool luthe_allowed = true;
    [SerializeField] private bool cherie_allowed = true;
    
    [Header("Trigger Settings")]
    [SerializeField] private bool one_time_only = true;
    [SerializeField] private bool debug_trigger = false;
    
    private bool has_triggered = false;
    private bool luthe_in_trigger = false;
    private bool cherie_in_trigger = false;
    
    void Start()
    {
        BoxCollider trigger_collider = GetComponent<BoxCollider>();
        if (trigger_collider == null)
        {
            trigger_collider = gameObject.AddComponent<BoxCollider>();
        }
        trigger_collider.isTrigger = true;
    }
    
    void OnTriggerEnter(Collider other)
    {
        Player_Controller player = other.GetComponent<Player_Controller>();
        if (player == null) return;
        
        if (player.Get_Player_Type() == Player_Type.Luthe && luthe_allowed)
        {
            luthe_in_trigger = true;
            if (debug_trigger) Debug.Log("Luthe entered achievement trigger");
        }
        else if (player.Get_Player_Type() == Player_Type.Cherie && cherie_allowed)
        {
            cherie_in_trigger = true;
            if (debug_trigger) Debug.Log("Cherie entered achievement trigger");
        }
        
        if (unlock_condition == Trigger_Condition.Enter || unlock_condition == Trigger_Condition.Any_Player_Enter)
        {
            CheckTriggerConditions();
        }
        else if (unlock_condition == Trigger_Condition.Both_Players_Enter)
        {
            CheckBothPlayersCondition();
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        Player_Controller player = other.GetComponent<Player_Controller>();
        if (player == null) return;
        
        if (player.Get_Player_Type() == Player_Type.Luthe)
        {
            luthe_in_trigger = false;
            if (debug_trigger) Debug.Log("Luthe exited achievement trigger");
        }
        else if (player.Get_Player_Type() == Player_Type.Cherie)
        {
            cherie_in_trigger = false;
            if (debug_trigger) Debug.Log("Cherie exited achievement trigger");
        }
        
        if (unlock_condition == Trigger_Condition.Exit)
        {
            CheckTriggerConditions();
        }
    }
    
    private void CheckTriggerConditions()
    {
        if (has_triggered && one_time_only) return;
        
        bool should_trigger = false;
        
        switch (unlock_condition)
        {
            case Trigger_Condition.Enter:
            case Trigger_Condition.Any_Player_Enter:
                should_trigger = (luthe_in_trigger && luthe_allowed) || (cherie_in_trigger && cherie_allowed);
                break;
                
            case Trigger_Condition.Exit:
                should_trigger = (!luthe_in_trigger && luthe_allowed) || (!cherie_in_trigger && cherie_allowed);
                break;
        }
        
        if (should_trigger)
        {
            TriggerAchievement();
        }
    }
    
    private void CheckBothPlayersCondition()
    {
        if (has_triggered && one_time_only) return;
        
        bool both_players_present = true;
        
        if (luthe_allowed && !luthe_in_trigger) both_players_present = false;
        if (cherie_allowed && !cherie_in_trigger) both_players_present = false;
        
        if (both_players_present)
        {
            TriggerAchievement();
        }
    }
    
    private void TriggerAchievement()
    {
        if (Achievement_Manager.Instance != null)
        {
            Achievement_Manager.Instance.UnlockAchievement(achievement_tag);
            has_triggered = true;
            
            if (debug_trigger)
            {
                Debug.Log($"Achievement triggered: {achievement_tag}");
            }
        }
        else
        {
            Debug.LogError("Achievement_Manager instance not found!");
        }
    }
    
    public void ManualTrigger()
    {
        TriggerAchievement();
    }
    
    public void ResetTrigger()
    {
        has_triggered = false;
        luthe_in_trigger = false;
        cherie_in_trigger = false;
    }
    
    void OnDrawGizmos()
    {
        BoxCollider trigger_collider = GetComponent<BoxCollider>();
        if (trigger_collider != null)
        {
            Gizmos.color = has_triggered ? Color.green : Color.yellow;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(trigger_collider.center, trigger_collider.size);
        }
        
        if (luthe_in_trigger || cherie_in_trigger)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.5f);
        }
    }
}