using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    public static SoundPlayer Instance;

    [SerializeField] private SoundDatabase soundDatabase;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource ambienceSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: Keep the SoundPlayer alive across scenes
        }
        else
        {
            Destroy(gameObject);
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }

        if (ambienceSource == null)
        {
            GameObject ambienceGO = new GameObject("AmbienceSource");
            ambienceGO.transform.SetParent(transform);
            ambienceSource = ambienceGO.AddComponent<AudioSource>();
            ambienceSource.loop = true;
        }
    }

    public void PlaySFX(string tag)
    {
        AudioClip clip = soundDatabase.GetClipByTag(tag);
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlayAmbience(string tag)
    {
        AudioClip clip = soundDatabase.GetClipByTag(tag);
        if (clip != null)
        {
            ambienceSource.clip = clip;
            ambienceSource.Play();
        }
    }

    public void StopAmbience()
    {
        ambienceSource.Stop();
    }
}