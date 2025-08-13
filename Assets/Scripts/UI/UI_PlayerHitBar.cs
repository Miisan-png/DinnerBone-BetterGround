using UnityEngine;

public class UI_PlayerHitBar : MonoBehaviour
{
    [SerializeField] GameObject[] healthUI;

    [SerializeField] private bool isLuther;

    private void Update()
    {
        if (isLuther)
        {
            int hits = Spider_Boss_Player_State.Instance.Player1Hits;

            for (int i = 0; i < healthUI.Length; i++)
            {
                // Active if index >= hits
                healthUI[i].SetActive(i >= hits);
            }
        }
        else
        {
            int hits = Spider_Boss_Player_State.Instance.Player2Hits;

            for (int i = 0; i < healthUI.Length; i++)
            {
                // Active if index >= hits
                healthUI[i].SetActive(i >= hits);
            }
        }
    }


}
