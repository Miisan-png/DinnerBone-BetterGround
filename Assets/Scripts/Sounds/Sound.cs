using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sound
{
    public string SoundName;
    public AudioClip Clip;
    [HideInInspector] public Transform AttachedParent;

    [Range(0f, 1f)] //Adds slider between the range
    public float Volume = 1f;
    [Range(0f, 1f)]
    public float SpatialBlend = 0f;
    [Range(.1f, 3f)]
    public float Pitch = 1f;

}

public struct SoundVariationizer
{

    public float Pitch;
    public float MinInclusiveRandomID;
    public float MaxExclusiveRandomID;
    public string SoundName;

    public SoundVariationizer(string soundName, float pitchVariation, int minInclusiveRandomID, int maxExclusiveRandomID)
    {
        Pitch = Random.Range(1 - pitchVariation, 1 + pitchVariation);

        MinInclusiveRandomID = minInclusiveRandomID;
        MaxExclusiveRandomID = maxExclusiveRandomID;

        SoundName = soundName + Random.Range(minInclusiveRandomID, maxExclusiveRandomID);
    }

    public SoundVariationizer(string soundName, float pitchVariation)
    {
        Pitch = Random.Range(1-pitchVariation, 1+pitchVariation);

        MinInclusiveRandomID = -1f;
        MaxExclusiveRandomID = -1f;

        SoundName = soundName;
    }

    public SoundVariationizer(string soundName, int minInclusiveRandomID, int maxExclusiveRandomID)
    {
        Pitch = 1f;

        MinInclusiveRandomID = minInclusiveRandomID;
        MaxExclusiveRandomID = maxExclusiveRandomID;

        SoundName = soundName + Random.Range(minInclusiveRandomID, maxExclusiveRandomID);
    }
}

[System.Serializable]
public class SoundSetting
{
    // Basic settings
    public float Volume=-1f;
    public float SpatialBlend = -1f;
    public float Pitch = -1f;

    public bool Loop = false;
    public float PlayDelay = -1f;

    // Stereo & 3D Sound
    public StereoSettings Stereo = new StereoSettings();
    public RolloffSettings Rolloff = new RolloffSettings();

    // Essential Advanced Settings
    public bool PlayOnAwake = true;
    public int Priority = 128; // Default Unity priority (0 = highest, 256 = lowest)
    public AudioClip Clip;
    public AudioMixerGroup MixerGroup;

    /// <summary>
    /// Applies the sound settings to a given AudioSource.
    /// </summary>
    public void ApplyTo(AudioSource audioSource)
    {

        audioSource.volume = Volume;
        audioSource.pitch = Pitch;
        audioSource.spatialBlend = SpatialBlend;
        audioSource.loop = Loop;
        audioSource.playOnAwake = PlayOnAwake;
        audioSource.priority = Priority;
        audioSource.clip = Clip;
        audioSource.outputAudioMixerGroup = MixerGroup;

        Stereo.ApplyTo(audioSource);
        Rolloff.ApplyTo(audioSource);

    }

    /// <summary>
    /// Copies settings from an existing AudioSource.
    /// </summary>
    public void CopyFrom(AudioSource audioSource)
    {
        Volume = audioSource.volume;
        Pitch = audioSource.pitch;
        SpatialBlend = audioSource.spatialBlend;
        Loop = audioSource.loop;
        PlayOnAwake = audioSource.playOnAwake;
        Priority = audioSource.priority;
        Clip = audioSource.clip;
        MixerGroup = audioSource.outputAudioMixerGroup;

        Stereo.CopyFrom(audioSource);
        Rolloff.CopyFrom(audioSource);
    }
}

[System.Serializable]
public class StereoSettings
{
    public float PanStereo = 0f;
    public float DopplerLevel = 1f;
    public float Spread = 0f;
    public float ReverbZoneMix = 1f;

    public void ApplyTo(AudioSource audioSource)
    {
        audioSource.panStereo = PanStereo;
        audioSource.dopplerLevel = DopplerLevel;
        audioSource.spread = Spread;
        audioSource.reverbZoneMix = ReverbZoneMix;
    }

    public void CopyFrom(AudioSource audioSource)
    {
        PanStereo = audioSource.panStereo;
        DopplerLevel = audioSource.dopplerLevel;
        Spread = audioSource.spread;
        ReverbZoneMix = audioSource.reverbZoneMix;
    }
}

[System.Serializable]
public class RolloffSettings
{
    public AudioRolloffMode RolloffMode = AudioRolloffMode.Logarithmic;
    public float MinDistance = 1f;
    public float MaxDistance = 500f;

    public void ApplyTo(AudioSource audioSource)
    {
        audioSource.rolloffMode = RolloffMode;
        audioSource.minDistance = MinDistance;
        audioSource.maxDistance = MaxDistance;
    }

    public void CopyFrom(AudioSource audioSource)
    {
        RolloffMode = audioSource.rolloffMode;
        MinDistance = audioSource.minDistance;
        MaxDistance = audioSource.maxDistance;
    }
}
