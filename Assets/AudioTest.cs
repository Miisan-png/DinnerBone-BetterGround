using UnityEngine;

/// <summary>
/// A simple script to play a single audio clip on a GameObject.
/// Attach this script to any GameObject, add an AudioSource component
/// to the same GameObject, and assign an AudioClip in the Inspector.
/// </summary>
public class AudioTest : MonoBehaviour
{
    // The AudioSource component on this GameObject
    // This will be automatically found if not assigned in the Inspector.
    private AudioSource audioSource;

    // The audio clip you want to play.
    // Drag your audio file here in the Unity Inspector.
    [SerializeField]
    private AudioClip clipToPlay;

    void Start()
    {
        // Get the AudioSource component attached to this GameObject.
        // It's good practice to do this in Start or Awake.
        audioSource = GetComponent<AudioSource>();

        // Check if both the AudioSource and the clip are available.
        if (audioSource != null && clipToPlay != null)
        {
            // Play the clip once. PlayOneShot is ideal for sound effects
            // as it doesn't stop other sounds already playing on this source.
            audioSource.PlayOneShot(clipToPlay);
        }
        else
        {
            // Log a warning if something is missing to help with debugging.
            Debug.LogWarning("AudioTest script is missing an AudioSource component or an AudioClip to play.");
        }
    }
}
