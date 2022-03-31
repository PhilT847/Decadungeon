using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillWindow : MonoBehaviour
{
    public TextMeshProUGUI descriptionText;

    /*
    public Button[] abilityButtons;
    public Image[] abilityImages;
    public string[] abilityDescriptions;
    */

    public SkillButton[] allSkillButtons;

    public List<Ability> shownAbilities;

    public Character chosenCharacter;
    private int chosenIndex; //the saved index of the chosen character, used by ActivateOtherWindow()

    public Ability selectedAbility;

    public Button[] heroSelectButtons;
    public Button castButton;
    public Button moreButton; //"More" button that appears if the character has >10 abilities.
    public Image statusPortraitBorder; //the gold border that goes around the selected hero in the Skills menu

    public Slider[] HP_Sliders;
    public Slider[] MP_Sliders;

    //Checks whether the first window (that is, the first 10 skills) are displayed.
    private bool FirstWindowOpen;

    public void SetToCharacter(int heroIndex)
    {
        //When switching to a new hero, always reset to the first window.
        if (heroIndex != chosenIndex)
        {
            FirstWindowOpen = true;
        }

        //Move the gold border to selected hero
        statusPortraitBorder.transform.position = heroSelectButtons[heroIndex].transform.position;

        chosenIndex = heroIndex;

        descriptionText.SetText("Select an ability to read what it does.");

        chosenCharacter = FindObjectOfType<GameController>().allCharacters[heroIndex];

        List<Ability> allCharacterAbilities = new List<Ability>();

        //Add all character skills and spells to the buttons.
        foreach (Ability charPassive in chosenCharacter.passiveList)
        {
            allCharacterAbilities.Add(charPassive);
        }

        foreach (Ability charSkill in chosenCharacter.skillList)
        {
            allCharacterAbilities.Add(charSkill);
        }

        foreach (Ability charSpell in chosenCharacter.spellList)
        {
            allCharacterAbilities.Add(charSpell);
        }

        shownAbilities = allCharacterAbilities;

        //There are always 20 buttons... deactivate any button that's outside the character's total ability count
        for(int i = 0; i < allSkillButtons.Length; i++)
        {
            if(i < shownAbilities.Count && ((FirstWindowOpen && i < 10) || (!FirstWindowOpen && i > 9)))
            {
                allSkillButtons[i].gameObject.SetActive(true);
                allSkillButtons[i].SetToAbility(shownAbilities[i]);
                //UpdateButton(i, shownAbilities[i]);
            }
            else
            {
                allSkillButtons[i].gameObject.SetActive(false);
            }
        }

        //cast button removed by default. More button active if the character has >10 skills.
        castButton.gameObject.SetActive(false);

        ActivateCastButtons(false);
        moreButton.gameObject.SetActive(shownAbilities.Count > 10);
    }

    // Updates each HP/MP bar on the hero icons
    void UpdateSliders()
    {
        Character[] allCharacters = FindObjectOfType<GameController>().allCharacters;

        for(int i = 0; i < HP_Sliders.Length; i++)
        {
            HP_Sliders[i].value = (float) allCharacters[i].currentHP / allCharacters[i].maxHP;
            MP_Sliders[i].value = (float) allCharacters[i].currentMP / allCharacters[i].maxMP;
        }
    }

    //Cast a spell
    public void CastFromMenu(int targetHeroIndex)
    {
        Character target = FindObjectOfType<GameController>().allCharacters[targetHeroIndex];

        // The spell is either white or green magic
        WhiteMagic whiteMagicSpell = null;
        WildMagic greenMagicSpell = null;

        // You can't heal dead units. If using a spell that doesn't revive, just return
        if (selectedAbility.GetComponent<WhiteMagic>())
        {
            whiteMagicSpell = selectedAbility.GetComponent<WhiteMagic>();

            if(!whiteMagicSpell.revivesAllies && target.currentHP < 1)
            {
                return;
            }
        }
        else
        {
            greenMagicSpell = selectedAbility.GetComponent<WildMagic>();

            if(target.currentHP < 1)
            {
                return;
            }
        }

        //remove the hero target buttons
        ActivateCastButtons(false);

        chosenCharacter.currentMP -= selectedAbility.GetComponent<Spell>().MP_Cost;

        //prevent continuous casting if MP isn't high enough for it.
        castButton.interactable = chosenCharacter.currentMP >= selectedAbility.GetComponent<Spell>().MP_Cost;
        
        if (whiteMagicSpell != null)
        {
            if (!whiteMagicSpell.revivesAllies && target.currentHP > 0)
            {
                target.currentHP += whiteMagicSpell.potencyBase + (int)(chosenCharacter.faith * whiteMagicSpell.potencyGrowth);

                if (target.currentHP > target.maxHP)
                {
                    target.currentHP = target.maxHP;
                }

                chosenCharacter.GetComponent<Character>().lifetimeHealingDealt += whiteMagicSpell.potencyBase + (int)(chosenCharacter.faith * whiteMagicSpell.potencyGrowth);
            }

            //Reviving allies increases their HP to the spell potency value.
            if (whiteMagicSpell.revivesAllies)
            {
                if (whiteMagicSpell.abilityName!= "Miracle") //revive only
                {
                    if(target.currentHP == 0)
                    {
                        target.currentHP = whiteMagicSpell.potencyBase + (int)(chosenCharacter.faith * whiteMagicSpell.potencyGrowth);

                        if (target.currentHP > target.maxHP)
                        {
                            target.currentHP = target.maxHP;
                        }

                        chosenCharacter.GetComponent<Character>().lifetimeHealingDealt += whiteMagicSpell.potencyBase + (int)(chosenCharacter.faith * whiteMagicSpell.potencyGrowth);
                    }
                }
                else //The "Miracle" spell can either heal or revive based on allied HP.
                {
                    for(int i = 0; i < FindObjectOfType<GameController>().allCharacters.Length; i++)
                    {
                        target = FindObjectOfType<GameController>().allCharacters[i];

                        if (target.currentHP == 0)
                        {
                            target.currentHP = whiteMagicSpell.potencyBase + (int)(chosenCharacter.faith * whiteMagicSpell.potencyGrowth);

                            if (target.currentHP > target.maxHP)
                            {
                                target.currentHP = target.maxHP;
                            }

                            chosenCharacter.GetComponent<Character>().lifetimeHealingDealt += whiteMagicSpell.potencyBase + (int)(chosenCharacter.faith * whiteMagicSpell.potencyGrowth);
                        }
                        else //heal
                        {
                            target.currentHP += whiteMagicSpell.potencyBase + (int)(chosenCharacter.faith * whiteMagicSpell.potencyGrowth);

                            if (target.currentHP > target.maxHP)
                            {
                                target.currentHP = target.maxHP;
                            }

                            chosenCharacter.GetComponent<Character>().lifetimeHealingDealt += whiteMagicSpell.potencyBase + (int)(chosenCharacter.faith * whiteMagicSpell.potencyGrowth);
                        }
                    }
                }
            }
        }
        else
        {
            if (greenMagicSpell.areaOfEffect)
            {
                for (int i = 0; i < FindObjectOfType<GameController>().allCharacters.Length; i++)
                {
                    target = FindObjectOfType<GameController>().allCharacters[i];

                    if(target.currentHP > 0)
                    {
                        target.currentHP += greenMagicSpell.potencyBase + (int)(chosenCharacter.faith * greenMagicSpell.potencyGrowth);

                        if (target.currentHP > target.maxHP)
                        {
                            target.currentHP = target.maxHP;
                        }

                        chosenCharacter.GetComponent<Character>().lifetimeHealingDealt += greenMagicSpell.potencyBase + (int)(chosenCharacter.faith * greenMagicSpell.potencyGrowth);
                    }
                }
            }
            else if (target.currentHP > 0)
            {
                target.currentHP += greenMagicSpell.potencyBase + (int)(chosenCharacter.faith * greenMagicSpell.potencyGrowth);

                if (target.currentHP > target.maxHP)
                {
                    target.currentHP = target.maxHP;
                }
                
                chosenCharacter.GetComponent<Character>().lifetimeHealingDealt += greenMagicSpell.potencyBase + (int)(chosenCharacter.faith * greenMagicSpell.potencyGrowth);
            }
        }

        UpdateSliders();
    }

    /*
    void UpdateButton(int buttonIndex, Ability thisAbility)
    {
        abilityImages[buttonIndex].sprite = thisAbility.abilityIcon;
        abilityButtons[buttonIndex].GetComponentInChildren<TextMeshProUGUI>().SetText(thisAbility.abilityName);
        abilityDescriptions[buttonIndex] = thisAbility.abilityDescription;
    }
    */

    public void ActivateCastButtons(bool isActivated)
    {
        //If the hero selection buttons are already active, remove them. Otherwise, attempt to cast.
        if (heroSelectButtons[0].gameObject.activeSelf)
        {
            isActivated = false;
            castButton.GetComponent<Image>().color = Color.blue;
            castButton.GetComponentInChildren<TextMeshProUGUI>().SetText("Cast");
            descriptionText.SetText(selectedAbility.abilityDescription);
        }
        else if (isActivated)
        {
            castButton.GetComponent<Image>().color = Color.red;
            castButton.GetComponentInChildren<TextMeshProUGUI>().SetText("Cancel");
            descriptionText.SetText("Select a target.");
        }

        for (int i = 0; i < heroSelectButtons.Length; i++)
        {
            heroSelectButtons[i].gameObject.SetActive(isActivated);
        }
    }

    public void PressSkillButton(int skillIndex)
    {
        // Make all buttons white, and the selected button gray
        for(int i = 0; i < allSkillButtons.Length; i++)
        {
            allSkillButtons[i].buttonImage.color = Color.white;
        }

        allSkillButtons[skillIndex].buttonImage.color = new Color32(255, 120, 120, 255);

        selectedAbility = allSkillButtons[skillIndex].heldAbility;

        descriptionText.SetText(allSkillButtons[skillIndex].heldAbility.abilityDescription);

        // Cancel casting if attempting to cast
        if (castButton.GetComponent<Image>().color != Color.blue)
        {
            castButton.GetComponent<Image>().color = Color.blue;
            castButton.GetComponentInChildren<TextMeshProUGUI>().SetText("Cast");

            for (int i = 0; i < heroSelectButtons.Length; i++)
            {
                heroSelectButtons[i].gameObject.SetActive(false);
            }
        }

        //if the selected ability can be casted from the menu, make the cast button appear while not in battle. Otherwise, set it invisible.
        castButton.gameObject.SetActive(selectedAbility.castableInMenu && !FindObjectOfType<GameController>().battleScreen.gameObject.activeSelf);

        //Spells can only be used if the unit possesses the MP for it (and is currently alive). 
        if (selectedAbility.GetComponent<Spell>())
        {
            castButton.interactable = chosenCharacter.currentMP >= selectedAbility.GetComponent<Spell>().MP_Cost && chosenCharacter.currentHP > 0;
            //castButton.GetComponentInChildren<TextMeshProUGUI>().SetText("Cast [{0}]", selectedAbility.GetComponent<Spell>().MP_Cost);
        }
        else
        {
            castButton.interactable = true;
        }
    }

    public void ActivateOtherWindow()
    {
        FirstWindowOpen = !FirstWindowOpen;

        SetToCharacter(chosenIndex);
    }

    public void OpenSkillWindow()
    {
        SetToCharacter(0);

        UpdateSliders();

        //When opening the menu, always set skills to first page.
        FirstWindowOpen = true;
    }
}
