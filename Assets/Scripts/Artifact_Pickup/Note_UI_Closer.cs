using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class Note_UI_Closer : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image showNoteSampleImage;
    [SerializeField] private TextMeshProUGUI continueText;
    
    [Header("Fixed Positions")]
    [SerializeField] private Vector3 textFinalPosition = Vector3.zero;
    [SerializeField] private Vector3 imageFinalPosition = Vector3.zero;
    
    void Update()
    {
        if (gameObject.activeInHierarchy && canvasGroup != null && canvasGroup.alpha > 0.5f)
        {
            if (Input.anyKeyDown)
            {
                ForceClose();
            }
        }
    }
    
    public void ForceClose()
    {
        if (canvasGroup != null)
        {
            canvasGroup.DOFade(0f, 0.3f).OnComplete(() => {
                ResetPositions();
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                gameObject.SetActive(false);
            });
        }
        else
        {
            ResetPositions();
            gameObject.SetActive(false);
        }
    }
    
    private void ResetPositions()
    {
        if (continueText != null)
        {
            continueText.DOKill();
            continueText.transform.localScale = Vector3.one;
            continueText.color = new Color(continueText.color.r, continueText.color.g, continueText.color.b, 0f);
        }
        
        if (showNoteSampleImage != null)
        {
            showNoteSampleImage.transform.localPosition = imageFinalPosition + Vector3.down * 469f;
        }
    }
    
    public Vector3 GetTextFinalPosition()
    {
        return textFinalPosition;
    }
    
    public Vector3 GetImageFinalPosition()
    {
        return imageFinalPosition;
    }
}