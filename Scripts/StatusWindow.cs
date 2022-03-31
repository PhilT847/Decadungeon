using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusWindow : MonoBehaviour
{
    public GameController gameController;

    public Image pauseWeaponIcon;
    public Image pauseBadgeIcon;

    public Button[] heroIcons;
    public Button[] swapButtons;
    public StatusWeaponFrame shownWeapon;

    public Image statusPortraitBorder; //the gold border that goes around the selected hero in the Status menu

    public Image statusPortrait;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI vitalsText;
    public TextMeshProUGUI statusText;

    public Character viewedCharacter; //the character currently viewed in the Status menu

    // Sliders and bad range indicators for each hero. Indicators show whether this hero deals half damage with their weapon
    public Slider[] HP_Sliders;
    public Slider[] MP_Sliders;
    public Image[] badRangeIndicators;

    public bool checkingDetailedScreen;
    public DetailedInfo detailScreen;

    public void OpenStatusWindow()
    {
        CheckStatus(0);

        if (checkingDetailedScreen)
        {
            ToggleDetailedScreen();
        }
    }

    public void CheckStatus(int heroIndex)
    {
        viewedCharacter = gameController.allCharacters[heroIndex];

        statusPortraitBorder.transform.localPosition = heroIcons[heroIndex].transform.localPosition;

        statusPortrait.sprite = viewedCharacter.characterPortrait;

        nameText.SetText(viewedCharacter.unitName);

        string shownVitals = " Lv. " + viewedCharacter.level + "\n";

        shownVitals += viewedCharacter.currentHP + "/" + viewedCharacter.maxHP + "\n";

        shownVitals += viewedCharacter.currentMP + "/" + viewedCharacter.maxMP;

        vitalsText.SetText(shownVitals);

        string shownStats = viewedCharacter.GetRawStrength() + "\n";

        shownStats += viewedCharacter.GetRawMagic() + "\n";

        shownStats += viewedCharacter.GetRawDexterity() + "\n";

        shownStats += viewedCharacter.GetRawFaith() + "\n";

        statusText.SetText(shownStats);

        pauseWeaponIcon.sprite = viewedCharacter.equippedWeapon.itemDisplayIcon;

        shownWeapon.SetWeaponStatus(viewedCharacter);

        if (viewedCharacter.equippedBadge != null)
        {
            pauseBadgeIcon.color = Color.white;
            pauseBadgeIcon.sprite = viewedCharacter.equippedBadge.itemImage;
        }
        else
        {
            pauseBadgeIcon.color = Color.clear;
        }

        // While in combat, the swap buttons are made non-interactable.
        foreach (Button swapButton in swapButtons)
        {
            swapButton.interactable = !FindObjectOfType<GameController>().battleScreen.gameObject.activeSelf;

            if (FindObjectOfType<GameController>().battleScreen.gameObject.activeSelf)
            {
                swapButton.GetComponent<Image>().color = Color.clear;
            }
            else
            {
                swapButton.GetComponent<Image>().color = Color.white;
            }
        }
        
        // Check if the characters are in a "bad range" for their weapon (for example, using a bow in the frontline)
        for(int i = 0; i < gameController.allCharacters.Length; i++)
        {
            Character thisCharacter = gameController.allCharacters[i];

            if (!thisCharacter.equippedWeapon.mixedRangeWeapon
                && ((thisCharacter.inBackRow && !thisCharacter.equippedWeapon.rangedWeapon)
                || (!thisCharacter.inBackRow && thisCharacter.equippedWeapon.rangedWeapon)))
            {
                badRangeIndicators[i].color = Color.white;
            }
            else
            {
                badRangeIndicators[i].color = Color.clear;
            }
        }

        // Also set up the detail screen
        detailScreen.SetToCharacter(viewedCharacter);

        UpdateSliders();
    }

    // Updates each HP/MP bar on the hero icons
    void UpdateSliders()
    {
        Character[] allCharacters = FindObjectOfType<GameController>().allCharacters;

        for (int i = 0; i < HP_Sliders.Length; i++)
        {
            HP_Sliders[i].value = (float)allCharacters[i].currentHP / allCharacters[i].maxHP;
            MP_Sliders[i].value = (float)allCharacters[i].currentMP / allCharacters[i].maxMP;
        }
    }

    public void ToggleDetailedScreen()
    {
        checkingDetailedScreen = !checkingDetailedScreen;

        detailScreen.gameObject.SetActive(checkingDetailedScreen);

        // When looking at the detailed screen, the weapon/badge icons disappear
        shownWeapon.gameObject.SetActive(!checkingDetailedScreen);
        pauseWeaponIcon.transform.parent.gameObject.SetActive(!checkingDetailedScreen);
        pauseBadgeIcon.transform.parent.gameObject.SetActive(!checkingDetailedScreen);
    }

    public void SwapHeroPosition(int index)
    {
        Vector3 savePosition = heroIcons[index].transform.localPosition;
        heroIcons[index].transform.localPosition = swapButtons[index].transform.localPosition;
        swapButtons[index].transform.localPosition = savePosition;

        //if the selected hero is moved, move the selected portrait along with them
        if (gameController.allCharacters[index] == viewedCharacter)
        {
            statusPortraitBorder.transform.localPosition = heroIcons[index].transform.localPosition;
        }

        gameController.allCharacters[index].SwapPosition();

        // If moved to a bad range for their weapon, make their indicator appear. Otherwise, it's clear. Mixed range weapons always work at max power
        if (!gameController.allCharacters[index].equippedWeapon.mixedRangeWeapon
            && ((!gameController.allCharacters[index].equippedWeapon.rangedWeapon && gameController.allCharacters[index].inBackRow)
            || (gameController.allCharacters[index].equippedWeapon.rangedWeapon && !gameController.allCharacters[index].inBackRow)))
        {
            badRangeIndicators[index].color = Color.white;
        }
        else
        {
            badRangeIndicators[index].color = Color.clear;
        }
    }
}
