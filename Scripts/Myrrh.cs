using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Myrrh : MonoBehaviour
{
    public Animator myrrhScreenAnim;
    public bool myrrhScreenOpen;

    public int myrrhCount;

    public Image fillImage;
    public Sprite[] myrrhSprites;

    public TextMeshProUGUI countText;

    public Slider[] hpSliders;
    public Slider[] mpSliders;

    private void Start()
    {
        InitializeMyrrhCounter();
    }

    public void ToggleMyrrhScreen()
    {
        GetComponent<Animator>().SetTrigger("PushButton");

        // Open the screen if it's closed
        if (!myrrhScreenOpen)
        {
            myrrhScreenOpen = true;
            myrrhScreenAnim.SetTrigger("OpenScreen");
            FindObjectOfType<GameController>().DisableJoystick(true);

            // Show current HP/MP for each hero
            UpdateStatusSliders();
        }
        else // If open, close the screen
        {
            CloseMyrrhScreen();
        }

        // While the myrrh screen is up, you can't pause
        FindObjectOfType<GameController>().pauseButton.interactable = !myrrhScreenOpen;
    }

    public void CloseMyrrhScreen()
    {
        myrrhScreenOpen = false;

        myrrhScreenAnim.SetTrigger("CloseScreen");
        FindObjectOfType<GameController>().EnableJoystick(true);
        FindObjectOfType<GameController>().pauseButton.interactable = true;
    }

    public void SpendMyrrh(int heroIndex)
    {
        if(myrrhCount > 0)
        {
            GetComponent<Animator>().SetTrigger("SpendMyrrh");

            FindObjectOfType<GameController>().allCharacters[heroIndex].FullRestore();

            // Play sound


            myrrhCount--;
            
            ChangeMyrrhFillSprite();
            countText.SetText("{0}", myrrhCount);

            countText.color = Color.white;

            UpdateStatusSliders();
        }
    }

    // When starting a game, set the myrrhCounter to the required sprite/fill amount
    void InitializeMyrrhCounter()
    {
        countText.SetText("{0}", myrrhCount);
        ChangeMyrrhFillSprite();
    }

    void UpdateStatusSliders()
    {
        for(int i = 0; i < hpSliders.Length; i++)
        {
            Character thisCharacter = FindObjectOfType<GameController>().allCharacters[i];

            hpSliders[i].value = (float) thisCharacter.currentHP / thisCharacter.maxHP;
            mpSliders[i].value = (float) thisCharacter.currentMP / thisCharacter.maxMP;
        }
    }

    // Loads a Myrrh value from the Savefile
    public void ForcedSetMyrrh(int setMyrrh)
    {
        myrrhCount = setMyrrh;

        ChangeMyrrhFillSprite();

        countText.SetText("{0}", myrrhCount);
    }

    public IEnumerator GainMyrrh()
    {
        if(myrrhCount < 4)
        {
            FindObjectOfType<GameController>().pauseButton.interactable = false;

            FindObjectOfType<GameController>().DisableJoystick(false);

            GetComponent<Animator>().SetTrigger("AddMyrrh");

            yield return new WaitForSeconds(1.33f);

            myrrhCount++;
            ChangeMyrrhFillSprite();
            countText.SetText("{0}", myrrhCount);

            yield return new WaitForSeconds(1.67f);

            FindObjectOfType<GameController>().pauseButton.interactable = true;
        }
        else // If you don't gain any, just return to idle
        {
            GetComponent<Animator>().SetTrigger("MyrrhEnter");
        }

        // Boss battles always disable the joystick after, so this should run even if no myrrh was gained
        FindObjectOfType<GameController>().EnableJoystick(true);

        // Save the extra Myrrh received
        FindObjectOfType<GameController>().SaveGame();

        yield return null;
    }
    
    void ChangeMyrrhFillSprite()
    {
        // Change the liquid's sprite from 0-4 Myrrh
        if(myrrhCount > 0)
        {
            fillImage.sprite = myrrhSprites[myrrhCount - 1];
        }
        else
        {
            fillImage.sprite = myrrhSprites[0];
        }

        // To indicate fullness, make the count text yellow at maximum Myrrh
        if(myrrhCount == 4)
        {
            countText.color = Color.yellow;
        }
        else
        {
            countText.color = Color.white;
        }
    }
}
