using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewSoundDatabase", menuName = "Audio/Sound Database", order = 1)]
public class SoundDatabase : ScriptableObject
{
    public List<SoundData> sounds = new List<SoundData>();

    public AudioClip GetClipByTag(string tag)
    {
        foreach (var sound in sounds)
        {
            if (sound.tag == tag)
            {
                return sound.audioClip;
            }
        }
        Debug.LogWarning($"Sound with tag '{tag}' not found in database.");
        return null;
    }
}