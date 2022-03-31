using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public Button[] joystickButtons;
    public Button pauseButton;

    public BattleController battleScreen;

    public Character[] allCharacters;

    public LevelUpScreen levelingScreen;
    public LoseGameScreen loseGameScreen;

    public Button DEBUG_BUTTON;
    public int DEBUG_MODE; //Debug mode activates 10x EXP for skill testing (mode = 1), or skips all encounters (mode = 2)

    // Set DEBUG to 0 naturally
    private void Start()
    {
        DEBUG_MODE = -1;
        ToggleDebugMode();
    }

    //movement is disabled whenever paused, during combat, or while moving.
    public void DisableJoystick(bool shrinkIt)
    {
        if (shrinkIt)
        {
            joystickButtons[0].transform.parent.gameObject.GetComponent<Animator>().SetTrigger("JoystickExit");
        }

        foreach (Button b in joystickButtons)
        {
            b.interactable = false;
        }
    }

    public void EnableJoystick(bool growIt)
    {
        if (growIt)
        {
            joystickButtons[0].transform.parent.gameObject.GetComponent<Animator>().SetTrigger("JoystickEnter");
        }

        foreach (Button b in joystickButtons)
        {
            b.interactable = true;
        }
    }

    public void ExitBattle()
    {
        // Remove the battle screen
        battleScreen.gameObject.SetActive(false);
        FindObjectOfType<BattleController>(true).unitCommandList.CloseCommandsMenu(false);

        // Turn this room back to a "normal" room after clearing the enemies.
        FindObjectOfType<FloorBuilder>().ClearRoom(FindObjectOfType<FieldCharacter>().currentRoom);

        // Reactivate the floor counter.
        FindObjectOfType<FloorBuilder>().floorNumberCounter.transform.parent.gameObject.SetActive(true);

        // Save the game
        SaveGame();
    }

    public IEnumerator EnterBattle(bool bossFight)
    {
        battleScreen.gameObject.SetActive(true);
        battleScreen.isBossBattle = bossFight; // Set whether this is a boss fight, which affects how the screen is laid out
        battleScreen.GetComponent<Animator>().SetTrigger("EnterBattle");
        battleScreen.AddCharactersToBattleScreen();

        // Wait until the battle starts for the pause button to be usable again
        pauseButton.interactable = false;

        battleScreen.ATB_Active = false;

        //deactivate the floor counter so it doesn't cover enemies.
        FindObjectOfType<FloorBuilder>().floorNumberCounter.transform.parent.gameObject.SetActive(false);

        // Animate Myrrh
        FindObjectOfType<Myrrh>().GetComponent<Animator>().SetTrigger("MyrrhExit");

        //update HP bars and remove ATB charge from each hero.
        foreach (HeroStatus heroHP in FindObjectsOfType<HeroStatus>())
        {
            // Remove ghost status if the unit was revived in the field before combat
            if(heroHP.owner.currentHP > 0)
            {
                heroHP.owner.GhostifyUnit(false);
            }

            // Start battles with 0 chi
            heroHP.owner.currentChi = 0;
            heroHP.owner.ATB_CurrentCharge = 0f;
            heroHP.UpdateHeroStatus();

            // Automatically set faces to neutral when a battle starts
            heroHP.owner.unitFace.SetFace("Neutral", 1f);
        }

        DisableJoystick(true);

        battleScreen.PopulateEnemies(); // Populate enemies and properly set the screen
        battleScreen.unitCommandList.CloseCommandsMenu(false);

        //update the battle hp/mp/atb bars.
        foreach (HeroStatus heroHP in FindObjectsOfType<HeroStatus>())
        {
            //set the gauge to 0 until the battle starts.
            heroHP.ATB_Gauge.value = 0f;
            heroHP.UpdateHeroStatus();
        }

        yield return new WaitForSeconds(2f);

        // In boss battles, wait an additional 3s as the boss appears on screen
        if (bossFight)
        {
            yield return new WaitForSeconds(3f);
        }

        // The battle starts- allow pausing again
        pauseButton.interactable = true;

        battleScreen.StartFirstTurn();

        yield return null;
    }

    //Returns all living heroes. Can be used to determine if any heroes are still alive (check for count > 0).
    public List<Character> FindLivingCharacters()
    {
        List<Character> livingHeroes = new List<Character>();

        foreach(Character c in allCharacters)
        {
            if(c.currentHP > 0)
            {
                livingHeroes.Add(c);
            }
        }

        return livingHeroes;
    }

    public IEnumerator LoseGame()
    {
        // Delete saved data when you lose
        PlayerPrefs.DeleteKey("SaveCode");

        yield return new WaitForSeconds(1.25f);

        //removes characters from the screen so they don't cover the game over screen.
        FindObjectOfType<BattleController>().RemoveCharactersFromBattleScreen(); 

        loseGameScreen.gameObject.SetActive(true);
        loseGameScreen.GetComponent<Animator>().SetTrigger("GameOver");
        loseGameScreen.SetHeadstones();

        DeleteSaveData();

        yield return null;
    }

    public void SaveGame()
    {
        FindObjectOfType<Savefile>().CreateSaveData();

        PlayerPrefs.SetString("SaveCode", FindObjectOfType<Savefile>().saveCode);
    }

    // Delete all data when you lose the game
    void DeleteSaveData()
    {
        FindObjectOfType<Savefile>().saveCode = "";
        FindObjectOfType<Savefile>().lootCode = "";

        PlayerPrefs.SetString("SaveCode", FindObjectOfType<Savefile>().saveCode);
        PlayerPrefs.SetString("LootCode", FindObjectOfType<Savefile>().lootCode);
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void ToggleDebugMode()
    {
        DEBUG_MODE++;

        if(DEBUG_MODE > 2)
        {
            DEBUG_MODE = 0;
        }

        switch (DEBUG_MODE)
        {
            case 0:
                DEBUG_BUTTON.GetComponent<Image>().color = Color.red;
                DEBUG_BUTTON.GetComponentInChildren<Text>().text = "N/A";
                break;
            case 1:
                DEBUG_BUTTON.GetComponent<Image>().color = Color.green;
                DEBUG_BUTTON.GetComponentInChildren<Text>().text = "EASY";
                break;
            case 2:
                DEBUG_BUTTON.GetComponent<Image>().color = Color.yellow;
                DEBUG_BUTTON.GetComponentInChildren<Text>().text = "SKIP";
                break;
        }
    }
}
