using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;

public class Achievement_Manager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image achievement_icon;
    [SerializeField] private TextMeshProUGUI description_text;
    [SerializeField] private TextMeshProUGUI count_label;
    
    [Header("Achievement Data")]
    [SerializeField] private Achievement_Database achievement_database;
    
    [Header("Animation Settings")]
    [SerializeField] private float slide_duration = 0.8f;
    [SerializeField] private float display_duration = 3f;
    [SerializeField] private float slide_distance = 400f;
    
    private HashSet<string> unlocked_achievements = new HashSet<string>();
    private Vector3 icon_original_position;
    private Vector3 icon_hidden_position;
    private Vector3 text_original_position;
    private Vector3 text_hidden_position;
    private Vector3 count_original_position;
    private Vector3 count_hidden_position;
    private bool is_showing = false;
    private Queue<Achievement_Database.Achievement_Entry> achievement_queue = new Queue<Achievement_Database.Achievement_Entry>();
    
    void Start()
    {
        if (achievement_icon != null)
        {
            icon_original_position = achievement_icon.transform.localPosition;
            icon_hidden_position = icon_original_position + Vector3.left * slide_distance;
            achievement_icon.transform.localPosition = icon_hidden_position;
        }
        
        if (description_text != null)
        {
            text_original_position = description_text.transform.localPosition;
            text_hidden_position = text_original_position + Vector3.left * slide_distance;
            description_text.transform.localPosition = text_hidden_position;
        }
        
        if (count_label != null)
        {
            count_original_position = count_label.transform.localPosition;
            count_hidden_position = count_original_position + Vector3.left * slide_distance;
            count_label.transform.localPosition = count_hidden_position;
        }
        
        UpdateCountLabel();
    }
    
    public void UnlockAchievement(string achievement_tag)
    {
        if (unlocked_achievements.Contains(achievement_tag)) return;
        if (achievement_database == null) return;
        
        Achievement_Database.Achievement_Entry achievement = achievement_database.GetAchievement(achievement_tag);
        if (achievement == null) return;
        
        unlocked_achievements.Add(achievement_tag);
        UpdateCountLabel();
        
        achievement_queue.Enqueue(achievement);
        
        if (!is_showing)
        {
            ShowNextAchievement();
        }
    }
    
    private void ShowNextAchievement()
    {
        if (achievement_queue.Count == 0) return;
        
        Achievement_Database.Achievement_Entry achievement = achievement_queue.Dequeue();
        is_showing = true;
        
        if (achievement_icon != null)
        {
            achievement_icon.sprite = achievement.achievement_icon;
            achievement_icon.color = achievement.icon_color;
        }
        
        if (description_text != null)
        {
            description_text.text = achievement.description_text;
            description_text.color = achievement.text_color;
        }
        
        Sequence achievement_sequence = DOTween.Sequence();
        
        if (achievement_icon != null)
        {
            achievement_sequence.Append(
                achievement_icon.transform.DOLocalMove(icon_original_position, slide_duration)
                    .SetEase(Ease.OutBack)
            );
        }
        
        if (description_text != null)
        {
            achievement_sequence.Join(
                description_text.transform.DOLocalMove(text_original_position, slide_duration)
                    .SetEase(Ease.OutBack)
            );
        }
        
        if (count_label != null)
        {
            achievement_sequence.Join(
                count_label.transform.DOLocalMove(count_original_position, slide_duration)
                    .SetEase(Ease.OutBack)
            );
        }
        
        achievement_sequence.AppendInterval(display_duration);
        
        if (achievement_icon != null)
        {
            achievement_sequence.Append(
                achievement_icon.transform.DOLocalMove(icon_hidden_position, slide_duration)
                    .SetEase(Ease.InBack)
            );
        }
        
        if (description_text != null)
        {
            achievement_sequence.Join(
                description_text.transform.DOLocalMove(text_hidden_position, slide_duration)
                    .SetEase(Ease.InBack)
            );
        }
        
        if (count_label != null)
        {
            achievement_sequence.Join(
                count_label.transform.DOLocalMove(count_hidden_position, slide_duration)
                    .SetEase(Ease.InBack)
            );
        }
        
        achievement_sequence.OnComplete(() => {
            is_showing = false;
            
            if (achievement_queue.Count > 0)
            {
                DOVirtual.DelayedCall(0.2f, ShowNextAchievement);
            }
        });
    }
    
    private void UpdateCountLabel()
    {
        if (count_label != null && achievement_database != null)
        {
            count_label.text = $"{unlocked_achievements.Count}/{achievement_database.GetTotalCount()}";
        }
    }
    
    public bool IsAchievementUnlocked(string achievement_tag)
    {
        return unlocked_achievements.Contains(achievement_tag);
    }
    
    public int GetUnlockedCount()
    {
        return unlocked_achievements.Count;
    }
    
    public int GetTotalCount()
    {
        return achievement_database != null ? achievement_database.GetTotalCount() : 0;
    }
    
    public void ResetAllAchievements()
    {
        unlocked_achievements.Clear();
        UpdateCountLabel();
    }
    
    [ContextMenu("Test Achievement")]
    private void TestAchievement()
    {
        if (achievement_database != null && achievement_database.achievements.Count > 0)
        {
            UnlockAchievement(achievement_database.achievements[0].achievement_tag);
        }
    }
}
