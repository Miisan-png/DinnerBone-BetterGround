using UnityEngine;
using UnityEngine.Video;
using DG.Tweening;
public class CutsceneVideoPlayer : MonoBehaviour
{
    [Header("References")]
    public VideoPlayer videoPlayer;
    public CutsceneManager cutsceneManager;
    
    [Header("Settings")]
    public bool autoPlayOnStart = true;
    public bool skipToSceneOnVideoEnd = true;
    
    private bool hasVideoEnded = false;
    
    void Start()
    {
        InitializeVideoPlayer();
    }
    
    void InitializeVideoPlayer()
    {
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }
        
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
            
            if (autoPlayOnStart)
            {
                videoPlayer.Play();
            }
        }
    }
    
    void OnVideoFinished(VideoPlayer vp)
    {
        if (hasVideoEnded) return;

        hasVideoEnded = true;

        if (cutsceneManager != null && cutsceneManager.fadePanel != null)
        {
            cutsceneManager.fadePanel.DOFade(1f, cutsceneManager.fadeInSpeed).OnComplete(() =>
            {
                if (skipToSceneOnVideoEnd)
                {
                    cutsceneManager.LoadNextScene();
                }
            });
        }
        else
        {
            if (skipToSceneOnVideoEnd && cutsceneManager != null)
            {
                cutsceneManager.LoadNextScene();
            }
        }
    }

    
    public void PlayVideo()
    {
        if (videoPlayer != null)
        {
            hasVideoEnded = false;
            videoPlayer.Play();
        }
    }
    
    public void StopVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }
    }
    
    public void PauseVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Pause();
        }
    }
    
    public void ForceSkipVideo()
    {
        if (cutsceneManager != null)
        {
            cutsceneManager.LoadNextScene();
        }
    }
    
    void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }
}