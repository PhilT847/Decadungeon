using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameController gameController;
    public StatusWindow statusWindow;
    public SkillWindow skillWindow;
    public BagWindow bagWindow;

    public GameObject[] pauseWindows; //0 (status), 1 (skills), 2 (options)

    public int openPanel;

    // The pause panel exposed when pausing either outside or during combat
    public GameObject regularPause;
    public GameObject battlePause;

    public void OpenWindow(int windowIndex)
    {
        //So long as you're opening a new panel, animate it and prepare the page
        if (windowIndex != openPanel)
        {
            //play a noise and set the currently open window to inactive
            FindObjectOfType<AudioManager>().Play("turnPage");

            pauseWindows[openPanel].GetComponent<Animator>().SetTrigger("Panel_Out");

            switch (windowIndex)
            {
                case 0:

                    statusWindow.OpenStatusWindow();

                    break;
                case 1:

                    skillWindow.OpenSkillWindow();

                    break;
                case 2:

                    bagWindow.OpenInventory();

                    break;
                case 3:

                    break;
            }

            //Set the index of the currently open panel for animating
            openPanel = windowIndex;
            pauseWindows[openPanel].GetComponent<Animator>().SetTrigger("Panel_In");
        }
    }

    //Prepare the first "Status" window upon opening the pause menu
    public void OpenInitialWindow()
    {
        for(int i = 0; i < pauseWindows.Length; i++)
        {
            pauseWindows[i].GetComponent<Animator>().ResetTrigger("Panel_In");
            pauseWindows[i].GetComponent<Animator>().ResetTrigger("Panel_Out");
            pauseWindows[i].GetComponent<Animator>().Play("PanelOutsideIdle");
        }

        pauseWindows[0].GetComponent<Animator>().Play("PanelIdle");
        openPanel = 0;
        statusWindow.CheckStatus(0);

        // Ensure that the detail screen isn't open when opening the menu
        if (statusWindow.checkingDetailedScreen)
        {
            statusWindow.ToggleDetailedScreen();
        }
    }

    public void OpenPauseMenu()
    {
        //Freeze time while paused.
        Time.timeScale = 0f;

        // Play the open noise
        FindObjectOfType<AudioManager>().Play("openMenu");

        // When not in battle, open the normal pause menu
        if (!FindObjectOfType<GameController>().battleScreen.gameObject.activeSelf)
        {
            regularPause.SetActive(true);

            //Animate pause screen
            GetComponent<Animator>().SetTrigger("OpenPauseMenu");

            // Animate myrrh
            FindObjectOfType<Myrrh>().GetComponent<Animator>().SetTrigger("MyrrhExit");

            //Open/update the status menu
            OpenInitialWindow();

            //ensures that the status screen is always refreshed when opening
            statusWindow.CheckStatus(0);

            skillWindow.SetToCharacter(0);

            gameController.DisableJoystick(false);

            //since heroes are on the UI, they have to be descaled so that the pause menu can be read.
            Hero[] allHeroes = FindObjectsOfType<Hero>(true);

            for (int i = 0; i < allHeroes.Length; i++)
            {
                allHeroes[i].transform.localScale = Vector3.zero;
                //allHeroes[i].gameObject.SetActive(false);
            }
        }
        else // Otherwise, open the battle pause menu
        {
            battlePause.SetActive(true);
        }
    }

    public void ClosePauseMenu()
    {
        FindObjectOfType<AudioManager>().Play("closeMenu");

        // When not in combat, animate myrrh
        if (!FindObjectOfType<GameController>().battleScreen.gameObject.activeSelf)
        {
            FindObjectOfType<Myrrh>().GetComponent<Animator>().SetTrigger("MyrrhEnter");
        }

        //Return to normal time when unpaused.
        Time.timeScale = 1f;

        gameController.EnableJoystick(false);

        regularPause.SetActive(false);

        //If the treasure menu is open, then remove the "Drop Item" portion in case the player had a full inventory then dropped items while paused
        if (FindObjectOfType<TreasureRoom>(true).inventoryFullPanel.activeSelf)
        {
            FindObjectOfType<TreasureRoom>(true).CancelDroppingItem();
        }

        Hero[] allHeroes = FindObjectsOfType<Hero>(true);

        for (int i = 0; i < allHeroes.Length; i++)
        {
            allHeroes[i].transform.localScale = new Vector3(18f,18f,1f);
            //allHeroes[i].gameObject.SetActive(true);
        }
    }

    public void CloseBattlePause()
    {
        //Return time when unpausing
        Time.timeScale = 1f;

        FindObjectOfType<AudioManager>().Play("closeMenu");
        battlePause.SetActive(false);
    }
}
