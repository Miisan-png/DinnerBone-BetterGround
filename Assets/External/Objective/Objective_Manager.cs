using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ObjectiveManager : MonoBehaviour
{
    [System.Serializable]
    public class Objective
    {
        public string title;
        public Sprite icon;
    }

    [SerializeField] private Canvas objectiveCanvas;
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private Image objectiveIcon;
    [SerializeField] private float typeWriterSpeed = 0.05f;
    [SerializeField] private float fadeDuration = 0.5f;
    
    public Objective[] objectives;
    private int currentObjectiveIndex = 0;
    private Coroutine typeWriterCoroutine;

    void Start()
    {
        if (objectiveCanvas != null) objectiveCanvas.gameObject.SetActive(true);
        if (objectives.Length > 0)
        {
            StartCoroutine(ShowObjectiveWithEffects(objectives[0]));
        }
    }

    public void UpdateObjective(int index)
    {
        if (index >= 0 && index < objectives.Length)
        {
            currentObjectiveIndex = index;
            if (typeWriterCoroutine != null) StopCoroutine(typeWriterCoroutine);
            StartCoroutine(ShowObjectiveWithEffects(objectives[index]));
        }
    }

    public void UpdateObjective(string objectiveTitle)
    {
        for (int i = 0; i < objectives.Length; i++)
        {
            if (objectives[i].title == objectiveTitle)
            {
                UpdateObjective(i);
                return;
            }
        }
    }

    public void NextObjective()
    {
        if (currentObjectiveIndex < objectives.Length - 1)
        {
            UpdateObjective(currentObjectiveIndex + 1);
        }
    }

    private IEnumerator ShowObjectiveWithEffects(Objective objective)
    {
        yield return StartCoroutine(FadeIcon(objective.icon));
        yield return StartCoroutine(TypeWriterEffect(objective.title));
    }

    private IEnumerator FadeIcon(Sprite newIcon)
    {
        if (objectiveIcon != null)
        {
            objectiveIcon.sprite = newIcon;
            objectiveIcon.color = new Color(1, 1, 1, 0);
            
            float elapsedTime = 0;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
                objectiveIcon.color = new Color(1, 1, 1, alpha);
                yield return null;
            }
            objectiveIcon.color = new Color(1, 1, 1, 1);
        }
    }

    private IEnumerator TypeWriterEffect(string text)
    {
        if (objectiveText != null)
        {
            objectiveText.text = "";
            foreach (char letter in text)
            {
                objectiveText.text += letter;
                yield return new WaitForSeconds(typeWriterSpeed);
            }
        }
    }

    [ContextMenu("Next Objective")]
    private void DebugNextObjective()
    {
        NextObjective();
    }
}