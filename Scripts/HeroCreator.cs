using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeroCreator : MonoBehaviour
{
    public GameController gameController;

    public string[] unitTexts;
    public string[] classDescriptions;

    public Button[] selectButtons;
    public Button confirmButton;

    //index of the current character from the GameController
    public int currentCharacter;

    //an index that corresponds to a classObject
    public int selectedClass;

    public GameObject[] classObjects;

    public TextMeshProUGUI characterInfo;
    public TextMeshProUGUI classInfo;

    //the transform where hero objects are shown in the display, as well as their eventual spawn location in the Battle screen
    public RectTransform[] heroLocations;
    public RectTransform spawnLocation;

    public RectTransform currentHeroBorder;

    public RectTransform heroDisplay;

    private void Start()
    {
        StartCoroutine(OpenHeroCreator());
    }

    //used by buttons in the creator
    public void SetSelectedClass(int c)
    {
        //If there's already a hero in the display, destroy it before adding another.
        if (heroLocations[currentCharacter].GetComponentInChildren<Hero>())
        {
            Destroy(heroLocations[currentCharacter].GetComponentInChildren<Hero>().gameObject);
        }

        selectedClass = c;

        //Spawn the display unit and set them to the desired unit.
        var displayUnit = Instantiate(classObjects[selectedClass], heroLocations[currentCharacter]);

        displayUnit.GetComponent<Hero>().DisplayHero(gameController.allCharacters[currentCharacter]);

        displayUnit.transform.localScale = new Vector3(32f,32f,32f);
        displayUnit.transform.localPosition = new Vector3(-12f, -70f, 0f);

        classInfo.SetText(classDescriptions[selectedClass]);

        DeactivateSelectorButton(c);
    }

    public void SelectClass()
    {
        StartCoroutine(SelectHero());
    }

    public IEnumerator OpenHeroCreator()
    {
        // Animate the staircase going out (which went in on the menu)
        FindObjectOfType<FieldCharacter>().staircaseAnim.SetTrigger("ForcedExit");

        // When playing without save data, load the hero creator. Otherwise, begin the game from where you left off
        if (!FindObjectOfType<Savefile>().playingSavedGame)
        {
            confirmButton.interactable = false;

            currentHeroBorder.GetComponent<Image>().color = Color.clear;

            yield return new WaitForSeconds(0.75f);

            heroLocations[currentCharacter].GetComponent<Animator>().SetTrigger("HeroEntry");

            //Set the character/class to Terra/Cleric to begin.
            currentCharacter = 0;

            SetSelectedClass(2);

            yield return new WaitForSeconds(1.25f);

            SetCharacterInfo(currentCharacter);

            currentHeroBorder.position = heroLocations[0].position;
            currentHeroBorder.GetComponent<Image>().color = Color.white;

            confirmButton.interactable = true;
        }
        else
        {
            AutoGenerateCharacters();

            // Use functions from BeginGame(), then close the menu.

            // Animate Myrrh
            FindObjectOfType<Myrrh>().GetComponent<Animator>().SetTrigger("MyrrhEnter");

            // Color in the unit shadows
            foreach (GameObject g in GameObject.FindGameObjectsWithTag("UnitShadow"))
            {
                g.GetComponent<SpriteRenderer>().color = new Color32(40, 36, 36, 255);
            }

            Destroy(gameObject);
        }

        yield return null;
    }

    public IEnumerator BeginGame()
    {
        yield return new WaitForSeconds(1f);

        FindObjectOfType<FieldCharacter>().staircaseAnim.SetTrigger("Descend");

        yield return new WaitForSeconds(0.75f);

        // Animate Myrrh
        FindObjectOfType<Myrrh>().GetComponent<Animator>().SetTrigger("MyrrhEnter");

        // Color in the unit shadows
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("UnitShadow"))
        {
            g.GetComponent<SpriteRenderer>().color = new Color32(40, 36, 36, 255);
        }

        // Save your game with these new characters
        FindObjectOfType<GameController>().SaveGame();

        Destroy(gameObject);

        yield return null;
    }

    public IEnumerator SelectHero()
    {
        confirmButton.interactable = false;

        var newHero = Instantiate(classObjects[selectedClass], spawnLocation);

        newHero.transform.localPosition = new Vector3(-200f, 95f - (currentCharacter * 85f), 0f);

        newHero.GetComponent<Hero>().HeroSetup(gameController.allCharacters[currentCharacter]);

        heroLocations[currentCharacter].GetComponent<Animator>().SetTrigger("BeginMoving");

        // You selected a hero- make them smile for a moment
        heroLocations[currentCharacter].GetComponentInChildren<Face>().SetFace("Happy", 1.5f);

        currentHeroBorder.GetComponent<Image>().color = Color.clear;

        //Cycle through each character. If each character has been created, finish the creator.
        if (currentCharacter < 3)
        {
            currentCharacter++;

            heroLocations[currentCharacter].GetComponent<Animator>().SetTrigger("HeroEntry");

            // Set their class to create the object
            switch (currentCharacter)
            {
                case 1: //Brick; Knight
                    SetSelectedClass(0);
                    break;
                case 2: //Iris; Mage
                    SetSelectedClass(1);
                    break;
                case 3: //Leon; Rogue
                    SetSelectedClass(3);
                    break;
            }

            yield return new WaitForSeconds(1.25f);

            SetCharacterInfo(currentCharacter);
            currentHeroBorder.position = heroLocations[currentCharacter].position;
            currentHeroBorder.GetComponent<Image>().color = Color.white;

            confirmButton.interactable = true;
        }
        else //Completed!
        {
            StartCoroutine(BeginGame());
        }

        yield return null;
    }

    //Deactivate the selected class button and activate all others.
    void DeactivateSelectorButton(int index)
    {
        for(int i = 0; i < selectButtons.Length; i++)
        {
            selectButtons[i].interactable = i != index;
        }
    }

    void SetCharacterInfo(int index)
    {
        string fullText = "";

        fullText += gameController.allCharacters[index].unitName + ", " + unitTexts[index]; //"\n\n" + unitTexts[index];

        characterInfo.SetText(fullText);
    }

    // Generates characters from save data
    void AutoGenerateCharacters()
    {
        currentCharacter = 0;

        for(int i = 0; i < 4; i++)
        {
            int addIndex = 72 * currentCharacter;
            int chosenClass = FindObjectOfType<SavedHeroLoader>().GetIntegerValueFrom('N', 'N', 'N', FindObjectOfType<Savefile>().saveCode[7 + addIndex]);//Random.Range(0, 9); //PlayerPrefs.GetString("SaveCode")[7 + (66 * currentCharacter)];

            var newHero = Instantiate(classObjects[chosenClass], spawnLocation);

            newHero.transform.localPosition = new Vector3(-200f, 95f - (currentCharacter * 85f), 0f);

            newHero.GetComponent<Hero>().HeroSetup(gameController.allCharacters[currentCharacter]);

            // Use the SavedHeroLoader to create loaded heroes and equipment
            FindObjectOfType<SavedHeroLoader>().LoadStats(gameController.allCharacters[currentCharacter], currentCharacter);

            currentCharacter++;
        }

        // After characters load, load in the inventory
        FindObjectOfType<SavedHeroLoader>().LoadInventory();

        // Once characters/inventory loads, move to the correct floor and set the boss/treasure
        FindObjectOfType<SavedHeroLoader>().SetFloorAndBoss();

        // Remove any items that players already took from the loot pool
        FindObjectOfType<Savefile>().AdjustLootFromCode();
    }
}
