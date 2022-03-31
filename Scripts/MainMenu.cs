using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public HowToPlayMenu howToPlayMenu;

    public Button[] mainButtons; //"Start", "Load Saved", and "How to Play" buttons

    private void Awake()
    {
        // You can only load a saved game if a valid save code exists
        mainButtons[2].interactable = PlayerPrefs.GetString("SaveCode") != "";

        FindObjectOfType<Savefile>().playingSavedGame = false;
    }

    public void StartGame(bool loadingData)
    {
        StartCoroutine(SetupGame(loadingData));
    }

    public void OpenHowToPlay()
    {
        howToPlayMenu.gameObject.SetActive(true);
        howToPlayMenu.OpenMenu();

        foreach(Button b in mainButtons)
        {
            b.gameObject.SetActive(false);
        }
    }

    public void ReturnToMenu()
    {
        foreach (Button b in mainButtons)
        {
            b.gameObject.SetActive(true);
        }
    }

    public IEnumerator SetupGame(bool loadingData)
    {
        FindObjectOfType<Savefile>().playingSavedGame = loadingData;

        // When starting a new game, delete save data
        if (!loadingData)
        {
            DeleteSaveData();
        }

        GetComponent<Animator>().SetTrigger("BeginGame");

        yield return new WaitForSeconds(1f);

        SceneManager.LoadScene(1);

        yield return null;
    }

    // Delete the saved hero and loot codes
    public void DeleteSaveData()
    {
        FindObjectOfType<Savefile>().saveCode = "";
        FindObjectOfType<Savefile>().lootCode = "";

        PlayerPrefs.SetString("SaveCode", FindObjectOfType<Savefile>().saveCode);
        PlayerPrefs.SetString("SaveCode", FindObjectOfType<Savefile>().lootCode);
    }
}
