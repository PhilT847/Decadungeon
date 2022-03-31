using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelUpScreen : MonoBehaviour
{
    public LevelUpPanel[] allLevelPanels;

    public TextMeshProUGUI expGainedText;

    public Button closeButton; // Close button can't activate until all EXP is earned.

    public Slider rankSlider; // The slider with the player's rank

    // Defeating a boss initializes the Myrrh gain
    public bool defeatedBoss;

    public void InitializeLevelScreen(bool beatBoss)
    {
        GetComponent<Animator>().ResetTrigger("CloseScreen");
        GetComponent<Animator>().SetTrigger("OpenScreen");

        defeatedBoss = beatBoss;

        closeButton.gameObject.SetActive(false);

        FindObjectOfType<GameController>().DisableJoystick(true);

        //use the battleExp integer found in the battleController to determine exp gained.
        expGainedText.SetText("EXP Earned: {0}", FindObjectOfType<BattleController>().battleExp);

        //set each panel based on unit stats.
        foreach (LevelUpPanel panel in allLevelPanels)
        {
            panel.InitializePanel();
        }

        StartCoroutine(IncreaseCharacterEXP(FindObjectOfType<BattleController>().battleExp));
    }

    public void CloseLevelScreen()
    {
        StartCoroutine(EndLevelUp());
    }

    public IEnumerator EndLevelUp()
    {
        GetComponent<Animator>().SetTrigger("CloseScreen");
        closeButton.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.8f);

        // If you beat normal enemies, return the joystick. When beating a boss, add Myrrh
        if (!defeatedBoss)
        {
            FindObjectOfType<Myrrh>().GetComponent<Animator>().SetTrigger("MyrrhEnter");
            FindObjectOfType<GameController>().EnableJoystick(true);

            // Unless gaining myrrh, you can pause once again
            FindObjectOfType<GameController>().pauseButton.interactable = true;
        }
        else
        {
            FindObjectOfType<Myrrh>().StartCoroutine(FindObjectOfType<Myrrh>().GainMyrrh());
        }

        gameObject.SetActive(false);

        yield return null;
    }

    //Grant EXP to all surviving units.
    public IEnumerator IncreaseCharacterEXP(int grantedEXP)
    {
        //The opening animation takes 1s. Wait an additional second before adding exp.
        yield return new WaitForSeconds(2f);

        List<Character> livingCharacters = new List<Character>();

        //EXP is only applied to living characters.
        foreach (Character c in FindObjectsOfType<Character>(true))
        {
            if (c.currentHP > 0)
            {
                livingCharacters.Add(c);
            }
        }

        int expGranted = grantedEXP;

        while (expGranted > 0)
        {
            foreach (LevelUpPanel panel in allLevelPanels)
            {
                if (livingCharacters.Contains(panel.controlledCharacter))
                {
                    panel.controlledCharacter.exp += 1;

                    panel.expSlider.value = panel.controlledCharacter.exp;

                    if (panel.controlledCharacter.exp >= panel.controlledCharacter.expToNextLevel)
                    {
                        //Add EXP to each unit at a constant rate.
                        IncreaseCharacterLevel(panel, panel.controlledCharacter);
                        panel.expSlider.maxValue = panel.controlledCharacter.expToNextLevel;
                    }

                    expGranted -= 1;
                }
            }

            if(expGranted < 200)
            {
                yield return new WaitForSeconds(0.01f); //100 EXP is distributed per second.
            }
            else if(expGranted < 500)
            {
                yield return new WaitForSeconds(0.0034f); //When total EXP is above 200, distribute 300 per second.
            }
            else
            {
                yield return new WaitForSeconds(0.00125f); //When total EXP is above 500, distribute 800 per second.
            }
        }

        //Allow the player to close the menu once all EXP is gained.
        closeButton.gameObject.SetActive(true);

        yield return null;
    }

    public void IncreaseCharacterLevel(LevelUpPanel panel, Character leveledCharacter)
    {
        if(leveledCharacter.level < 99)
        {
            leveledCharacter.level++;
            panel.levelText.SetText("{0}", leveledCharacter.level);

            leveledCharacter.exp -= leveledCharacter.expToNextLevel;
            leveledCharacter.expToNextLevel = (int) (leveledCharacter.expToNextLevel * 1.1f); //need 10% more exp per level.

            // Increase stats for the hero based on growths.
            RollForStats(panel, leveledCharacter.chosenClass);

            // Knights and Liches have physical armor equal to 1 (given at level 1 in HeroSetup()) plus 1 at every even level
            if (leveledCharacter.chosenClass.className == "Knight")
            {
                if(leveledCharacter.level % 2 == 0)
                {
                    leveledCharacter.physicalDefense += 1;
                }
            }

            // Monk's "Fists" weapon increases in power based on level
            if (leveledCharacter.chosenClass.className == "Monk")
            {
                // Increase in power by 2 per level until level 10, after which it increases by 4 per level
                if (leveledCharacter.level < 11)
                {
                    leveledCharacter.equippedWeapon.weaponMight = leveledCharacter.level * 2;
                }
                else
                {
                    leveledCharacter.equippedWeapon.weaponMight = 20 + ((leveledCharacter.level - 10) * 4);
                }
            }

            //check for and learn new skills. When learning a new skill, display it in the level up panel
            if (leveledCharacter.chosenClass.CheckForLearnedSkills(leveledCharacter.level))
            {
                panel.newSkillDisplay.SetActive(true);
            }
        }
    }

    //Roll based on growths. Note that even a <5% rate is upped to at least 5%.
    public void RollForStats(LevelUpPanel panel, Hero leveledHero)
    {
        int hpRoll = Random.Range(1, 101);

        //even if the unit's growth is less than 10%, rolling 10 or lower grants +1 to ensure units have some chance of increasing the stat.

        //Unlike other stats, HP can go up by 1 or 2 when it levels up as it is naturally higher than other stats. Both the first and second rolls can be 1's or 2's.

        if (leveledHero.classOwner.maxHP < 999)
        {
            if (hpRoll <= leveledHero.classOwner.hpGrowth || hpRoll <= 10)
            {
                leveledHero.classOwner.maxHP += Random.Range(1, 3);
                panel.VisualizeStatIncrease("HP");

                int hpSecondRoll = Random.Range(1, 101);

                if (hpSecondRoll <= leveledHero.classOwner.hpGrowth / 2)
                {
                    leveledHero.classOwner.maxHP += Random.Range(1, 3);
                    panel.VisualizeStatIncrease("HP");
                }
            }

            // HP increases even faster based on level. Higher levels are guaranteed more HP per level. HP growth also affects this increase
            if (leveledHero.classOwner.level > 4)
            {
                int levelBasedHPGrowth = leveledHero.classOwner.level / 5;

                // Characters with high combined personal/class HP growth get 1 extra
                if(leveledHero.classOwner.hpGrowth >= 100)
                {
                    levelBasedHPGrowth += 1;
                }
                else if(leveledHero.classOwner.hpGrowth <= 50) // Characters with low HP growths gain less per level
                {
                    levelBasedHPGrowth -= 1;
                }

                // Increase by the given modifier (if it's at least +1)
                if(levelBasedHPGrowth > 0)
                {
                    leveledHero.classOwner.maxHP += levelBasedHPGrowth;
                    panel.VisualizeStatIncrease("HP");
                }
            }
        }

        if(leveledHero.classOwner.strength < 99)
        {
            int strRoll = Random.Range(1, 101);

            if (strRoll <= leveledHero.classOwner.strGrowth || strRoll <= 10)
            {
                leveledHero.classOwner.strength += 1;
                panel.VisualizeStatIncrease("STR");

                if (leveledHero.classOwner.strength < 99)
                {
                    int strSecondRoll = Random.Range(1, 101);

                    if (strSecondRoll <= leveledHero.classOwner.strGrowth / 2)
                    {
                        leveledHero.classOwner.strength += 1;
                        panel.VisualizeStatIncrease("STR");
                    }
                }
            }
        }

        if (leveledHero.classOwner.magic < 99)
        {
            int magRoll = Random.Range(1, 101);

            if (magRoll <= leveledHero.classOwner.magGrowth || magRoll <= 10)
            {
                leveledHero.classOwner.magic += 1;

                if(leveledHero.className != "Monk")
                {
                    leveledHero.classOwner.maxMP += 1;
                }

                panel.VisualizeStatIncrease("MAG");

                if (leveledHero.classOwner.magic < 99)
                {
                    int magSecondRoll = Random.Range(1, 101);

                    if (magSecondRoll <= leveledHero.classOwner.magGrowth / 2)
                    {
                        leveledHero.classOwner.magic += 1;

                        if (leveledHero.className != "Monk")
                        {
                            leveledHero.classOwner.maxMP += 1;
                        }

                        panel.VisualizeStatIncrease("MAG");
                    }
                }
            }
        }

        if (leveledHero.classOwner.dexterity < 99)
        {
            int dexRoll = Random.Range(1, 101);

            if (dexRoll <= leveledHero.classOwner.dexGrowth || dexRoll <= 10)
            {
                leveledHero.classOwner.dexterity += 1;
                panel.VisualizeStatIncrease("DEX");

                if (leveledHero.classOwner.dexterity < 99)
                {
                    int dexSecondRoll = Random.Range(1, 101);

                    if (dexSecondRoll <= leveledHero.classOwner.dexGrowth / 2)
                    {
                        leveledHero.classOwner.dexterity += 1;
                        panel.VisualizeStatIncrease("DEX");
                    }
                }
            }
        }

        if (leveledHero.classOwner.faith < 99)
        {
            int fthRoll = Random.Range(1, 101);

            if (fthRoll <= leveledHero.classOwner.fthGrowth || fthRoll <= 10)
            {
                leveledHero.classOwner.faith += 1;
                panel.VisualizeStatIncrease("FTH");

                if (leveledHero.classOwner.faith < 99)
                {
                    int fthSecondRoll = Random.Range(1, 101);

                    if (fthSecondRoll <= leveledHero.classOwner.fthGrowth / 2)
                    {
                        leveledHero.classOwner.faith += 1;
                        panel.VisualizeStatIncrease("FTH");
                    }
                }
            }
        }
    }

}
