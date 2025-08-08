using UnityEngine;
using DG.Tweening;

public class Button_Activation_Logic : MonoBehaviour
{
    public GameObject[] buttons; // Assign Button 1, 2, 3 in inspector
    private Material[] buttonMaterials;
    private bool[] isActivated;

    void Start()
    {
        if (buttons.Length > 0)
        {
            buttonMaterials = new Material[buttons.Length];
            isActivated = new bool[buttons.Length];
            
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] != null)
                {
                    Renderer renderer = buttons[i].GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        buttonMaterials[i] = renderer.material;
                        // Start with buttons deactivated (fully transparent)
                        buttonMaterials[i].SetFloat("_Alpha", 0f);
                        buttons[i].SetActive(false);
                        isActivated[i] = false;
                    }
                }
            }
        }
    }

    public void ActivateButton(int buttonIndex)
    {
        if (buttonIndex >= 0 && buttonIndex < buttons.Length && buttons[buttonIndex] != null)
        {
            if (!isActivated[buttonIndex])
            {
                buttons[buttonIndex].SetActive(true);
                isActivated[buttonIndex] = true;
                
                // Fade in the button
                buttonMaterials[buttonIndex].DOFloat(1f, "_Alpha", 1f).SetEase(Ease.InOutQuad);
            }
        }
    }

    public void ActivateAllButtons()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            ActivateButton(i);
        }
    }
}