using UnityEngine;

[CreateAssetMenu(fileName = "SO_SoundRepo", menuName = "Scriptable Objects/SO_SoundRepo")]
public class SO_SoundRepository : ScriptableObject
{
    public Sound[] SoundList;
    public Sound[] BGMList;

    //public Sound[] DialogueList;
    public string DialoguePath = "Dialogues/Voicelines";
}