using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HowToPlayMenu : MonoBehaviour
{
    public TextMeshProUGUI displayText;
    public Image displayImage;

    //Parallel arrays for displayed texts/images
    public string[] howToTexts;
    public Sprite[] howToImages;

    public Button[] allButtons;

    public void OpenMenu()
    {
        SetDisplay(0);
    }

    public void CloseMenu()
    {
        FindObjectOfType<MainMenu>().ReturnToMenu();
        gameObject.SetActive(false);
    }

    public void SetDisplay(int whatToDisplay)
    {
        displayText.SetText(howToTexts[whatToDisplay]);
        displayImage.sprite = howToImages[whatToDisplay];

        //Set the chosen button to inactive, and the rest active
        for(int i = 0; i < allButtons.Length; i++)
        {
            allButtons[i].enabled = i != whatToDisplay;
        }
    }
}
