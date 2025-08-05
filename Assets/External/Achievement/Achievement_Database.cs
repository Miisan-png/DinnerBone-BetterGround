using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Achievement Database", menuName = "Achievements/Achievement Database")]
public class Achievement_Database : ScriptableObject
{
    [System.Serializable]
    public class Achievement_Entry
    {
        [Header("Achievement Info")]
        public string achievement_tag;
        public Sprite achievement_icon;
        [TextArea(2, 4)]
        public string description_text;
        
        [Header("Visual Settings")]
        public Color icon_color = Color.white;
        public Color text_color = Color.white;
    }
    
    [Header("All Achievements")]
    public List<Achievement_Entry> achievements = new List<Achievement_Entry>();
    
    public Achievement_Entry GetAchievement(string achievement_tag)
    {
        return achievements.Find(a => a.achievement_tag == achievement_tag);
    }
    
    public int GetTotalCount()
    {
        return achievements.Count;
    }
    
    public bool HasAchievement(string achievement_tag)
    {
        return GetAchievement(achievement_tag) != null;
    }
}