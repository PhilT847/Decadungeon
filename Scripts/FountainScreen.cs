using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FountainScreen : MonoBehaviour
{
    public void OpenFountainMenu()
    {
        FindObjectOfType<GameController>().DisableJoystick(true);
        gameObject.SetActive(true);
        GetComponent<Animator>().SetTrigger("OpenFountain");

        // Animate Myrrh
        FindObjectOfType<Myrrh>().GetComponent<Animator>().SetTrigger("MyrrhExit");
    }

    public void UseFountain()
    {
        for(int i = 0; i < FindObjectOfType<GameController>().allCharacters.Length; i++)
        {
            FindObjectOfType<GameController>().allCharacters[i].FullRestore();
        }

        CloseFountainMenu(true);
    }

    /*
    public void SelectRestoredUnit(int heroIndex)
    {
        Character chosenHero = FindObjectOfType<GameController>().allCharacters[heroIndex];

        chosenHero.FullRestore();

        CloseFountainMenu();
    }
    */

    public void CloseFountainMenu(bool usedFountain)
    {
        // Animate Myrrh
        FindObjectOfType<Myrrh>().GetComponent<Animator>().SetTrigger("MyrrhEnter");

        // Clears out the fountain room, turning it into a normal room, unless you Skipped it, in which is just closes
        if (usedFountain)
        {
            FindObjectOfType<FloorBuilder>().ClearRoom(FindObjectOfType<FieldCharacter>().currentRoom);
        }

        FindObjectOfType<GameController>().EnableJoystick(true);
        gameObject.SetActive(false);
    }
}
