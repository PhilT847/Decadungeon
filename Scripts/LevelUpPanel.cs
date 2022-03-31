using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelUpPanel : MonoBehaviour
{
    public Character controlledCharacter;

    public Slider expSlider;

    public GameObject newSkillDisplay; //circle that says "New Skill" when the character learns an ability

    public TextMeshProUGUI levelText;

    public TextMeshProUGUI healthText;
    public TextMeshProUGUI strengthText;
    public TextMeshProUGUI magicText;
    public TextMeshProUGUI dexterityText;
    public TextMeshProUGUI faithText;

    public TextMeshProUGUI healthIncreaseText;
    public TextMeshProUGUI strengthIncreaseText;
    public TextMeshProUGUI magicIncreaseText;
    public TextMeshProUGUI dexterityIncreaseText;
    public TextMeshProUGUI faithIncreaseText;

    public int originalHP;
    public int originalStrength;
    public int originalMagic;
    public int originalDexterity;
    public int originalFaith;

    public void InitializePanel()
    {
        Color graphiteGray = new Color32(50, 50, 50, 255);

        healthText.color = graphiteGray;
        strengthText.color = graphiteGray;
        magicText.color = graphiteGray;
        dexterityText.color = graphiteGray;
        faithText.color = graphiteGray;

        newSkillDisplay.SetActive(false);

        originalHP = controlledCharacter.GetRawMaxHP();
        originalStrength = controlledCharacter.GetRawStrength();
        originalMagic = controlledCharacter.GetRawMagic();
        originalDexterity = controlledCharacter.GetRawDexterity();
        originalFaith = controlledCharacter.GetRawFaith();

        levelText.SetText("{0}", controlledCharacter.level);

        healthText.SetText("{0}", controlledCharacter.GetRawMaxHP());
        strengthText.SetText("{0}", controlledCharacter.GetRawStrength());
        magicText.SetText("{0}", controlledCharacter.GetRawMagic());
        dexterityText.SetText("{0}", controlledCharacter.GetRawDexterity());
        faithText.SetText("{0}", controlledCharacter.GetRawFaith());

        healthIncreaseText.transform.parent.gameObject.SetActive(false);
        strengthIncreaseText.transform.parent.gameObject.SetActive(false);
        magicIncreaseText.transform.parent.gameObject.SetActive(false);
        dexterityIncreaseText.transform.parent.gameObject.SetActive(false);
        faithIncreaseText.transform.parent.gameObject.SetActive(false);

        expSlider.maxValue = controlledCharacter.expToNextLevel;
        expSlider.value = controlledCharacter.exp;
    }

    public void VisualizeStatIncrease(string statIncreased)
    {
        Color increaseGreen = new Color32(80,120,60, 255);

        switch (statIncreased)
        {
            case "HP":
                healthText.color = increaseGreen;
                healthIncreaseText.transform.parent.gameObject.SetActive(true);
                healthText.SetText("{0}", controlledCharacter.GetRawMaxHP());

                // Increases above 10 are shown as "X" to not overload the display
                if(controlledCharacter.GetRawMaxHP() - originalHP < 10)
                {
                    healthIncreaseText.SetText("+{0}", controlledCharacter.GetRawMaxHP() - originalHP);
                    healthIncreaseText.color = new Color32(105,255,115,255);
                }
                else
                {
                    healthIncreaseText.SetText("+X");
                    healthIncreaseText.color = Color.yellow;
                    healthText.color = new Color32(210, 120, 0, 255);
                }

                break;
            case "STR":
                strengthText.color = increaseGreen;
                strengthIncreaseText.transform.parent.gameObject.SetActive(true);
                strengthText.SetText("{0}", controlledCharacter.GetRawStrength());
                strengthIncreaseText.SetText("+{0}", controlledCharacter.GetRawStrength() - originalStrength);
                break;
            case "MAG":
                magicText.color = increaseGreen;
                magicIncreaseText.transform.parent.gameObject.SetActive(true);
                magicText.SetText("{0}", controlledCharacter.GetRawMagic());
                magicIncreaseText.SetText("+{0}", controlledCharacter.GetRawMagic() - originalMagic);
                break;
            case "DEX":
                dexterityText.color = increaseGreen;
                dexterityIncreaseText.transform.parent.gameObject.SetActive(true);
                dexterityText.SetText("{0}", controlledCharacter.GetRawDexterity());
                dexterityIncreaseText.SetText("+{0}", controlledCharacter.GetRawDexterity() - originalDexterity);
                break;
            case "FTH":
                faithText.color = increaseGreen;
                faithIncreaseText.transform.parent.gameObject.SetActive(true);
                faithText.SetText("{0}", controlledCharacter.GetRawFaith());
                faithIncreaseText.SetText("+{0}", controlledCharacter.GetRawFaith() - originalFaith);
                break;
        }
    }
}
