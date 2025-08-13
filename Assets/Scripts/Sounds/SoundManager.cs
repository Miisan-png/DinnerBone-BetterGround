using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mostly used for oneshot sounds.
/// Warning: Not recommended to use it for loopable objects. The pooling will cause the sound to be cut off when reusing the sound emitter
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] private SO_SoundRepository SoundRepoSO;
    [SerializeField] private Pool<SoundEmitter> m_SoundEmitterPool;
    [SerializeField] private string m_TestSoundName;
    [SerializeField] private AudioSource m_BGMAudioSource;
    [SerializeField] private AudioSource m_DialogueAudioSource;

    private Dictionary<string, Sound> m_OneShotAudioDict;
    private Dictionary<string, Sound> m_BGMAudioDict;
    private Dictionary<string, Sound> m_DialogueAudioDict;
    private List<SoundEmitter> m_SoundEmitterList = new List<SoundEmitter>();

    private Coroutine _bgmStartLoopCr;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;

            // Unparent the object to enable don't destroy on load as it is attached to a high level manager empty object
            if (transform.parent != null)
            {
                transform.SetParent(null);
            }
            DontDestroyOnLoad(gameObject);
        }

        this.m_OneShotAudioDict = new Dictionary<string, Sound>();
        for (int s = 0; s < this.SoundRepoSO.SoundList.Length; s++)
        {
            Sound sound = this.SoundRepoSO.SoundList[s];
            this.m_OneShotAudioDict.Add(sound.SoundName, sound);
        }

        this.m_BGMAudioDict = new Dictionary<string, Sound>();
        for (int i = 0; i < this.SoundRepoSO.BGMList.Length; i++)
        {
            Sound sound = this.SoundRepoSO.BGMList[i];
            this.m_BGMAudioDict.Add(sound.SoundName, sound);
        }

        

        GameObject soundEmitterParent = new GameObject("SoundEmitter Parent");
        soundEmitterParent.transform.parent = transform;

        m_SoundEmitterPool.Initialize(soundEmitterParent.transform);
    }

    [ContextMenu("Test Sound fx")]
    private void TestSound()
    {
        PlaySound(m_TestSoundName, transform);
    }

    [ContextMenu("Test Stop Sound")]
    private void TestStopSound()
    {
        StopOneShotBySoundName(m_TestSoundName);
    }

    [ContextMenu("Test Music")]
    private void TestMusic()
    {
        PlayBGMusic("bgm_testmusic");
    }

    /// <summary>
    /// Sound Emitters register itself to sound manager to get its entity ID
    /// Allows for tracking and managing of sound emitters
    /// </summary>
    /// <param name="soundEmitter"></param>
    /// <returns>Entity ID</returns>
    public int SubscribeSoundEmitter(SoundEmitter soundEmitter)
    {
        m_SoundEmitterList.Add(soundEmitter);
        return m_SoundEmitterList.Count - 1;
    }

    public void LoadDialogueVoicelines(int levelID)
    {
        this.m_DialogueAudioDict = new Dictionary<string, Sound>();
        AudioClip[] dialogues = Resources.LoadAll<AudioClip>(this.SoundRepoSO.DialoguePath + "/Lv"+levelID);

        foreach(AudioClip dialogue in dialogues) 
        {
            Sound sound = new Sound()
            {
                Clip = dialogue,
                Pitch = 1.0f,
                SoundName = dialogue.name,
                SpatialBlend = 0f,
                Volume = 1.0f
            };
            
            this.m_DialogueAudioDict.Add(dialogue.name, sound);
        }
    }

    // To fix in firesout! lol
    public int PlaySound(SoundVariationizer variationizer, Transform attachedObj = null, SoundSetting soundSettingOverwrite = null)
    {
        if (soundSettingOverwrite != null)
        {
            soundSettingOverwrite.Pitch = variationizer.Pitch;
        }
        else
        {
            soundSettingOverwrite = new SoundSetting() { Pitch = variationizer.Pitch };
        }

        return PlaySound(variationizer.SoundName, attachedObj, soundSettingOverwrite);
    }

    /// <summary>
    /// Plays the specified sound using PlayOneShot
    /// </summary>
    /// <param name="soundName"></param>
    public int PlaySound(string soundName, Transform attachedObj = null, SoundSetting soundSettingOverwrite = null)
    {

        Sound soundToPlay;
        if (this.m_OneShotAudioDict.TryGetValue(soundName, out soundToPlay))
        {
            SoundEmitter soundEmitter = m_SoundEmitterPool.GetNextObject();

            if (attachedObj != null)
                soundEmitter.PlaySound(soundToPlay, attachedObj, soundSettingOverwrite);
            else
                soundEmitter.PlaySound(soundToPlay, transform, soundSettingOverwrite);

            //Debug.Log("[SoundSystem] Playing sfx: " + soundName);

            return soundEmitter.GetSoundEntityID();
        }
        else
        {
            Debug.LogWarning("Sound: " + soundName + " not found!");
            return -1;
        }

    }

    public int PlayAtLocation(string soundName, Vector3 location, SoundSetting soundSettingOverwrite = null)
    {
        Sound soundToPlay;
        if (this.m_OneShotAudioDict.TryGetValue(soundName, out soundToPlay))
        {
            SoundEmitter soundEmitter = m_SoundEmitterPool.GetNextObject();
            soundEmitter.transform.position = location;
            soundEmitter.PlaySound(soundToPlay, null, soundSettingOverwrite);

            return soundEmitter.GetSoundEntityID();
        }
        else
        {
            Debug.LogWarning("Sound: " + soundName + " not found!");
            return -1;
        }
    }

    /// <summary>
    /// Stops the first existing found sound. 
    /// Warning: If multiple instances of the same sound are playing, only the first instance found will be stopped.
    /// </summary>
    /// <param name="soundName"></param>
    public void StopOneShotBySoundName(string soundName)
    {
        if (!m_OneShotAudioDict.TryGetValue(soundName, out Sound soundToStop))
        {
            Debug.LogWarning($"Sound: {soundName} not found! Cannot stop playback.");
            return;
        }

        // Iterate through active SoundEmitters in the pool
        foreach (SoundEmitter soundEmitter in m_SoundEmitterPool.GetActiveObjects())
        {
            if (soundEmitter.IsPlayingSound(soundToStop))
            {
                soundEmitter.StopSound();
                Debug.Log($"Stopped sound: {soundName}");
                return; // Stop once the sound is found
            }
        }

        Debug.LogWarning($"Sound: {soundName} is not currently playing!");
    }

    public void StopOneShotByEntityID(int entityID)
    {
        if (entityID < 0 || entityID >= m_SoundEmitterList.Count)
        {
            Debug.LogWarning($"Entity ID: {entityID} is out of range! Cannot stop playback.");
            return;
        }

        m_SoundEmitterList[entityID].StopSound();
    }


    public void PlayBGMusic(string musicName)
    {
        Sound soundToPlay;
        if (this.m_BGMAudioDict.TryGetValue(musicName, out soundToPlay))
        {
            m_BGMAudioSource.Stop();
            m_BGMAudioSource.clip = soundToPlay.Clip;
            m_BGMAudioSource.volume = soundToPlay.Volume;
            m_BGMAudioSource.spatialBlend = soundToPlay.SpatialBlend;
            m_BGMAudioSource.pitch = soundToPlay.Pitch;
            m_BGMAudioSource.loop = false;
            m_BGMAudioSource.Play();

            if (_bgmStartLoopCr != null)
            {
                StopCoroutine(_bgmStartLoopCr);
            }

            _bgmStartLoopCr = StartCoroutine(BgmStartLoopCr(musicName, soundToPlay.Clip));
        }
        else
        {
            Debug.LogWarning("Music: " + musicName + " not found!");
            return;
        }
    

    }

    public void StopBGMMusic()
    {
        m_BGMAudioSource.Stop();
    }

    public void PlayDialogue(string dialogueName)
    {
        Sound soundToPlay;
        if (this.m_DialogueAudioDict.TryGetValue(dialogueName, out soundToPlay))
        {
            m_DialogueAudioSource.Stop();
            m_DialogueAudioSource.clip = soundToPlay.Clip;
            m_DialogueAudioSource.volume = soundToPlay.Volume;
            m_DialogueAudioSource.spatialBlend = soundToPlay.SpatialBlend;
            m_DialogueAudioSource.pitch = soundToPlay.Pitch;
            m_DialogueAudioSource.Play();
        }
        else
        {
            Debug.LogWarning("Dialogue: " + dialogueName + " not found!");
            return;
        }
    }

    public void StopDialogue()
    {
        m_DialogueAudioSource.Stop();
    }

    public AudioClip GetCurrentDialogueClip()
    {
        return m_DialogueAudioSource.clip;
    }

    /// <summary>
    /// Ensure that the looping bgm has suffix ending with _loop
    /// </summary>
    /// <param name="musicName"></param>
    /// <returns></returns>
    private IEnumerator BgmStartLoopCr(string musicName, AudioClip clip)
    {
        float openLength = clip.length;

        yield return new WaitForSecondsRealtime(openLength);

        Sound soundToPlay;
        if (this.m_BGMAudioDict.TryGetValue(musicName + "_loop", out soundToPlay))
        {
            m_BGMAudioSource.Stop();
            m_BGMAudioSource.clip = soundToPlay.Clip;
            m_BGMAudioSource.volume = soundToPlay.Volume;
            m_BGMAudioSource.spatialBlend = soundToPlay.SpatialBlend;
            m_BGMAudioSource.pitch = soundToPlay.Pitch;
            m_BGMAudioSource.loop = true;
            m_BGMAudioSource.Play();
        }
        else
        {
            Debug.LogWarning("Music Loop: " + musicName + " not found!");
            yield break;
        }
    }
}