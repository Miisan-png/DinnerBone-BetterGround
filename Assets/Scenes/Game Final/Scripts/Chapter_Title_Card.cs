using UnityEngine;
using TMPro;
using System.Collections;

public class Chapter_Title_Card : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI titleText;
    public float typeWriterSpeed = 0.05f;
    public float holdTime = 2f;
    public float fadeOutDuration = 1f;

    void Start()
    {
        StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        canvasGroup.alpha = 1;
        string fullText = titleText.text;
        titleText.text = "";

        for (int i = 0; i < fullText.Length; i++)
        {
            titleText.text = fullText.Substring(0, i + 1);
            yield return new WaitForSeconds(typeWriterSpeed);
        }

        yield return new WaitForSeconds(holdTime);

        float t = 0;
        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1, 0, t / fadeOutDuration);
            yield return null;
        }
    }
}
