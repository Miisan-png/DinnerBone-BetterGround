using UnityEngine;
using TMPro;
using System.Collections;

public class Objective_Global : MonoBehaviour
{
    public static Objective_Global Instance;
    [SerializeField] private TextMeshProUGUI objectiveText;
    [SerializeField] private float charInterval = 0.03f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetObjective(string text)
    {
        StopAllCoroutines();
        if (objectiveText != null) StartCoroutine(TypewriterRoutine(text));
    }

    IEnumerator TypewriterRoutine(string text)
    {
        objectiveText.text = text;
        objectiveText.ForceMeshUpdate();
        objectiveText.maxVisibleCharacters = 0;
        int total = objectiveText.textInfo.characterCount;
        for (int i = 1; i <= total; i++)
        {
            objectiveText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(charInterval);
        }
    }
}
