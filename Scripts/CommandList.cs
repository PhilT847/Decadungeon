using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CommandList : MonoBehaviour
{
    public Character currentCharacter;

    //when searching thru multiple spells or skills, keep track of the page number so each one can be listed
    public int currentPageNumber;

    public Button spellMenuButton;
    public Button skillMenuButton;

    //the panels used to select spells/skills from the unit's list
    public GameObject SpellsPanel;
    public GameObject SkillsPanel;

    //the three buttons that display spells/skills. Use parallel arrays to also alter the image and MP cost based on the spell.
    public Button[] spellButtons;
    public TextMeshProUGUI[] spellButtonNames;
    public Image[] spellImages;
    public TextMeshProUGUI[] spellMPCosts;
    public Ability[] heldSpells;

    public Button[] skillButtons;
    public TextMeshProUGUI[] skillButtonNames;
    public Image[] skillImages;
    public Ability[] heldSkills;

    // When enchanted or using a magic weapon, the "Attack" button shows an image of the element
    public Image attackElementImage;
    public Sprite[] elementImages; // 0 (none), 1 (fire), 2 (ice), 3 (lightning), 4 (holy), 5 (aura)

    public Button nextButton_spell;
    public Button previousButton_spell;

    public Button nextButton_skill;
    public Button previousButton_skill;
    
    //stop picking allies/enemies
    public Button cancelSelectionButton;

    // The image color changes based on the character in control
    public Image[] panelImages;
    public Color[] panelColors;

    public void SkipTurn()
    {
        FindObjectOfType<BattleController>().UnitSkipTurn();
    }

    public void SetNewCharacter(Character newCharacter)
    {
        ClearButtons();

        currentCharacter = newCharacter;

        spellMenuButton.interactable = (newCharacter.spellList.Count > 0);

        // Clear the Skills button until needed
        skillMenuButton.onClick.RemoveAllListeners();

        // If the current character only has one unique skill, change the "Skills" button. Otherwise, keep it as "Skills" or special text
        if (currentCharacter.chosenClass.hasClassSkill)
        {
            skillMenuButton.interactable = true;
            skillMenuButton.GetComponentInChildren<TextMeshProUGUI>().SetText(currentCharacter.skillList[0].abilityName);

            skillMenuButton.onClick.AddListener(() => ActivateClassAbility());
        }
        else
        {
            skillMenuButton.interactable = (newCharacter.skillList.Count > 0);
            skillMenuButton.GetComponentInChildren<TextMeshProUGUI>().SetText(currentCharacter.chosenClass.specialActionName);

            skillMenuButton.onClick.AddListener(() => OpenSkillsPanel());
        }

        // Change the menu color
        for(int i = 0; i < panelImages.Length; i++)
        {
            switch (currentCharacter.unitName)
            {
                case "Terra":
                    panelImages[i].color = panelColors[0];
                    break;
                case "Brick":
                    panelImages[i].color = panelColors[1];
                    break;
                case "Iris":
                    panelImages[i].color = panelColors[2];
                    break;
                case "Leon":
                    panelImages[i].color = panelColors[3];
                    break;
                default:
                    panelImages[i].color = currentCharacter.primaryColor;
                    break;
            }
        }

        // Set the element shown on the "Attack" button, if applicable
        ShowAttackElement();

        //transform.position = new Vector3(transform.position.x,currentCharacter.chosenClass.transform.position.y,0f);
    }

    public void OpenSpellsPanel()
    {
        FindObjectOfType<BattleController>().DeselectUnitButtons();

        SpellsPanel.SetActive(true);

        UpdateSpellsPanel(0);
    }

    public void OpenSkillsPanel()
    {
        FindObjectOfType<BattleController>().DeselectUnitButtons();

        SkillsPanel.SetActive(true);

        UpdateSkillsPanel(0);
    }

    public void ActivateClassAbility()
    {
        FindObjectOfType<BattleController>().DeselectUnitButtons();

        FindObjectOfType<BattleController>().SelectClassSkill();
    }

    void UpdateSpellsPanel(int pageNumber)
    {
        //since pages start at zero, round up but reduce this number by 1.
        float totalSpellPages = Mathf.Ceil(currentCharacter.spellList.Count / 3f) - 1;

        //set next/previous buttons up based on current page
        nextButton_spell.gameObject.SetActive(currentPageNumber < totalSpellPages);
        previousButton_spell.gameObject.SetActive(currentPageNumber > 0);

        for (int i = 0; i < 3; i++)
        {
            if (currentCharacter.spellList.Count > (i + 3 * currentPageNumber))
            {
                spellButtons[i].gameObject.SetActive(true);

                UpdateSpellButton(i, currentCharacter.spellList[i + 3 * currentPageNumber]);

                spellButtons[i].interactable = (currentCharacter.currentMP >= currentCharacter.spellList[i + 3 * currentPageNumber].GetComponent<Spell>().MP_Cost);
            }
            else
            {
                //clear button
                DisableButton(spellButtons[i]);
            }
        }
    }

    void UpdateSkillsPanel(int pageNumber)
    {
        //since pages start at zero, round up but reduce this number by 1.
        float totalSkillPages = Mathf.Ceil(currentCharacter.skillList.Count / 3f) - 1;

        //set next/previous buttons up based on current page
        nextButton_skill.gameObject.SetActive(currentPageNumber < totalSkillPages);
        previousButton_skill.gameObject.SetActive(currentPageNumber > 0);

        for (int i = 0; i < 3; i++)
        {
            if (currentCharacter.skillList.Count > (i + 3 * currentPageNumber))
            {
                skillButtons[i].gameObject.SetActive(true);

                UpdateSkillButton(i, currentCharacter.skillList[i + 3 * currentPageNumber]);

                // Skills that require Chi are not interactable if the unit has 0 Chi
                skillButtons[i].interactable = !currentCharacter.skillList[i + 3 * currentPageNumber].spendsChi || (currentCharacter.currentChi > 0);
            }
            else
            {
                //clear button
                DisableButton(skillButtons[i]);
            }
        }
    }

    public void UpdateSpellButton(int buttonIndex, Ability newAbility)
    {
        spellButtonNames[buttonIndex].SetText(newAbility.abilityName);
        spellImages[buttonIndex].sprite = newAbility.GetComponent<Spell>().abilityIcon;
        spellMPCosts[buttonIndex].SetText("{0}", newAbility.GetComponent<Spell>().MP_Cost);

        heldSpells[buttonIndex] = newAbility;
    }

    void UpdateSkillButton(int buttonIndex, Ability newAbility)
    {
        skillButtonNames[buttonIndex].SetText(newAbility.abilityName);
        skillImages[buttonIndex].sprite = newAbility.GetComponent<Ability>().abilityIcon;

        heldSkills[buttonIndex] = newAbility;
    }

    void DisableButton(Button thisButton)
    {
        thisButton.gameObject.SetActive(false);
    }

    public void NextPage(bool isSpells)
    {
        currentPageNumber++;

        if (isSpells)
        {
            UpdateSpellsPanel(currentPageNumber);
        }
        else
        {
            UpdateSkillsPanel(currentPageNumber);
        }
    }

    public void PreviousPage(bool isSpells)
    {
        currentPageNumber--;

        if (isSpells)
        {
            UpdateSpellsPanel(currentPageNumber);
        }
        else
        {
            UpdateSkillsPanel(currentPageNumber);
        }
    }

    public void ClearButtons()
    {
        for(int i = 0; i < 3; i++)
        {
            spellButtonNames[i].SetText("");
            spellMPCosts[i].SetText("");
            spellImages[i].sprite = null;

            skillButtonNames[i].SetText("");

            heldSpells[i] = null;
            heldSkills[i] = null;
        }
    }

    public void ReturnToMainCommandMenu()
    {
        currentPageNumber = 0;
        ClearButtons();

        SpellsPanel.SetActive(false);
        SkillsPanel.SetActive(false);
    }

    void ShowAttackElement()
    {
        switch (currentCharacter.equippedWeapon.weaponElement)
        {
            case "Physical":
                attackElementImage.sprite = elementImages[0];
                break;
            case "Fire":
                attackElementImage.sprite = elementImages[1];
                break;
            case "Ice":
                attackElementImage.sprite = elementImages[2];
                break;
            case "Lightning":
                attackElementImage.sprite = elementImages[3];
                break;
        }

        // Then, check for enchantments that override the weapon's base element
        switch (currentCharacter.weaponEnchant.enchantmentName)
        {
            case "Fire":
                attackElementImage.sprite = elementImages[1];
                break;
            case "Holy":
                attackElementImage.sprite = elementImages[4];
                break;
            case "Aura":
                attackElementImage.sprite = elementImages[5];
                break;
        }

        // When using "Physical" element, make the sprite clear so there's no white square on top of the button
        if(attackElementImage.sprite == null)
        {
            attackElementImage.color = Color.clear;
        }
        else
        {
            attackElementImage.color = Color.white;
        }
    }

    public void OpenCommandsMenu()
    {
        GetComponent<Animator>().SetTrigger("OpenCommands");
    }

    public void CloseCommandsMenu(bool animate)
    {
        // For an instant close, set animate to false. Otherwise, watch it move downwards
        if (animate)
        {
            GetComponent<Animator>().SetTrigger("CloseCommands");
        }
        else
        {
            GetComponent<Animator>().SetTrigger("InstantCloseCommands");
        }
    }

}
