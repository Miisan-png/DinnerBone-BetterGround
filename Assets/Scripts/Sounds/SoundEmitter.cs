using System.Collections;
using UnityEngine;

public class SoundEmitter : MonoBehaviour
{
    private static int EXECUTE_EVERY_N_FRAMES = 5;

    private AudioSource m_AudioSource;
    private Transform m_AttachedObj;
    private Sound m_CurrentSound;
    private int m_SoundEntityID;
    private Coroutine m_DelayCr;
    
    private void Awake()
    {
        m_AudioSource = GetComponent<AudioSource>();
        m_SoundEntityID = SoundManager.Instance.SubscribeSoundEmitter(this);
    }

    /// <summary>
    /// Play sound and have the sound follow the attached object
    /// 
    /// If attached object is null, the sound will play at the sound emitter's position
    /// You can pass in special effects to overwrite the sound settings. If not, it will use the sound's default settings
    /// </summary>
    /// <param name="soundToPlay"></param>
    /// <param name="attachObj">Object sound to follow</param>
    /// <param name="soundSettingOverwrite">Special Effects</param>
    public void PlaySound(Sound soundToPlay, Transform attachObj, SoundSetting soundSettingOverwrite = null)
    {
        float playDelay = -1f;
        bool resetAttachedObjAfterPlay = true;
        m_AttachedObj = attachObj;
        m_CurrentSound = soundToPlay;

        m_AudioSource.clip = soundToPlay.Clip;

        if (soundSettingOverwrite == null)
        {
            m_AudioSource.volume = soundToPlay.Volume;
            m_AudioSource.spatialBlend = soundToPlay.SpatialBlend;
            m_AudioSource.pitch = soundToPlay.Pitch;
        }
        else
        {
            m_AudioSource.volume = soundSettingOverwrite.Volume == -1f ? soundToPlay.Volume : soundSettingOverwrite.Volume;
            m_AudioSource.spatialBlend = soundSettingOverwrite.SpatialBlend == -1f ? soundToPlay.SpatialBlend : soundSettingOverwrite.SpatialBlend;
            m_AudioSource.pitch = soundSettingOverwrite.Pitch == -1f ? soundToPlay.Pitch : soundSettingOverwrite.Pitch;
            m_AudioSource.loop = soundSettingOverwrite.Loop;
            playDelay = soundSettingOverwrite.PlayDelay;
            resetAttachedObjAfterPlay = playDelay == -1f ? true : false;
        }

        if (playDelay == -1f)
        {
            m_AudioSource.Play();

            if (resetAttachedObjAfterPlay)
                Invoke("ResetAttachedObject", soundToPlay.Clip.length);
        }
        else
        {
            if (m_DelayCr != null)
            {
                StopCoroutine(m_DelayCr);
            }

            m_DelayCr = StartCoroutine(DelayPlayCr(playDelay, soundToPlay, resetAttachedObjAfterPlay));
        }
        
    }
    
    public void StopSound()
    {
        if (m_DelayCr != null)
        {
            StopCoroutine(m_DelayCr);
        }

        m_AudioSource.Stop();
        m_CurrentSound = null;
    }

    public bool IsPlayingSound(Sound sound)
    {
        return m_AudioSource.isPlaying && m_CurrentSound == sound;
    }

    public int GetSoundEntityID()
    {
        return m_SoundEntityID;
    }

    private void ResetAttachedObject()
    {
        m_AudioSource.Stop();
        m_AttachedObj = null;
        m_CurrentSound = null;
    }

    private void Update()
    {
        // Restricts update to every N frames
        if (Time.frameCount % EXECUTE_EVERY_N_FRAMES == 0)
        {
            if (m_AttachedObj != null)
            {
                transform.position = m_AttachedObj.position;
            }
        }
        
    }

    private IEnumerator DelayPlayCr(float delay, Sound soundToPlay, bool resetAttachedObjAfterPlay)
    {
        yield return new WaitForSeconds(delay);
        m_AudioSource.Play();

        if (resetAttachedObjAfterPlay)
            Invoke("ResetAttachedObject", soundToPlay.Clip.length);
    }
}

